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
    [Key] // Int primary key auto-increments by convention in EF Core
    public int ReportId { get; set; }

    [Required]
    [MaxLength(11)]
    public int UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ReportName { get; set; }

    [Required]
    public ReportType ReportType { get; set; }

    [Required]
    public DateTime GeneratedDateTime { get; set; } = DateTime.Now;

    // Navigation property linking to your User/Account model
    [ForeignKey("UserId")]
    public User User { get; set; }
}