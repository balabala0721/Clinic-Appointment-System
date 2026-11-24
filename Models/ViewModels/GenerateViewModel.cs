using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicSystem.Models.ViewModels
{
    public class GenerateReportViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Please select report type.")]
        public string ReportType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select start date.")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "Please select end date.")]
        public DateTime? EndDate { get; set; }

        public bool IncludeSummary { get; set; }

        public bool GroupByDepartment { get; set; }

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            if (StartDate.HasValue && EndDate.HasValue)
            {
                if (StartDate.Value.Date > EndDate.Value.Date)
                {
                    yield return new ValidationResult(
                        "Start Date cannot be later than End Date.",
                        new[] { nameof(StartDate), nameof(EndDate) }
                    );
                }

                if (StartDate.Value.Date > DateTime.Today ||
                    EndDate.Value.Date > DateTime.Today)
                {
                    yield return new ValidationResult(
                        "Future dates are not allowed.",
                        new[] { nameof(StartDate), nameof(EndDate) }
                    );
                }
            }
        }
    }
}