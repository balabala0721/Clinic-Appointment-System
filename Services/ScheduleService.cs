using ClinicSystem.Config;
using ClinicSystem.Data;
using ClinicSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicSystem.Services;

public record ScheduleConflict(Appointment Appointment, string Reason);

/// <summary>Doctor timetables, leave requests, and clash detection.</summary>
public class ScheduleService
{
    private readonly ClinicDbContext _db;

    public ScheduleService(ClinicDbContext db) => _db = db;

    // ------------------------------------------------------------ timetables

    public Task<List<DoctorSchedule>> GetSchedulesAsync(int doctorId, bool activeOnly = true) =>
        _db.DoctorSchedules
           .Where(s => s.DoctorId == doctorId && (!activeOnly || s.IsActive))
           .OrderBy(s => s.DayOfWeek).ThenBy(s => s.EffectiveFrom)
           .AsNoTracking().ToListAsync();

    /// <summary>Create (ScheduleId = 0) or update a weekly timetable row.</summary>
    public async Task<ServiceResult> SaveScheduleAsync(DoctorSchedule input)
    {
        var error = Validate(input);
        if (error != null) return ServiceResult.Fail(error);

        // Two active rows for the same weekday with overlapping date ranges would be ambiguous.
        var overlapping = await _db.DoctorSchedules.AnyAsync(s =>
            s.DoctorId == input.DoctorId &&
            s.DayOfWeek == input.DayOfWeek &&
            s.IsActive &&
            s.ScheduleId != input.ScheduleId &&
            s.EffectiveFrom <= (input.EffectiveTo ?? DateOnly.MaxValue) &&
            (s.EffectiveTo == null || s.EffectiveTo >= input.EffectiveFrom));
        if (overlapping)
            return ServiceResult.Fail("This doctor already has a timetable for that day in the same date range. " +
                                      "Edit that one, or set an end date on it first.");

        if (input.ScheduleId == 0)
        {
            input.IsActive = true;
            _db.DoctorSchedules.Add(input);
        }
        else
        {
            var existing = await _db.DoctorSchedules.FirstOrDefaultAsync(s => s.ScheduleId == input.ScheduleId);
            if (existing == null) return ServiceResult.Fail("Schedule not found.");
            existing.DayOfWeek = input.DayOfWeek;
            existing.StartTime = input.StartTime;
            existing.EndTime = input.EndTime;
            existing.BreakStart = input.BreakStart;
            existing.BreakEnd = input.BreakEnd;
            existing.SlotDurationMinutes = input.SlotDurationMinutes;
            existing.EffectiveFrom = input.EffectiveFrom;
            existing.EffectiveTo = input.EffectiveTo;
        }

        await _db.SaveChangesAsync();
        var conflicts = await GetConflictsAsync(input.DoctorId);
        return ServiceResult.Ok(conflicts.Count == 0
            ? "Schedule saved."
            : $"Schedule saved. {conflicts.Count} existing appointment(s) now clash - see Conflicts.");
    }

    /// <summary>Soft delete - old appointments keep pointing at a valid history.</summary>
    public async Task<ServiceResult> DeactivateScheduleAsync(int scheduleId)
    {
        var s = await _db.DoctorSchedules.FirstOrDefaultAsync(x => x.ScheduleId == scheduleId);
        if (s == null) return ServiceResult.Fail("Schedule not found.");
        s.IsActive = false;
        await _db.SaveChangesAsync();
        return ServiceResult.Ok("Schedule removed.");
    }

    private static string? Validate(DoctorSchedule s)
    {
        if (s.DayOfWeek > 6) return "Invalid day.";
        if (s.EndTime <= s.StartTime) return "End time must be after start time.";
        if (s.SlotDurationMinutes <= 0) return "Slot duration must be more than 0 minutes.";
        if ((s.EndTime - s.StartTime).TotalMinutes < s.SlotDurationMinutes) return "Working hours are shorter than one slot.";
        if (s.BreakStart.HasValue != s.BreakEnd.HasValue) return "Set both break start and break end, or neither.";
        if (s.BreakStart.HasValue &&
            (s.BreakStart < s.StartTime || s.BreakEnd > s.EndTime || s.BreakEnd <= s.BreakStart))
            return "Break must be inside working hours.";
        if (s.EffectiveTo.HasValue && s.EffectiveTo < s.EffectiveFrom) return "End date must be after start date.";
        return null;
    }

    // ---------------------------------------------------------------- leave

