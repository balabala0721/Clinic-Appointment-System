using ClinicSystem.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicAppointmentSystem.Models
{
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
        public int ReportID { get; set; }

        [Required]
        [MaxLength(11)]
        public string UserID { get; set; }

        [Required]
        [MaxLength(100)]
        public string ReportName { get; set; }

        [Required]
        public ReportType ReportType { get; set; }

        [Required]
        public DateTime GeneratedDateTime { get; set; } = DateTime.Now;

        // Navigation property linking to your User/Account model
        [ForeignKey("UserID")]
        public User User { get; set; }
    }
}