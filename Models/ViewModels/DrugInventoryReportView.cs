using ClinicSystem.Models.Drugs;
using ClinicSystem.Models.Entities;

namespace ClinicSystem.Models.ViewModels
{
    public class DrugInventoryReportView
    {
        public int DrugId { get; set; }

        public string DrugName { get; set; } = "";

        public DrugCategory DrugCategory { get; set; }

        public string MedicalUses { get; set; } = "";

        public decimal UnitPrice { get; set; }

        public int StockQuantity { get; set; }

        public int MinStockLevel { get; set; }

        public StockStatus StockStatus { get; set; }
    }
}