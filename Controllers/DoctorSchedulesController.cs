using ClinicSystem.Config;
using ClinicSystem.Data;
using ClinicSystem.Models.Entities;
using ClinicSystem.Models.ViewModels;
using ClinicSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicSystem.Controllers;

[Authorize]
public class DoctorSchedulesController : Controller
{
    private readonly ClinicDbContext _db;
    private readonly ScheduleService _schedules;
    private readonly CurrentUserService _me;

    public DoctorSchedulesController(ClinicDbContext db, ScheduleService schedules, CurrentUserService me)
    {
        _db = db;
        _schedules = schedules;
        _me = me;
    }

    // ------------------------------------------------------ overview
    // Staff can VIEW schedules and conflicts (to contact and reschedule patients).
    // Only Admin can CHANGE timetables or leave - those affect every booking.

    [Authorize(Roles = Roles.StaffOrAdmin)]
    public async Task<IActionResult> Index(int? specialisationId, string? search)
    {
        var doctors = await _db.Doctors.Include(d => d.Specialisation)
            .Where(d => d.IsActive
                     && (specialisationId == null || d.SpecialisationId == specialisationId)
                     && (search == null || d.FullName.Contains(search)))
            .OrderBy(d => d.FullName).AsNoTracking().ToListAsync();

        var doctorIds = doctors.Select(d => d.DoctorId).ToList();
        var schedules = await _db.DoctorSchedules
            .Where(s => doctorIds.Contains(s.DoctorId) && s.IsActive)
            .AsNoTracking().ToListAsync();

        var vm = new ScheduleIndexViewModel
        {
            Rows = doctors.Select(d => new ScheduleIndexRow
            {
                Doctor = d,
                Schedules = schedules.Where(s => s.DoctorId == d.DoctorId).OrderBy(s => s.DayOfWeek).ToList()
            }).ToList(),
            Specialisations = await _db.Specialisations.OrderBy(s => s.Name).AsNoTracking().ToListAsync(),
            SpecialisationId = specialisationId,
            Search = search,
            ConflictCount = (await _schedules.GetConflictsAsync()).Count,
            PendingLeaveCount = (await _schedules.GetLeavesAsync(LeaveStatus.Pending, null)).Count
        };
        return View(vm);
    }

    // ------------------------------------------- admin: one doctor's timetable

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Edit(int doctorId, int? scheduleId)
    {
        var vm = await BuildEditViewModelAsync(doctorId);
        if (vm == null) return NotFound();

        if (scheduleId.HasValue)
        {
            var row = vm.Schedules.FirstOrDefault(s => s.ScheduleId == scheduleId);
            if (row != null) vm.Form = ScheduleFormModel.From(row);
        }
        return View(vm);
    }

