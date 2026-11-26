using System.ComponentModel.DataAnnotations;

namespace ClinicSystem.Models.Entities;

public class Prescription
{
    public int PrescriptionId { get; set; }

    public int VisitId { get; set; }

    [Required]
    [MaxLength(11)]
    public string DrugId { get; set; } = "";

    [MaxLength(50)]
    public string Dosage { get; set; } = "";

    [MaxLength(50)]
    public string Frequency { get; set; } = "";

    public int? DurationDays { get; set; }

    public int Quantity { get; set; }

    [MaxLength(300)]
    public string? Instructions { get; set; }

    // Navigation Properties
    public MedicalVisit? Visit { get; set; }

    public Drug? Drug { get; set; }
}