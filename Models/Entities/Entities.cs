using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClinicSystem.Models.Account;

namespace ClinicSystem.Models.Entities;

// CODE-FIRST: these classes ARE the database design. EF Core migrations turn them into tables.
// To change the database: edit a class -> Add-Migration <Name> -> Update-Database.
// [MaxLength(n)] = NVARCHAR(n). Non-nullable string = NOT NULL; string? = NULL allowed.
// Other modules' tables are in OtherModuleEntities.cs.

public static class Roles
{
    public const string Patient = "Patient";
    public const string Doctor = "Doctor";
    public const string Staff = "Staff";
    public const string Admin = "Admin";
    public const string StaffOrAdmin = Staff + "," + Admin;   // for [Authorize(Roles = ...)]
}

public static class AppointmentStatus
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string CheckedIn = "CheckedIn";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
    public const string NoShow = "NoShow";

    public static readonly string[] All = { Pending, Confirmed, CheckedIn, Completed, Cancelled, NoShow };

    // Statuses that no longer hold the slot. Must match the WHERE of UX_Appointments_DoctorSlot.
    public static readonly string[] ReleasesSlot = { Cancelled, NoShow };

    // Statuses a patient can still cancel or reschedule
    public static readonly string[] Changeable = { Pending, Confirmed };

    // Allowed status changes for staff. Anything not listed is rejected.
    public static readonly Dictionary<string, string[]> AllowedTransitions = new()
    {
        [Pending]   = new[] { Confirmed, Cancelled },
        [Confirmed] = new[] { CheckedIn, Cancelled, NoShow },
        [CheckedIn] = new[] { Completed },
        [Completed] = Array.Empty<string>(),
        [Cancelled] = Array.Empty<string>(),
        [NoShow]    = Array.Empty<string>(),
    };

    // Doctors only record the outcome of their own consultations.
    public static readonly Dictionary<string, string[]> DoctorTransitions = new()
    {
        [Confirmed] = new[] { Completed, NoShow },
        [CheckedIn] = new[] { Completed },
    };

    // Outcomes that can only be recorded once the appointment day has arrived.
    public static readonly string[] OutcomeStatuses = { Completed, NoShow };

    public static string[] NextFor(string current, bool isDoctor) =>
        (isDoctor ? DoctorTransitions : AllowedTransitions).TryGetValue(current, out var next) ? next : Array.Empty<string>();
}

public static class LeaveStatus
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
}

public class DoctorSchedule
{
    public int ScheduleId { get; set; }
    public int DoctorId { get; set; }
    public byte DayOfWeek { get; set; }            // 0 = Sunday ... 6 = Saturday (same as System.DayOfWeek)
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public TimeOnly? BreakStart { get; set; }
    public TimeOnly? BreakEnd { get; set; }
    public int SlotDurationMinutes { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;

    public Doctor? Doctor { get; set; }

    public bool AppliesOn(DateOnly date) =>
        IsActive
        && DayOfWeek == (byte)date.DayOfWeek
        && EffectiveFrom <= date
        && (EffectiveTo == null || EffectiveTo >= date);
}

public class DoctorLeave
{
    public int LeaveId { get; set; }
    public int DoctorId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly? StartTime { get; set; }       // null = whole day(s)
    public TimeOnly? EndTime { get; set; }
    [MaxLength(300)] public string? Reason { get; set; }
    [MaxLength(20)] public string Status { get; set; } = LeaveStatus.Pending;
    public int? ReviewedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Doctor? Doctor { get; set; }

    public bool IsWholeDay => StartTime == null || EndTime == null;
}

public class Appointment
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    [MaxLength(300)] public string? ReasonForVisit { get; set; }
    [MaxLength(20)] public string Status { get; set; } = AppointmentStatus.Pending;
    [MaxLength(300)] public string? CancelReason { get; set; }
    public int? RescheduledFromId { get; set; }      // previous appointment, if this one replaced it
    public int BookedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Patient? Patient { get; set; }
    public Doctor? Doctor { get; set; }

    public DateTime StartsAt => AppointmentDate.ToDateTime(StartTime);
}
