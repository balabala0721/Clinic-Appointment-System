
using System.ComponentModel.DataAnnotations;

namespace ClinicSystem.Models.ViewModels
{
    public class GenerateReportViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Please select a report type.")]
        public string ReportType { get; set; } = "";

        [Required(ErrorMessage = "Start date is required.")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "End date is required.")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today;

        public bool IncludeSummary { get; set; }

        public bool GroupByDepartment { get; set; }

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            if (StartDate.Date > EndDate.Date)
            {
                yield return new ValidationResult(
                    "Start date cannot be later than end date.",
                    new[] { nameof(StartDate), nameof(EndDate) }
                );
            }
        }
    }
}
