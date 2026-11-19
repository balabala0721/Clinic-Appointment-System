using System.ComponentModel.DataAnnotations;
using ClinicSystem.Config;
using ClinicSystem.Models.Entities;
using ClinicSystem.Services;
using ClinicSystem.Services.Ai;

namespace ClinicSystem.Models.ViewModels;

// ------------------------------------------------------------- appointments

public class BookPageViewModel
{
    public List<Specialisation> Specialisations { get; set; } = new();
    public List<Doctor> Doctors { get; set; } = new();
    public int? PreselectedDoctorId { get; set; }
    public string PreselectedDate { get; set; } = "";
    public string MinDate { get; set; } = "";
    public string MaxDate { get; set; } = "";
}

// JSON bodies posted by jQuery
public class BookRequest
{
    [Required] public int DoctorId { get; set; }
    [Required] public DateOnly Date { get; set; }
    [Required] public TimeOnly StartTime { get; set; }
    [StringLength(300)] public string? Reason { get; set; }
}

public class RescheduleRequest
{
    [Required] public int AppointmentId { get; set; }
    [Required] public DateOnly Date { get; set; }
    [Required] public TimeOnly StartTime { get; set; }
}

public class CancelRequest
{
    [Required] public int AppointmentId { get; set; }
    [StringLength(300)] public string? Reason { get; set; }
}

public class ManageAppointmentsViewModel
{
    public List<Appointment> Appointments { get; set; } = new();
    public List<Doctor> Doctors { get; set; } = new();
    public DateOnly? Date { get; set; }
    public int? DoctorId { get; set; }
    public string? PatientName { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
}

public class DoctorAppointmentsViewModel
{
    public List<Appointment> Appointments { get; set; } = new();
    public DateOnly? Date { get; set; }
    public bool AllDates { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
}

// -------------------------------------------------------------- schedules

public class ScheduleFormModel
{
    public int ScheduleId { get; set; }
    [Required] public int DoctorId { get; set; }
    [Range(0, 6)] public byte DayOfWeek { get; set; } = 1;
    [Required] public TimeOnly StartTime { get; set; } = new(9, 0);
    [Required] public TimeOnly EndTime { get; set; } = new(17, 0);
    public TimeOnly? BreakStart { get; set; } = new(13, 0);
    public TimeOnly? BreakEnd { get; set; } = new(14, 0);
    [Range(5, 240)] public int SlotDurationMinutes { get; set; } = ClinicSettings.DefaultSlotDurationMinutes;
    [Required] public DateOnly EffectiveFrom { get; set; } = ClinicClock.Today;
    public DateOnly? EffectiveTo { get; set; }

    public DoctorSchedule ToEntity() => new()
    {
        ScheduleId = ScheduleId,
        DoctorId = DoctorId,
        DayOfWeek = DayOfWeek,
        StartTime = StartTime,
        EndTime = EndTime,
        BreakStart = BreakStart,
        BreakEnd = BreakEnd,
        SlotDurationMinutes = SlotDurationMinutes,
        EffectiveFrom = EffectiveFrom,
        EffectiveTo = EffectiveTo
    };

    public static ScheduleFormModel From(DoctorSchedule s) => new()
    {
        ScheduleId = s.ScheduleId,
        DoctorId = s.DoctorId,
        DayOfWeek = s.DayOfWeek,
        StartTime = s.StartTime,
        EndTime = s.EndTime,
        BreakStart = s.BreakStart,
        BreakEnd = s.BreakEnd,
        SlotDurationMinutes = s.SlotDurationMinutes,
        EffectiveFrom = s.EffectiveFrom,
        EffectiveTo = s.EffectiveTo
    };
}

public class LeaveFormModel
{
    public int DoctorId { get; set; }
    [Required] public DateOnly StartDate { get; set; } = ClinicClock.Today;
    [Required] public DateOnly EndDate { get; set; } = ClinicClock.Today;
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    [StringLength(300)] public string? Reason { get; set; }
}

public class ScheduleIndexRow
{
    public Doctor Doctor { get; set; } = null!;
    public List<DoctorSchedule> Schedules { get; set; } = new();
}

public class ScheduleIndexViewModel
{
    public List<ScheduleIndexRow> Rows { get; set; } = new();
    public List<Specialisation> Specialisations { get; set; } = new();
    public int? SpecialisationId { get; set; }
    public string? Search { get; set; }
    public int ConflictCount { get; set; }
    public int PendingLeaveCount { get; set; }
}

public class EditScheduleViewModel
{
    public Doctor Doctor { get; set; } = null!;
    public List<DoctorSchedule> Schedules { get; set; } = new();
    public List<DoctorLeave> Leaves { get; set; } = new();
    public ScheduleFormModel Form { get; set; } = new();
    public LeaveFormModel LeaveForm { get; set; } = new();
}

public class DoctorWeekViewModel
{
    public Doctor Doctor { get; set; } = null!;
    public List<DoctorSchedule> Schedules { get; set; } = new();
    public List<DoctorLeave> Leaves { get; set; } = new();
    public List<Appointment> Appointments { get; set; } = new();   // empty for patients
    public List<DoctorLeave> MyLeaveRequests { get; set; } = new(); // doctor's own requests, any status
    public DateOnly WeekStart { get; set; }
    public bool IsOwnSchedule { get; set; }
    public LeaveFormModel LeaveForm { get; set; } = new();
}

// ------------------------------------------------------------------- AI

public class AssistantChatRequest
{
    [Required, StringLength(AiSettings.MaxUserMessageLength)] public string Message { get; set; } = "";
    public List<ChatMessage> History { get; set; } = new();
}

public class RecommendRequest
{
    [Required, StringLength(AiSettings.MaxSymptomLength, MinimumLength = 3)] public string Symptoms { get; set; } = "";
}
