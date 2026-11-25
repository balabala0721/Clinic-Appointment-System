
using ClinicSystem.Models.Entities;

namespace ClinicSystem.Models.ViewModels
{
    public class ReportView
    {
        public List<GeneratedReportHistory> GeneratedReports { get; set; }
            = new List<GeneratedReportHistory>();
    }
}
