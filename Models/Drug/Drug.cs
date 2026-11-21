using ClinicSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicSystem.Models.Drugs;

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
    Corticosteroids
}

public class Drug
{
    [Key]
    [MaxLength(11)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)] // Since DrugID is a Varchar(11) custom ID
    public string DrugID { get; set; }

    [Required]
    [MaxLength(100)]
    public string DrugName { get; set; }

    [Required]
    public DrugCategory DrugCategory { get; set; }


    [Required]
    [MaxLength(255)]
    public string MedicalUses { get; set; }

    [Required]
    [Precision(10, 2)]
    [Range(0.01, 999.99)]
    public decimal UnitPrice { get; set; }

    [Required]
    [Range(0, 9999)]
    public int StockQuantity { get; set; }

    [Required]
    public StockStatus StockStatus { get; set; }

    [Required]
    [Range(0, 99)]
    public int MinStockLevel { get; set; }

    // Navigation Property
    public ICollection<PrescriptionDrug> PrescriptionDrugs { get; set; }
}