
namespace ClinicSystem.Models.ViewModels
{
    public class AppointmentReportView
    {
        public int AppointmentId { get; set; }

        public int PatientId { get; set; }

        public DateOnly AppointmentDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public string Status { get; set; } = "";

        public string PatientName { get; set; } = "";

        public string DoctorName { get; set; } = "";

        public string Specialisation { get; set; } = "";
    }
}