    public async Task<ServiceResult> SubmitLeaveAsync(int doctorId, DateOnly start, DateOnly end,
        TimeOnly? startTime, TimeOnly? endTime, string? reason, bool autoApprove, int? reviewerUserId)
    {
        if (end < start) return ServiceResult.Fail("End date must be on or after start date.");
        if (start < ClinicClock.Today) return ServiceResult.Fail("Leave can't start in the past.");
        if (startTime.HasValue != endTime.HasValue) return ServiceResult.Fail("Set both times for partial-day leave, or neither.");
        if (startTime.HasValue && endTime <= startTime) return ServiceResult.Fail("Leave end time must be after start time.");

        _db.DoctorLeaves.Add(new DoctorLeave
        {
            DoctorId = doctorId,
            StartDate = start,
            EndDate = end,
            StartTime = startTime,
            EndTime = endTime,
            Reason = reason?.Trim(),
            Status = autoApprove ? LeaveStatus.Approved : LeaveStatus.Pending,   // admin-recorded leave skips approval
            ReviewedByUserId = autoApprove ? reviewerUserId : null,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return ServiceResult.Ok(autoApprove ? "Leave recorded." : "Leave request submitted for approval.");
    }

    public async Task<ServiceResult> ReviewLeaveAsync(int leaveId, bool approve, int reviewerUserId)
    {
        var leave = await _db.DoctorLeaves.FirstOrDefaultAsync(l => l.LeaveId == leaveId);
        if (leave == null) return ServiceResult.Fail("Leave request not found.");
        if (leave.Status != LeaveStatus.Pending) return ServiceResult.Fail("This request was already reviewed.");

        leave.Status = approve ? LeaveStatus.Approved : LeaveStatus.Rejected;
        leave.ReviewedByUserId = reviewerUserId;
        await _db.SaveChangesAsync();

        if (!approve) return ServiceResult.Ok("Leave rejected.");
        var conflicts = await GetConflictsAsync(leave.DoctorId);
        return ServiceResult.Ok(conflicts.Count == 0
            ? "Leave approved."
            : $"Leave approved. {conflicts.Count} appointment(s) need rescheduling - see Conflicts.");
    }

    public Task<List<DoctorLeave>> GetLeavesAsync(string? status, int? doctorId) =>
        _db.DoctorLeaves
           .Include(l => l.Doctor)
           .Where(l => (status == null || l.Status == status) && (doctorId == null || l.DoctorId == doctorId))
           .OrderBy(l => l.StartDate)
           .AsNoTracking().ToListAsync();

    // ------------------------------------------------------------ conflicts

    /// <summary>
    /// Upcoming active appointments that no longer fit the doctor's timetable
    /// (outside new hours, in a new break, or during approved leave).
    /// Calculated live, so there's no "conflict" column that could go stale.
    /// </summary>
    public async Task<List<ScheduleConflict>> GetConflictsAsync(int? doctorId = null)
    {
        var today = ClinicClock.Today;

        var appointments = await _db.Appointments
            .Include(a => a.Patient).Include(a => a.Doctor)
            .Where(a => a.AppointmentDate >= today
                     && AppointmentStatus.Changeable.Contains(a.Status)
                     && (doctorId == null || a.DoctorId == doctorId))
            .AsNoTracking().ToListAsync();
        if (appointments.Count == 0) return new List<ScheduleConflict>();

        var doctorIds = appointments.Select(a => a.DoctorId).Distinct().ToList();
        var schedules = await _db.DoctorSchedules
            .Where(s => doctorIds.Contains(s.DoctorId) && s.IsActive).AsNoTracking().ToListAsync();
        var leaves = await _db.DoctorLeaves
            .Where(l => doctorIds.Contains(l.DoctorId) && l.Status == LeaveStatus.Approved && l.EndDate >= today)
            .AsNoTracking().ToListAsync();

        var conflicts = new List<ScheduleConflict>();
        foreach (var a in appointments)
        {
            var docSchedules = schedules.Where(s => s.DoctorId == a.DoctorId).ToList();
            var docLeaves = leaves.Where(l => l.DoctorId == a.DoctorId).ToList();

            if (ScheduleRules.IsOnLeave(docLeaves, a.AppointmentDate, a.StartTime, a.EndTime))
                conflicts.Add(new ScheduleConflict(a, "Doctor on leave"));
            else if (!ScheduleRules.IsWorkingTime(docSchedules, docLeaves, a.AppointmentDate, a.StartTime, a.EndTime))
                conflicts.Add(new ScheduleConflict(a, "Outside doctor's working hours"));
        }
        return conflicts.OrderBy(c => c.Appointment.StartsAt).ToList();
    }

    // -------------------------------------------------------- doctor's view

    public Task<List<Appointment>> GetDoctorAppointmentsAsync(int doctorId, DateOnly from, DateOnly to) =>
        _db.Appointments
           .Include(a => a.Patient)
           .Where(a => a.DoctorId == doctorId
                    && a.AppointmentDate >= from && a.AppointmentDate <= to
                    && !AppointmentStatus.ReleasesSlot.Contains(a.Status))
           .OrderBy(a => a.AppointmentDate).ThenBy(a => a.StartTime)
           .AsNoTracking().ToListAsync();
}
