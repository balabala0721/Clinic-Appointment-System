using ClinicSystem.Config;
using ClinicSystem.Data;
using ClinicSystem.Models.Entities;
using ClinicSystem.Models.ViewModels;
using ClinicSystem.Services;
using ClinicSystem.Services.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicSystem.Controllers;

[Authorize]
public class AppointmentsController : Controller
{
    private readonly ClinicDbContext _db;
    private readonly AppointmentService _appointments;
    private readonly SlotService _slots;
    private readonly CurrentUserService _me;

    public AppointmentsController(ClinicDbContext db, AppointmentService appointments, SlotService slots,
        CurrentUserService me)
    {
        _db = db;
        _appointments = appointments;
        _slots = slots;
        _me = me;
    }

    // ------------------------------------------------------------- patient

    [Authorize(Roles = Roles.Patient)]
    public async Task<IActionResult> Book(int? doctorId, DateOnly? date)
    {
        var today = ClinicClock.Today;
        var vm = new BookPageViewModel
        {
            Specialisations = await _db.Specialisations.OrderBy(s => s.Name).AsNoTracking().ToListAsync(),
            Doctors = await _db.Doctors.Include(d => d.Specialisation)
                               .OrderBy(d => d.User.FullName).AsNoTracking().ToListAsync(),
            PreselectedDoctorId = doctorId,
            PreselectedDate = (date.HasValue && date.Value >= today ? date.Value : today).ToString("yyyy-MM-dd"),
            MinDate = today.ToString("yyyy-MM-dd"),
            MaxDate = today.AddDays(ClinicSettings.MaxBookingDaysAhead).ToString("yyyy-MM-dd")
        };
        return View(vm);
    }

    /// <summary>AJAX: free slots for one doctor on one date. Used by Book and reschedule.</summary>
    [HttpGet]
    public async Task<IActionResult> Slots(int doctorId, DateOnly date)
    {
        var slots = await _slots.GetAvailableSlotsAsync(doctorId, date);
        return Json(slots.Select(s => new
        {
            value = s.Start.ToString("HH:mm:ss"),   // sent back when booking
            label = s.Start.ToString("HH:mm")
        }));
    }

    [HttpPost, Authorize(Roles = Roles.Patient), ValidateAntiForgeryToken]
    public async Task<IActionResult> Book([FromBody] BookRequest request)
    {
        if (!ModelState.IsValid) return Json(new { success = false, message = "Please choose a doctor, date and time." });

        var patientId = await _me.GetPatientIdAsync();
        if (patientId == null) return Json(new { success = false, message = "Patient profile not found." });

        var result = await _appointments.BookAsync(patientId.Value, request.DoctorId, request.Date,
            request.StartTime, request.Reason, _me.UserId);
        return Json(new { success = result.Success, message = result.Message });
    }

    [Authorize(Roles = Roles.Patient)]
    public async Task<IActionResult> My()
    {
        var patientId = await _me.GetPatientIdAsync();
        if (patientId == null) return Forbid();
        return View(await _appointments.GetPatientAppointmentsAsync(patientId.Value));
    }

    // ---------------------------------------------- shared: patient + staff

    [HttpPost, Authorize(Roles = Roles.Patient + "," + Roles.StaffOrAdmin), ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel([FromBody] CancelRequest request)
    {
        var owner = await OwnerPatientIdAsync();
        if (owner.Forbidden) return Forbid();
        var result = await _appointments.CancelAsync(request.AppointmentId, request.Reason, owner.PatientId);
        return Json(new { success = result.Success, message = result.Message });
    }

    [HttpPost, Authorize(Roles = Roles.Patient + "," + Roles.StaffOrAdmin), ValidateAntiForgeryToken]
    public async Task<IActionResult> Reschedule([FromBody] RescheduleRequest request)
    {
        if (!ModelState.IsValid) return Json(new { success = false, message = "Please choose a new date and time." });
        var owner = await OwnerPatientIdAsync();
        if (owner.Forbidden) return Forbid();
        var result = await _appointments.RescheduleAsync(request.AppointmentId, request.Date, request.StartTime,
            _me.UserId, owner.PatientId);
        return Json(new { success = result.Success, message = result.Message });
    }

    /// <summary>
    /// Patients may only touch their own appointments (PatientId returned);
    /// staff/admin may touch any (PatientId = null).
    /// </summary>
    private async Task<(bool Forbidden, int? PatientId)> OwnerPatientIdAsync()
    {
        if (_me.IsInRole(Roles.Staff) || _me.IsInRole(Roles.Admin)) return (false, null);
        var patientId = await _me.GetPatientIdAsync();
        return patientId == null ? (true, null) : (false, patientId);
    }

    // --------------------------------------------------------------- admin

    [Authorize(Roles = Roles.StaffOrAdmin)]
    public async Task<IActionResult> Manage(DateOnly? date, int? doctorId, string? patientName, string? status, int page = 1)
    {
        var vm = new ManageAppointmentsViewModel
        {
            Date = date,
            DoctorId = doctorId,
            PatientName = patientName,
            Status = status,
            Page = Math.Max(1, page),
            Appointments = await _appointments.SearchAsync(date, doctorId, patientName, status, page),
            Doctors = await _db.Doctors.OrderBy(d => d.User.FullName).AsNoTracking().ToListAsync()
        };
        return View(vm);
    }

    // -------------------------------------------------------------- doctor

    /// <summary>
    /// Doctor's own appointment list. Defaults to today; "allDates" shows every date.
    /// The doctorId always comes from the login, so a doctor can never see another doctor's patients.
    /// </summary>
    [Authorize(Roles = Roles.Doctor)]
    public async Task<IActionResult> Doctor(DateOnly? date, string? status, bool allDates = false, int page = 1)
    {
        var doctorId = await _me.GetDoctorIdAsync();
        if (doctorId == null) return Forbid();

        var filterDate = allDates ? (DateOnly?)null : (date ?? ClinicClock.Today);
        var vm = new DoctorAppointmentsViewModel
        {
            Date = filterDate,
            AllDates = allDates,
            Status = status,
            Page = Math.Max(1, page),
            Appointments = await _appointments.SearchAsync(filterDate, doctorId, null, status, page)
        };
        return View(vm);
    }

    // ------------------------------------------------------- status (shared)

    [HttpPost, Authorize(Roles = Roles.StaffOrAdmin + "," + Roles.Doctor), ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status, string? returnUrl)
    {
        // Doctors: restricted to their own appointments and doctor-only transitions.
        int? doctorId = null;
        if (_me.IsInRole(Roles.Doctor) && !_me.IsInRole(Roles.Staff) && !_me.IsInRole(Roles.Admin))
        {
            doctorId = await _me.GetDoctorIdAsync();
            if (doctorId == null) return Forbid();
        }

        var result = await _appointments.UpdateStatusAsync(id, status, doctorId);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
        return doctorId.HasValue ? RedirectToAction(nameof(Doctor)) : RedirectToAction(nameof(Manage));
    }
}
