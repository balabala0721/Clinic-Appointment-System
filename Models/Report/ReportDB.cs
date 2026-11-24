using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    public int ReportID { get; set; }

    [Required]
    public int UserID { get; set; }

    [Required]
    public ReportType ReportType { get; set; }

    [Required]
    public DateTime GeneratedDateTime { get; set; } = DateTime.Now;

    // Navigation property linking to your User/Account model
    [ForeignKey("UserID")]
    public User User { get; set; }
}