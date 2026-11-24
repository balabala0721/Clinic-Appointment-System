using ClinicSystem.Config;
using ClinicSystem.Data;
using ClinicSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicSystem.Services;

public record TimeSlot(int DoctorId, DateOnly Date, TimeOnly Start, TimeOnly End);

/// <summary>
/// Generates bookable slots from the doctor's weekly timetable.
/// Slots are NOT stored in the database - they're calculated on demand:
///   timetable for that weekday  ->  cut into SlotDurationMinutes pieces
///   minus break time, approved leave, already-booked appointments, past times.
/// </summary>
public class SlotService
{
    private readonly ClinicDbContext _db;

    public SlotService(ClinicDbContext db) => _db = db;

    public Task<List<TimeSlot>> GetAvailableSlotsAsync(int doctorId, DateOnly date) =>
        GetAvailableSlotsAsync(doctorId, date, date);

    /// <summary>
    /// All free slots for a doctor between two dates (inclusive).
    /// Loads the data for the whole range in 3 queries, then works in memory,
    /// so searching 14 days costs the same number of queries as searching 1.
    /// </summary>
    public async Task<List<TimeSlot>> GetAvailableSlotsAsync(int doctorId, DateOnly fromDate, DateOnly toDate)
    {
        var today = ClinicClock.Today;
        var lastBookable = today.AddDays(ClinicSettings.MaxBookingDaysAhead);
        if (fromDate < today) fromDate = today;
        if (toDate > lastBookable) toDate = lastBookable;
        if (fromDate > toDate) return new List<TimeSlot>();

        var doctorActive = await _db.Doctors.AnyAsync(d => d.DoctorId == doctorId);
        if (!doctorActive) return new List<TimeSlot>();

        var schedules = await _db.DoctorSchedules
            .Where(s => s.DoctorId == doctorId && s.IsActive
                     && s.EffectiveFrom <= toDate
                     && (s.EffectiveTo == null || s.EffectiveTo >= fromDate))
            .AsNoTracking().ToListAsync();

        var leaves = await _db.DoctorLeaves
            .Where(l => l.DoctorId == doctorId && l.Status == LeaveStatus.Approved
                     && l.StartDate <= toDate && l.EndDate >= fromDate)
            .AsNoTracking().ToListAsync();

        var booked = await _db.Appointments
            .Where(a => a.DoctorId == doctorId
                     && a.AppointmentDate >= fromDate && a.AppointmentDate <= toDate
                     && !AppointmentStatus.ReleasesSlot.Contains(a.Status))
            .Select(a => new { a.AppointmentDate, a.StartTime, a.EndTime })
            .AsNoTracking().ToListAsync();

        // Earliest start time allowed today (e.g. now + 60 min)
        var earliestToday = ClinicClock.Now.AddMinutes(ClinicSettings.MinBookingLeadMinutes);

        var result = new List<TimeSlot>();
        for (var date = fromDate; date <= toDate; date = date.AddDays(1))
        {
            var schedule = ScheduleRules.ScheduleFor(schedules, date);
            if (schedule == null) continue;

            var bookedToday = booked.Where(b => b.AppointmentDate == date).ToList();
            var step = TimeSpan.FromMinutes(schedule.SlotDurationMinutes);

            // Use TimeSpan for the loop - TimeOnly wraps around midnight and could loop forever.
            for (var cursor = schedule.StartTime.ToTimeSpan();
                 cursor + step <= schedule.EndTime.ToTimeSpan();
                 cursor += step)
            {
                var start = TimeOnly.FromTimeSpan(cursor);
                var end = TimeOnly.FromTimeSpan(cursor + step);

                if (date.ToDateTime(start) < earliestToday) continue;
                if (ScheduleRules.IsInBreak(schedule, start, end)) continue;
                if (ScheduleRules.IsOnLeave(leaves, date, start, end)) continue;
                if (bookedToday.Any(b => ScheduleRules.Overlaps(start, end, b.StartTime, b.EndTime))) continue;

                result.Add(new TimeSlot(doctorId, date, start, end));
            }
        }
        return result;
    }

    /// <summary>Finds the slot that starts at exactly this time, or null if it isn't free.</summary>
    public async Task<TimeSlot?> FindSlotAsync(int doctorId, DateOnly date, TimeOnly start)
    {
        var slots = await GetAvailableSlotsAsync(doctorId, date);
        return slots.FirstOrDefault(s => s.Start == start);
    }
}