    [HttpPost, Authorize(Roles = Roles.Admin), ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([Bind(Prefix = "Form")] ScheduleFormModel form)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please check the schedule details.";
            return RedirectToAction(nameof(Edit), new { doctorId = form.DoctorId });
        }
        var result = await _schedules.SaveScheduleAsync(form.ToEntity());
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Edit), new { doctorId = form.DoctorId });
    }

    [HttpPost, Authorize(Roles = Roles.Admin), ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int scheduleId, int doctorId)
    {
        var result = await _schedules.DeactivateScheduleAsync(scheduleId);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Edit), new { doctorId });
    }

    /// <summary>Admin records leave directly - approved immediately.</summary>
    [HttpPost, Authorize(Roles = Roles.Admin), ValidateAntiForgeryToken]
    public async Task<IActionResult> AddLeave([Bind(Prefix = "LeaveForm")] LeaveFormModel form)
    {
        var result = await _schedules.SubmitLeaveAsync(form.DoctorId, form.StartDate, form.EndDate,
            form.StartTime, form.EndTime, form.Reason, autoApprove: true, reviewerUserId: _me.UserId);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Edit), new { doctorId = form.DoctorId });
    }

    // ------------------------------------------------- admin: leave + clashes

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Leaves(string status = LeaveStatus.Pending) =>
        View(await _schedules.GetLeavesAsync(status, null));

    [HttpPost, Authorize(Roles = Roles.Admin), ValidateAntiForgeryToken]
    public async Task<IActionResult> ReviewLeave(int leaveId, bool approve)
    {
        var result = await _schedules.ReviewLeaveAsync(leaveId, approve, _me.UserId);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Leaves));
    }

    [Authorize(Roles = Roles.StaffOrAdmin)]
    public async Task<IActionResult> Conflicts() => View(await _schedules.GetConflictsAsync());

    // ------------------------------------------------------- doctor: own week

    [Authorize(Roles = Roles.Doctor)]
    public async Task<IActionResult> Mine(DateOnly? week)
    {
        var doctorId = await _me.GetDoctorIdAsync();
        if (doctorId == null) return Forbid();
        var vm = await BuildWeekViewModelAsync(doctorId.Value, week, includeAppointments: true);
        if (vm == null) return NotFound();
        vm.IsOwnSchedule = true;
        // FR 8.14: doctor sees the status of their own leave requests (current and upcoming)
        vm.MyLeaveRequests = (await _schedules.GetLeavesAsync(null, doctorId.Value))
                             .Where(l => l.EndDate >= ClinicClock.Today)
                             .OrderByDescending(l => l.CreatedAt).ToList();
        return View("Week", vm);
    }

    [HttpPost, Authorize(Roles = Roles.Doctor), ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestLeave([Bind(Prefix = "LeaveForm")] LeaveFormModel form)
    {
        var doctorId = await _me.GetDoctorIdAsync();
        if (doctorId == null) return Forbid();
        // DoctorId always comes from the login, never from the form.
        var result = await _schedules.SubmitLeaveAsync(doctorId.Value, form.StartDate, form.EndDate,
            form.StartTime, form.EndTime, form.Reason, autoApprove: false, reviewerUserId: null);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Mine));
    }

    // --------------------------------------------------- patient: view a doctor

    public async Task<IActionResult> Doctor(int id, DateOnly? week)
    {
        var vm = await BuildWeekViewModelAsync(id, week, includeAppointments: false);
        return vm == null ? NotFound() : View("Week", vm);
    }

    // ----------------------------------------------------------------- helpers

    private async Task<EditScheduleViewModel?> BuildEditViewModelAsync(int doctorId)
    {
        var doctor = await _db.Doctors.Include(d => d.Specialisation).AsNoTracking()
                              .FirstOrDefaultAsync(d => d.DoctorId == doctorId);
        if (doctor == null) return null;
        return new EditScheduleViewModel
        {
            Doctor = doctor,
            Schedules = await _schedules.GetSchedulesAsync(doctorId),
            Leaves = (await _schedules.GetLeavesAsync(null, doctorId)).Where(l => l.EndDate >= ClinicClock.Today).ToList(),
            Form = new ScheduleFormModel { DoctorId = doctorId },
            LeaveForm = new LeaveFormModel { DoctorId = doctorId }
        };
    }

    private async Task<DoctorWeekViewModel?> BuildWeekViewModelAsync(int doctorId, DateOnly? week, bool includeAppointments)
    {
        var doctor = await _db.Doctors.Include(d => d.Specialisation).AsNoTracking()
                              .FirstOrDefaultAsync(d => d.DoctorId == doctorId && d.IsActive);
        if (doctor == null) return null;

        // Week starts on Monday
        var anyDay = week ?? ClinicClock.Today;
        var weekStart = anyDay.AddDays(-(((int)anyDay.DayOfWeek + 6) % 7));
        var weekEnd = weekStart.AddDays(6);

        return new DoctorWeekViewModel
        {
            Doctor = doctor,
            WeekStart = weekStart,
            Schedules = await _schedules.GetSchedulesAsync(doctorId),
            Leaves = (await _schedules.GetLeavesAsync(LeaveStatus.Approved, doctorId))
                     .Where(l => l.StartDate <= weekEnd && l.EndDate >= weekStart).ToList(),
            Appointments = includeAppointments
                ? await _schedules.GetDoctorAppointmentsAsync(doctorId, weekStart, weekEnd)
                : new List<Appointment>()
        };
    }
}
