
namespace ClinicSystem.Models.ViewModels
{
    public class PatientMedicalReportView
    {
        public int VisitId { get; set; }
        public int PatientId { get; set; }
        public DateTime VisitDate { get; set; }
        public string? DoctorName { get; set; }
        public string? Diagnosis { get; set; }
        public string? Treatment { get; set; }
        public DateOnly? FollowUpDate { get; set; }

        public string? MedicineName { get; set; }
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public int? Quantity { get; set; }
        public string? Instructions { get; set; }
    }
}
