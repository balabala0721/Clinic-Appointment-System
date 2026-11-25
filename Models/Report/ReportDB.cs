
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClinicSystem.Models.Account;

namespace ClinicSystem.Models.Entities;

public enum ReportType
{
    AppointmentReport,
    ConsultationReport,
    MedicalVisitReport,
    PrescriptionReport,
    PatientMedicalHistoryReport,
    AppointmentSummaryReport,
    DrugInventoryReport,
    ClinicOverallPerformanceReport,
}

public class GeneratedReportHistory
{
    [Key]
    public int ReportId { get; set; }

    public int UserId { get; set; }

    public ReportType ReportType { get; set; }

    public DateTime GeneratedDateTime { get; set; } = DateTime.Now;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
