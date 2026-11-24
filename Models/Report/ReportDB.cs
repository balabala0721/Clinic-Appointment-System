using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClinicSystem.Models.Account;

namespace ClinicSystem.Models.Entities;

public enum ReportType
{
    AppointmentReport,
    AppointmentSummaryReport,
    MedicalVisitReport,
    PrescriptionReport,
    PatientMedicalHistoryReport,
    DrugInventoryReport,
    ClinicOverallPerformanceReport,
}

public class GeneratedReportHistory
{
    [Key] // Int primary key auto-increments by convention in EF Core
    public int ReportId { get; set; }

    [Required]
    public int UserID { get; set; }

    [Required]
    public ReportType ReportType { get; set; }

    [Required]
    public DateTime GeneratedDateTime { get; set; } = DateTime.Now;

    // Navigation property linking to your User/Account model
    [ForeignKey("UserId")]
    public User User { get; set; }
}