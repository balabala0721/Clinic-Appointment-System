using Microsoft.EntityFrameworkCore.Migrations;

namespace ClinicSystem.Data;

/// <summary>
/// SQL views for Report Management. EF Core can't generate views from classes,
/// so the InitialCreate migration calls these by hand:
///   Up():   ReportViews.Create(migrationBuilder);   (last line)
///   Down(): ReportViews.Drop(migrationBuilder);     (first line)
/// </summary>
public static class ReportViews
{
    private const string AppointmentReport = """
        CREATE VIEW vw_AppointmentReport AS
        SELECT a.AppointmentId, a.AppointmentDate, a.StartTime, a.Status,
               p.FullName AS PatientName, d.FullName AS DoctorName, s.Name AS Specialisation
        FROM Appointments a
        JOIN Patients p        ON p.PatientId = a.PatientId
        JOIN Doctors d         ON d.DoctorId  = a.DoctorId
        JOIN Specialisations s ON s.SpecialisationId = d.SpecialisationId;
        """;

    private const string PaymentReport = """
        CREATE VIEW vw_PaymentReport AS
        SELECT i.InvoiceNo, i.IssuedAt, i.TotalAmount, i.Status AS InvoiceStatus,
               pay.Amount AS PaidAmount, pay.PaymentMethod, pay.PaidAt, pay.ReceiptNo,
               p.PatientId, p.FullName AS PatientName
        FROM Invoices i
        JOIN Patients p        ON p.PatientId = i.PatientId
        LEFT JOIN Payments pay ON pay.InvoiceId = i.InvoiceId
                              AND pay.Status = 'Successful' AND pay.IsDeleted = 0
        WHERE i.IsDeleted = 0;
        """;

    private const string PatientMedicalReport = """
        CREATE VIEW vw_PatientMedicalReport AS
        SELECT v.VisitId, v.PatientId, v.VisitDate, d.FullName AS DoctorName,
               v.Diagnosis, v.Treatment, v.FollowUpDate,
               m.Name AS MedicineName, pr.Dosage, pr.Frequency, pr.Quantity, pr.Instructions
        FROM MedicalVisits v
        JOIN Doctors d             ON d.DoctorId = v.DoctorId
        LEFT JOIN Prescriptions pr ON pr.VisitId = v.VisitId
        LEFT JOIN Medicines m      ON m.MedicineId = pr.MedicineId
        WHERE v.IsDeleted = 0;
        """;

    private static readonly string[] ViewNames = { "vw_AppointmentReport", "vw_PaymentReport", "vw_PatientMedicalReport" };

    public static void Create(MigrationBuilder migrationBuilder)
    {
        // Each CREATE VIEW must run on its own (SQL Server rule), so one Sql() call per view.
        migrationBuilder.Sql(AppointmentReport);
        migrationBuilder.Sql(PaymentReport);
        migrationBuilder.Sql(PatientMedicalReport);
    }

    public static void Drop(MigrationBuilder migrationBuilder)
    {
        foreach (var name in ViewNames)
            migrationBuilder.Sql($"DROP VIEW IF EXISTS {name};");
    }
}
