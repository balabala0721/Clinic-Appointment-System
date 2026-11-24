using ClinicSystem.Config;
using ClinicSystem.Data;
using ClinicSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicSystem.Services;

/// <summary>
/// All booking rules live here, so the normal booking page, the admin panel and
/// the AI assistant's confirm buttons all go through exactly the same checks.
/// </summary>
public class AppointmentService
{
    private readonly ClinicDbContext _db;
    private readonly SlotService _slots;

    public AppointmentService(ClinicDbContext db, SlotService slots)
    {
        _db = db;
        _slots = slots;
    }

    public async Task<ServiceResult<Appointment>> BookAsync(
        int patientId, int doctorId, DateOnly date, TimeOnly start, string? reason, int bookedByUserId)
    {
        var slot = await _slots.FindSlotAsync(doctorId, date, start);
        if (slot == null)
            return ServiceResult<Appointment>.Fail("That time slot is not available. Please choose another.");

        var activeCount = await _db.Appointments.CountAsync(a =>
            a.PatientId == patientId &&
            AppointmentStatus.Changeable.Contains(a.Status) &&
            a.AppointmentDate >= ClinicClock.Today);
        if (activeCount >= ClinicSettings.MaxActiveAppointmentsPerPatient)
            return ServiceResult<Appointment>.Fail(
                $"You already have {ClinicSettings.MaxActiveAppointmentsPerPatient} upcoming appointments. Cancel one before booking another.");

        if (await PatientHasClashAsync(patientId, date, slot.Start, slot.End, ignoreAppointmentId: null))
            return ServiceResult<Appointment>.Fail("You already have another appointment at that time.");

        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            AppointmentDate = date,
            StartTime = slot.Start,
            EndTime = slot.End,
            ReasonForVisit = Trim(reason, 300),
            Status = ClinicSettings.AutoConfirmBookings ? AppointmentStatus.Confirmed : AppointmentStatus.Pending,
            BookedByUserId = bookedByUserId,
            CreatedAt = DateTime.UtcNow
        };
        _db.Appointments.Add(appointment);

        if (!await TrySaveAsync())
            return ServiceResult<Appointment>.Fail("Sorry, someone just booked that slot. Please choose another.");

