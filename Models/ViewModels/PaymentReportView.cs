
namespace ClinicSystem.Models.ViewModels
{
    public class PaymentReportView
    {
        public string InvoiceNo { get; set; } = "";

        public string InvoiceStatus { get; set; } = "";

        public string PatientName { get; set; } = "";

        public DateTime IssuedAt { get; set; }

        public DateTime PaidAt { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal PaidAmount { get; set; }

        public string? ReceiptNo { get; set; }

        public string? PaymentMethod { get; set; }

        public int PatientId { get; set; }
    }
}
