
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicSystem.Models.Entities;

public enum StockStatus
{
    InStock,
    LowStock,
    OutOfStock
}


public enum DrugCategory
{
    Analgesics,
    Antibiotics,
    Antihistamines,
    Antipyretics,
    Antacids,
    Antitussives,
    Expectorants,
    Antidiarrheals,
    Laxatives,
    Antifungals,
    Antiseptics,
    Antidiabetics,
    Antihypertensives,
    Bronchodilators,
    Corticosteroids,

    // Additional categories
    Vitamins,
    Dermatologicals,
    Cardiovascular,
    Gastrointestinal,
    MuscleRelaxants,
    Ophthalmic,
    ENT
}

public class Drug
{
    [Key]
    [MaxLength(11)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string DrugID { get; set; } = "";

    [Required]
    [MaxLength(100)]
    public string DrugName { get; set; } = "";

    [Required]
    public DrugCategory DrugCategory { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = "";

    [Required]
    [MaxLength(255)]
    public string MedicalUses { get; set; } = "";

    [Required]
    [Precision(10, 2)]
    [Range(0.01, 99999999.99)]
    public decimal UnitPrice { get; set; }

    [Required]
    [Range(0, 9999)]
    public int StockQuantity { get; set; }

    [Required]
    [Range(0, 99)]
    public int MinStockLevel { get; set; }
    [Required]
    public bool IsActive { get; set; } = true;

    // Automatically determine stock status
    [NotMapped]
    public StockStatus StockStatus =>
        StockQuantity == 0
            ? StockStatus.OutOfStock
            : StockQuantity < MinStockLevel
                ? StockStatus.LowStock
                : StockStatus.InStock;

    // Navigation Property
    public List<Prescription> Prescriptions { get; set; } = new();
}