        return ServiceResult<Appointment>.Ok(appointment, "Appointment booked.");
    }

    /// <summary>
    /// Reschedule = cancel the old appointment + create a new one linked by RescheduledFromId,
    /// inside one transaction so the patient never ends up with both or neither.
    /// </summary>
    public async Task<ServiceResult<Appointment>> RescheduleAsync(
        int appointmentId, DateOnly newDate, TimeOnly newStart, int actingUserId, int? ownerPatientId)
    {
        var old = await _db.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
        var check = CheckCanChange(old, ownerPatientId);
        if (!check.Success) return ServiceResult<Appointment>.Fail(check.Message);

        var slot = await _slots.FindSlotAsync(old!.DoctorId, newDate, newStart);
        if (slot == null)
            return ServiceResult<Appointment>.Fail("The new time slot is not available.");

        if (await PatientHasClashAsync(old.PatientId, newDate, slot.Start, slot.End, ignoreAppointmentId: old.AppointmentId))
            return ServiceResult<Appointment>.Fail("You already have another appointment at that time.");

        await using var tx = await _db.Database.BeginTransactionAsync();

        old.Status = AppointmentStatus.Cancelled;
        old.CancelReason = "Rescheduled";
        old.UpdatedAt = DateTime.UtcNow;

        var replacement = new Appointment
        {
            PatientId = old.PatientId,
            DoctorId = old.DoctorId,
            AppointmentDate = newDate,
            StartTime = slot.Start,
            EndTime = slot.End,
            ReasonForVisit = old.ReasonForVisit,
            Status = ClinicSettings.AutoConfirmBookings ? AppointmentStatus.Confirmed : AppointmentStatus.Pending,
            RescheduledFromId = old.AppointmentId,
            BookedByUserId = actingUserId,
            CreatedAt = DateTime.UtcNow
        };
        _db.Appointments.Add(replacement);

        // If the save fails, 'tx' is disposed without Commit, so the old appointment's
        // cancellation is rolled back too.

        if (!await TrySaveAsync())
            return ServiceResult<Appointment>.Fail("Sorry, someone just booked that slot. Please choose another.");

        await tx.CommitAsync();
        return ServiceResult<Appointment>.Ok(replacement, "Appointment rescheduled.");
    }

    public async Task<ServiceResult> CancelAsync(int appointmentId, string? reason, int? ownerPatientId)
    {
        var appt = await _db.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
        var check = CheckCanChange(appt, ownerPatientId);
        if (!check.Success) return check;

        appt!.Status = AppointmentStatus.Cancelled;
        appt.CancelReason = Trim(reason, 300) ?? "Cancelled by " + (ownerPatientId == null ? "clinic" : "patient");
        appt.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ServiceResult.Ok("Appointment cancelled.");
    }

    /// <summary>
    /// Status change by staff/admin (doctorId = null) or by a doctor (doctorId = the logged-in doctor,
    /// who may only update their OWN appointments, and only to Completed / No-show).
    /// </summary>
    public async Task<ServiceResult> UpdateStatusAsync(int appointmentId, string newStatus, int? doctorId = null)
    {
        var appt = await _db.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
        if (appt == null) return ServiceResult.Fail("Appointment not found.");
        if (doctorId.HasValue && appt.DoctorId != doctorId.Value)
            return ServiceResult.Fail("Appointment not found.");   // don't reveal other doctors' appointments

        var allowed = AppointmentStatus.NextFor(appt.Status, isDoctor: doctorId.HasValue);
        if (!allowed.Contains(newStatus))
            return ServiceResult.Fail($"Cannot change status from {appt.Status} to {newStatus}.");

        if (AppointmentStatus.OutcomeStatuses.Contains(newStatus) && appt.AppointmentDate > ClinicClock.Today)
            return ServiceResult.Fail($"Can't mark a future appointment as {newStatus}.");

        appt.Status = newStatus;
        appt.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ServiceResult.Ok($"Status changed to {newStatus}.");
    }

    public Task<List<Appointment>> GetPatientAppointmentsAsync(int patientId) =>
        _db.Appointments
           .Include(a => a.Doctor).ThenInclude(d => d!.Specialisation)
           .Where(a => a.PatientId == patientId)
           .OrderByDescending(a => a.AppointmentDate).ThenByDescending(a => a.StartTime)
           .AsNoTracking().ToListAsync();

    public Task<List<Appointment>> GetUpcomingForPatientAsync(int patientId)
    {
        var today = ClinicClock.Today;
        return _db.Appointments
           .Include(a => a.Doctor)
           .Where(a => a.PatientId == patientId && a.AppointmentDate >= today
                    && AppointmentStatus.Changeable.Contains(a.Status))
           .OrderBy(a => a.AppointmentDate).ThenBy(a => a.StartTime)
           .AsNoTracking().ToListAsync();
    }

    /// <summary>Admin search. Every filter is optional.</summary>
    public async Task<List<Appointment>> SearchAsync(DateOnly? date, int? doctorId, string? patientName, string? status, int page)
    {
        var query = _db.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .AsNoTracking()
            .AsQueryable();

        if (date.HasValue) query = query.Where(a => a.AppointmentDate == date.Value);
        if (doctorId.HasValue) query = query.Where(a => a.DoctorId == doctorId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(a => a.Status == status);
        if (!string.IsNullOrWhiteSpace(patientName))
            query = query.Where(a => a.Patient!.User.FullName.Contains(patientName));

        return await query
            .OrderBy(a => a.AppointmentDate).ThenBy(a => a.StartTime)
            .Skip(Math.Max(0, page - 1) * ClinicSettings.AdminPageSize)
            .Take(ClinicSettings.AdminPageSize)
            .ToListAsync();
    }

    // ---------------------------------------------------------------- helpers

    /// <summary>
    /// ownerPatientId = the logged-in patient (enforces "only your own" + the cancel window).
    /// Pass null for staff, who can change any appointment at any time.
    /// </summary>
    private static ServiceResult CheckCanChange(Appointment? appt, int? ownerPatientId)
    {
        if (appt == null) return ServiceResult.Fail("Appointment not found.");
        if (ownerPatientId.HasValue && appt.PatientId != ownerPatientId.Value)
            return ServiceResult.Fail("Appointment not found.");   // don't reveal that it exists
        if (!AppointmentStatus.Changeable.Contains(appt.Status))
            return ServiceResult.Fail($"A {appt.Status.ToLower()} appointment can't be changed.");
        if (appt.StartsAt <= ClinicClock.Now)
            return ServiceResult.Fail("This appointment has already started.");
        if (ownerPatientId.HasValue && appt.StartsAt < ClinicClock.Now.AddHours(ClinicSettings.MinCancelHoursBefore))
            return ServiceResult.Fail(
                $"Appointments can't be changed online less than {ClinicSettings.MinCancelHoursBefore} hours before. Please call the clinic.");
        return ServiceResult.Ok();
    }

    private Task<bool> PatientHasClashAsync(int patientId, DateOnly date, TimeOnly start, TimeOnly end, int? ignoreAppointmentId) =>
        _db.Appointments.AnyAsync(a =>
            a.PatientId == patientId &&
            a.AppointmentDate == date &&
            a.AppointmentId != (ignoreAppointmentId ?? 0) &&
            !AppointmentStatus.ReleasesSlot.Contains(a.Status) &&
            a.StartTime < end && start < a.EndTime);

    /// <summary>
    /// SaveChanges that returns false when the unique slot index rejects the insert -
    /// i.e. two people booked the same slot at the same moment and we lost the race.
    /// </summary>
    private async Task<bool> TrySaveAsync()
    {
        try
        {
            await _db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            // Detach the failed rows so the context can be reused in this request.
            foreach (var entry in _db.ChangeTracker.Entries().ToList())
                entry.State = EntityState.Detached;
            return false;
        }
    }

    private static string? Trim(string? text, int max) =>
        string.IsNullOrWhiteSpace(text) ? null : text.Trim()[..Math.Min(text.Trim().Length, max)];
}
