using ClinicSystem.Models.Account;
using ClinicSystem.Models.Entities;
using ClinicSystem.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ClinicSystem.Data;

/// <summary>
/// The ONE DbContext for the whole team (code-first with EF Core migrations).
///
///   Change a class or this file  ->  Package Manager Console:
///       Add-Migration DescribeTheChange
///       Update-Database
///
/// Everything the old clinic_schema.sql did (keys, unique indexes, CHECK rules,
/// the double-booking index, the computed LineTotal) is configured below, so the
/// migration builds the same database.
/// </summary>
public class ClinicDbContext : DbContext
{
    public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options) { }

    // Appointment + scheduling modules
    public DbSet<User> Users => Set<User>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Specialisation> Specialisations => Set<Specialisation>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<DoctorSchedule> DoctorSchedules => Set<DoctorSchedule>();
    public DbSet<DoctorLeave> DoctorLeaves => Set<DoctorLeave>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    // Other modules
    public DbSet<MedicalVisit> MedicalVisits => Set<MedicalVisit>();
    public DbSet<Drug> Drugs => Set<Drug>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentCancellationRequest> PaymentCancellationRequests => Set<PaymentCancellationRequest>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<GeneratedReportHistory> GeneratedReportHistories
        => Set<GeneratedReportHistory>();
    public DbSet<AppointmentReportView> AppointmentReports => Set<AppointmentReportView>();

    public DbSet<DrugInventoryReportView> DrugInventoryReports { get; set; }
    public DbSet<PatientMedicalReportView> PatientMedicalReports
    => Set<PatientMedicalReportView>();

    public DbSet<PaymentReportView> PaymentReports
        => Set<PaymentReportView>();
    // Shared settings
    private const string UtcNowSql = "SYSUTCDATETIME()";   // default for CreatedAt-style columns
    private const int MoneyPrecision = 10, MoneyScale = 2;   // DECIMAL(10,2)

    /// <summary>Builds "[Column] IN ('A','B')" for CHECK constraints from the status arrays.</summary>
    private static string In(string column, IEnumerable<string> values) =>
        $"[{column}] IN ({string.Join(",", values.Select(v => $"'{v}'"))})";

    // Every decimal in the model is money -> DECIMAL(10,2)
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(MoneyPrecision, MoneyScale);
        configurationBuilder.Properties<Enum>().HaveConversion<string>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ------------------------------------------------------------ users
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users", t =>
            {
                t.HasCheckConstraint("CK_Users_Role", In("Role", new[] { Roles.Patient, Roles.Doctor, Roles.Staff, Roles.Admin }));
                t.HasCheckConstraint("CK_Users_Status", In("AccountStatus", new[] { "Active", "Blocked", "Deactivated" }));
            });
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.CreatedAt).HasDefaultValueSql(UtcNowSql);
        });

        modelBuilder.Entity<Patient>(e =>
        {
            e.ToTable("Patients", t => t.HasCheckConstraint("CK_Patients_Gender", "[Gender] IN ('Male','Female')"));
            e.HasOne(p => p.User).WithOne().HasForeignKey<Patient>(p => p.UserId);
            e.HasIndex(p => p.UserId).IsUnique();          // one patient profile per login
            e.HasIndex(p => p.IdNumber).IsUnique();        // EF makes nullable unique indexes ignore NULLs
        });

        // ---------------------------------------------------------- doctors
        modelBuilder.Entity<Specialisation>(e =>
        {
            e.ToTable("Specialisations");
            e.HasIndex(s => s.Name).IsUnique();
        });

        modelBuilder.Entity<Doctor>(e =>
        {
            e.ToTable("Doctors");
            e.HasOne(d => d.User).WithOne().HasForeignKey<Doctor>(d => d.UserId);
            e.HasIndex(d => d.UserId).IsUnique();
            e.HasOne(d => d.Specialisation).WithMany().HasForeignKey(d => d.SpecialisationId);
        });

        // ------------------------------------------------------- scheduling
        // Key names don't follow EF's "<ClassName>Id" convention, so set them explicitly.
        modelBuilder.Entity<DoctorSchedule>(e =>
        {
            e.ToTable("DoctorSchedules", t =>
            {
                t.HasCheckConstraint("CK_Schedule_Day", "[DayOfWeek] BETWEEN 0 AND 6");
                t.HasCheckConstraint("CK_Schedule_Time", "[EndTime] > [StartTime]");
                t.HasCheckConstraint("CK_Schedule_Slot", "[SlotDurationMinutes] > 0");
                t.HasCheckConstraint("CK_Schedule_Break",
                    "([BreakStart] IS NULL AND [BreakEnd] IS NULL) OR " +
                    "([BreakStart] >= [StartTime] AND [BreakEnd] <= [EndTime] AND [BreakEnd] > [BreakStart])");
            });
            e.HasKey(s => s.ScheduleId);
            e.HasOne(s => s.Doctor).WithMany().HasForeignKey(s => s.DoctorId);
        });

        modelBuilder.Entity<DoctorLeave>(e =>
        {
            e.ToTable("DoctorLeaves", t =>
            {
                t.HasCheckConstraint("CK_Leave_Status", In("Status", new[] { LeaveStatus.Pending, LeaveStatus.Approved, LeaveStatus.Rejected }));
                t.HasCheckConstraint("CK_Leave_Dates", "[EndDate] >= [StartDate]");
            });
            e.HasKey(l => l.LeaveId);
            e.HasOne(l => l.Doctor).WithMany().HasForeignKey(l => l.DoctorId);
            e.HasOne<User>().WithMany().HasForeignKey(l => l.ReviewedByUserId);
            e.Property(l => l.CreatedAt).HasDefaultValueSql(UtcNowSql);
        });

        // ----------------------------------------------------- appointments
        modelBuilder.Entity<Appointment>(e =>
        {
            e.ToTable("Appointments", t => t.HasCheckConstraint("CK_Appointments_Status", In("Status", AppointmentStatus.All)));
            e.HasOne(a => a.Patient).WithMany().HasForeignKey(a => a.PatientId);
            e.HasOne(a => a.Doctor).WithMany().HasForeignKey(a => a.DoctorId);
            e.HasOne<Appointment>().WithMany().HasForeignKey(a => a.RescheduledFromId);
            e.HasOne<User>().WithMany().HasForeignKey(a => a.BookedByUserId);
            e.Property(a => a.CreatedAt).HasDefaultValueSql(UtcNowSql);

            // The real double-booking guard: one active booking per doctor per start time.
            // FILTERED so a cancelled / no-show slot can be booked again.
            e.HasIndex(a => new { a.DoctorId, a.AppointmentDate, a.StartTime })
             .IsUnique()
             .HasFilter(In("Status", AppointmentStatus.All.Except(AppointmentStatus.ReleasesSlot)))
             .HasDatabaseName("UX_Appointments_DoctorSlot");
            e.HasIndex(a => new { a.PatientId, a.AppointmentDate });
        });

        // --------------------------------------------------- medical visits
        modelBuilder.Entity<MedicalVisit>(e =>
        {
            e.ToTable("MedicalVisits");
            e.HasOne(v => v.Appointment).WithMany().HasForeignKey(v => v.AppointmentId);
            e.HasIndex(v => v.AppointmentId).IsUnique();   // at most one visit per appointment
            e.HasOne(v => v.Patient).WithMany().HasForeignKey(v => v.PatientId);
            e.HasOne(v => v.Doctor).WithMany().HasForeignKey(v => v.DoctorId);
            e.HasOne<User>().WithMany().HasForeignKey(v => v.CreatedByUserId);
            e.Property(v => v.VisitDate).HasDefaultValueSql(UtcNowSql);
        });

        // ------------------------------------------------------------ drugs
        modelBuilder.Entity<Drug>(e =>
        {
            e.ToTable("Drugs", t => t.HasCheckConstraint("CK_Drugs_Stock", "[StockQuantity] >= 0"));
            e.HasMany(d => d.Prescriptions).WithOne(p => p.Drug).HasForeignKey(p => p.DrugId);
        });

        modelBuilder.Entity<Prescription>(e =>
        {
            e.ToTable("Prescriptions", t => t.HasCheckConstraint("CK_Prescriptions_Qty", "[Quantity] > 0"));
            e.HasOne(p => p.Visit).WithMany(v => v.Prescriptions).HasForeignKey(p => p.VisitId);
        });

        // --------------------------------------------------------- payments
        modelBuilder.Entity<Invoice>(e =>
        {
            e.ToTable("Invoices", t => t.HasCheckConstraint("CK_Invoices_Status", In("Status", InvoiceStatus.All)));
            e.HasIndex(i => i.InvoiceNo).IsUnique();
            e.HasOne(i => i.Visit).WithMany().HasForeignKey(i => i.VisitId);
            e.HasIndex(i => i.VisitId).IsUnique();         // one bill per visit
            e.HasOne(i => i.Patient).WithMany().HasForeignKey(i => i.PatientId);
            e.Property(i => i.IssuedAt).HasDefaultValueSql(UtcNowSql);
        });

        modelBuilder.Entity<InvoiceItem>(e =>
        {
            e.ToTable("InvoiceItems", t => t.HasCheckConstraint("CK_InvoiceItems_Type", In("ItemType", InvoiceItemType.All)));
            e.HasOne(i => i.Invoice).WithMany(inv => inv.Items).HasForeignKey(i => i.InvoiceId);
            e.HasOne(i => i.Prescription).WithMany().HasForeignKey(i => i.PrescriptionId);
            // Calculated by SQL Server and stored, so reports can SUM it directly.
            e.Property(i => i.LineTotal).HasComputedColumnSql("[Quantity] * [UnitPrice]", stored: true);
        });

        modelBuilder.Entity<Payment>(e =>
        {
            e.ToTable("Payments", t =>
            {
                t.HasCheckConstraint("CK_Payments_Amount", "[Amount] > 0");
                t.HasCheckConstraint("CK_Payments_Method", In("PaymentMethod", PaymentStatus.Methods));
                t.HasCheckConstraint("CK_Payments_Status", In("Status", PaymentStatus.All));
            });
            e.HasOne(p => p.Invoice).WithMany(i => i.Payments).HasForeignKey(p => p.InvoiceId);
            e.HasOne<User>().WithMany().HasForeignKey(p => p.RecordedByUserId);
            e.HasIndex(p => p.ReceiptNo).IsUnique();
        });

        modelBuilder.Entity<PaymentCancellationRequest>(e =>
        {
            e.ToTable("PaymentCancellationRequests", t =>
                t.HasCheckConstraint("CK_PayCancel_Status", In("Status", new[] { LeaveStatus.Pending, LeaveStatus.Approved, LeaveStatus.Rejected })));
            e.HasOne(r => r.Payment).WithMany().HasForeignKey(r => r.PaymentId);
            e.HasOne<User>().WithMany().HasForeignKey(r => r.RequestedBy);
            e.HasOne<User>().WithMany().HasForeignKey(r => r.ReviewedByUserId);
            e.Property(r => r.CreatedAt).HasDefaultValueSql(UtcNowSql);
        });

        // ------------------------------------------- notifications + audit
        modelBuilder.Entity<Notification>(e =>
        {
            e.ToTable("Notifications");
            e.HasOne<User>().WithMany().HasForeignKey(n => n.UserId);
            e.Property(n => n.CreatedAt).HasDefaultValueSql(UtcNowSql);
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.ToTable("AuditLogs", t => t.HasCheckConstraint("CK_Audit_Action", In("Action", AuditAction.All)));
            e.HasOne<User>().WithMany().HasForeignKey(a => a.UserId);
            e.Property(a => a.CreatedAt).HasDefaultValueSql(UtcNowSql);
        });

        modelBuilder.Entity<AppointmentReportView>(e =>
        {
            e.HasNoKey();
            e.ToView("vw_AppointmentReport");
        });

        modelBuilder.Entity<DrugInventoryReportView>(e =>
        {
            e.HasNoKey();
            e.ToView("vw_DrugInventoryReport");
        });

        modelBuilder.Entity<PatientMedicalReportView>(e =>
        {
            e.HasNoKey();
            e.ToView("vw_PatientMedicalReport");
        });

        modelBuilder.Entity<PaymentReportView>(e =>
        {
            e.HasNoKey();
            e.ToView("vw_PaymentReport");
        });

        modelBuilder.Entity<GeneratedReportHistory>(e =>
        {
            e.ToTable("GeneratedReportHistories");

            e.HasKey(r => r.ReportId);

            e.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        // No cascade deletes anywhere. Medical and payment records must never vanish because
        // a parent row was deleted (we soft-delete instead). This also avoids SQL Server's
        // "multiple cascade paths" error, since many tables point at Users.
        foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys()))
            fk.DeleteBehavior = DeleteBehavior.Restrict;
    }
}
