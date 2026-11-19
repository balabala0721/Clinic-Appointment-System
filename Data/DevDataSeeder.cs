using ClinicSystem.Config;
using ClinicSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicSystem.Data;

/// <summary>
/// TEMPORARY - fills an empty database with fake demo data so every feature can be tested.
/// Runs only in Development (see Program.cs). Remove when the real Auth/Patient/Doctor modules are merged.
///
/// Dates are relative to "today", so the demo always has upcoming appointments,
/// a past visit, a pending leave request and one schedule conflict to look at.
/// </summary>
public static class DevDataSeeder
{
    // Marks demo accounts. The temporary login page doesn't check passwords.
    public const string DemoPasswordHash = "DEV-ONLY-NO-PASSWORD";
    public const string DemoEmailDomain = "@clinic.test";

    public static async Task SeedAsync(ClinicDbContext db)
    {
        if (await db.Users.AnyAsync()) return;   // never touch a database that already has users

        // ---------------------------------------------------------- specialisations
        var gp = new Specialisation { Name = ClinicSettings.FallbackSpecialisation };   // "General Practice"
        var paeds = new Specialisation { Name = "Paediatrics" };
        var derm = new Specialisation { Name = "Dermatology" };
        db.Specialisations.AddRange(gp, paeds, derm);

        // ------------------------------------------------------------------ users
        User NewUser(string email, string role) => new()
        {
            Email = email + DemoEmailDomain,
            PasswordHash = DemoPasswordHash,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        var admin = NewUser("admin", Roles.Admin);
        var staff = NewUser("staff", Roles.Staff);
        var uTan = NewUser("dr.tan", Roles.Doctor);
        var uLim = NewUser("dr.lim", Roles.Doctor);
        var uRaj = NewUser("dr.rajesh", Roles.Doctor);
        var uAis = NewUser("dr.aisyah", Roles.Doctor);
        var uAli = NewUser("ali", Roles.Patient);
        var uMei = NewUser("meiling", Roles.Patient);
        var uSiva = NewUser("siva", Roles.Patient);
        db.Users.AddRange(admin, staff, uTan, uLim, uRaj, uAis, uAli, uMei, uSiva);
        await db.SaveChangesAsync();   // need the generated UserIds below

        // ---------------------------------------------------------------- doctors
        var tan = new Doctor { UserId = uTan.UserId, SpecialisationId = gp.SpecialisationId, FullName = "Dr Tan Wei Ming", ConsultationFee = 40 };
        var lim = new Doctor { UserId = uLim.UserId, SpecialisationId = gp.SpecialisationId, FullName = "Dr Lim Mei Ling", ConsultationFee = 40 };
        var raj = new Doctor { UserId = uRaj.UserId, SpecialisationId = paeds.SpecialisationId, FullName = "Dr Rajesh Kumar", ConsultationFee = 60 };
        var ais = new Doctor { UserId = uAis.UserId, SpecialisationId = derm.SpecialisationId, FullName = "Dr Nur Aisyah", ConsultationFee = 80 };
        db.Doctors.AddRange(tan, lim, raj, ais);

        // --------------------------------------------------------------- patients
        var ali = new Patient { UserId = uAli.UserId, FullName = "Ali bin Ahmad", Phone = "012-345 6789" };
        var mei = new Patient { UserId = uMei.UserId, FullName = "Wong Mei Ling", Phone = "013-222 3344" };
        var siva = new Patient { UserId = uSiva.UserId, FullName = "Siva Raman", Phone = "014-555 6677" };
        db.Patients.AddRange(ali, mei, siva);
        await db.SaveChangesAsync();

        // -------------------------------------------------------------- schedules
        var today = ClinicClock.Today;
        var from = today.AddDays(-60);   // started in the past, so past appointments are valid too

        void Weekly(Doctor d, byte day, int startH, int endH, int? breakH, int slotMin) =>
            db.DoctorSchedules.Add(new DoctorSchedule
            {
                DoctorId = d.DoctorId,
                DayOfWeek = day,
                StartTime = new TimeOnly(startH, 0),
                EndTime = new TimeOnly(endH, 0),
                BreakStart = breakH.HasValue ? new TimeOnly(breakH.Value, 0) : null,
                BreakEnd = breakH.HasValue ? new TimeOnly(breakH.Value + 1, 0) : null,
                SlotDurationMinutes = slotMin,
                EffectiveFrom = from,
                IsActive = true
            });

        for (byte day = 1; day <= 5; day++)          // Monday - Friday
        {
            Weekly(tan, day, 9, 17, 13, 15);         // GP, day shift
            Weekly(lim, day, 14, 21, 18, 15);        // GP, evening shift
            Weekly(raj, day, 9, 13, null, 20);       // Paediatrics, mornings
        }
        Weekly(tan, 6, 9, 12, null, 15);             // Saturday morning
        Weekly(ais, 2, 10, 16, 12, 30);              // Dermatology, Tuesday
        Weekly(ais, 4, 10, 16, 12, 30);              // Dermatology, Thursday
        await db.SaveChangesAsync();

        await AddSampleAppointmentsAsync(db);
    }

    /// <summary>
    /// Appointments + leave. Also used by "Reset demo data" on the login page.
    /// </summary>
    public static async Task AddSampleAppointmentsAsync(ClinicDbContext db)
    {
        var today = ClinicClock.Today;
        var tan = await db.Doctors.FirstAsync(d => d.FullName == "Dr Tan Wei Ming");
        var lim = await db.Doctors.FirstAsync(d => d.FullName == "Dr Lim Mei Ling");
        var raj = await db.Doctors.FirstAsync(d => d.FullName == "Dr Rajesh Kumar");
        var ali = await db.Patients.FirstAsync(p => p.FullName == "Ali bin Ahmad");
        var mei = await db.Patients.FirstAsync(p => p.FullName == "Wong Mei Ling");
        var siva = await db.Patients.FirstAsync(p => p.FullName == "Siva Raman");
        var adminId = await db.Users.Where(u => u.Role == Roles.Admin).Select(u => u.UserId).FirstAsync();

        Appointment Appt(Patient p, Doctor d, DateOnly date, int h, int m, int lengthMin, string status, string reason) => new()
        {
            PatientId = p.PatientId,
            DoctorId = d.DoctorId,
            AppointmentDate = date,
            StartTime = new TimeOnly(h, m),
            EndTime = new TimeOnly(h, m).AddMinutes(lengthMin),
            Status = status,
            ReasonForVisit = reason,
            BookedByUserId = adminId,
            CreatedAt = DateTime.UtcNow
        };

        var day1 = NextWeekday(today, 1);   // next working day
        var day2 = NextWeekday(today, 3);
        var day5 = NextWeekday(today, 5);   // this one gets leave -> conflict
        var past = PreviousWeekday(today, 7);

        db.Appointments.AddRange(
            // Ali: one upcoming (try cancel / reschedule), one past completed
            Appt(ali, tan, day1, 10, 0, 15, AppointmentStatus.Confirmed, "Follow-up for cough"),
            Appt(ali, tan, past, 9, 30, 15, AppointmentStatus.Completed, "Fever"),
            // Mei: upcoming paediatrics + evening GP
            Appt(mei, raj, day2, 9, 20, 20, AppointmentStatus.Confirmed, "Child vaccination"),
            Appt(mei, lim, day1, 15, 0, 15, AppointmentStatus.Pending, "Headache"),
            // Siva: booked on a day Dr Tan will be on leave -> shows under Conflicts
            Appt(siva, tan, day5, 11, 0, 15, AppointmentStatus.Confirmed, "Blood pressure check"),
            // Yesterday's appointment with no outcome yet -> log in as Dr Tan and mark Completed / No-show
            Appt(mei, tan, PreviousWeekday(today, 1), 14, 30, 15, AppointmentStatus.Confirmed, "Sore throat"),
            // A past no-show for the admin list
            Appt(siva, lim, past, 16, 0, 15, AppointmentStatus.NoShow, "Back pain"));

        db.DoctorLeaves.AddRange(
            // Approved leave over Siva's appointment -> conflict
            new DoctorLeave { DoctorId = tan.DoctorId, StartDate = day5, EndDate = day5, Reason = "Medical conference",
                              Status = LeaveStatus.Approved, ReviewedByUserId = adminId, CreatedAt = DateTime.UtcNow },
            // Pending request for the admin to approve/reject
            new DoctorLeave { DoctorId = lim.DoctorId, StartDate = NextWeekday(today, 8), EndDate = NextWeekday(today, 9),
                              Reason = "Annual leave", Status = LeaveStatus.Pending, CreatedAt = DateTime.UtcNow });

        await db.SaveChangesAsync();
    }

    /// <summary>Deletes all appointments and leave, then re-adds the samples. Users/doctors/schedules stay.</summary>
    public static async Task ResetAppointmentsAsync(ClinicDbContext db)
    {
        // Appointments point at each other (RescheduledFromId), so clear that link before deleting.
        var appts = await db.Appointments.ToListAsync();
        foreach (var a in appts) a.RescheduledFromId = null;
        await db.SaveChangesAsync();

        db.Appointments.RemoveRange(appts);
        db.DoctorLeaves.RemoveRange(await db.DoctorLeaves.ToListAsync());
        await db.SaveChangesAsync();

        await AddSampleAppointmentsAsync(db);
    }

    // n-th working day (Mon-Fri) after / before 'from'
    private static DateOnly NextWeekday(DateOnly from, int n)
    {
        var d = from;
        while (n > 0) { d = d.AddDays(1); if (IsWeekday(d)) n--; }
        return d;
    }

    private static DateOnly PreviousWeekday(DateOnly from, int n)
    {
        var d = from;
        while (n > 0) { d = d.AddDays(-1); if (IsWeekday(d)) n--; }
        return d;
    }

    private static bool IsWeekday(DateOnly d) => d.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday);
}
