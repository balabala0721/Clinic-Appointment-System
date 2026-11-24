using ClinicSystem.Models.Entities;
using ClinicSystem.Models.Drugs;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClinicSystem.Models.Account;

namespace ClinicSystem.Models.Entities;

// Tables owned by teammates' modules. They're defined here so ONE migration builds the
// whole database. Teammates: add properties to these classes rather than creating new ones,
// then run Add-Migration.

// ------------------------------------------------------- Medical Visit Records

public class MedicalVisit
{
    [Key] public int VisitId { get; set; }
    public int? AppointmentId { get; set; }          // null = walk-in without a booking
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime VisitDate { get; set; }
    [MaxLength(1000)] public string? Symptoms { get; set; }
    [MaxLength(1000)] public string? Diagnosis { get; set; }
    [MaxLength(1000)] public string? Treatment { get; set; }
    public DateOnly? FollowUpDate { get; set; }
    [MaxLength(2000)] public string? Notes { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public Appointment? Appointment { get; set; }
    public Patient? Patient { get; set; }
    public Doctor? Doctor { get; set; }
    public List<Prescription> Prescriptions { get; set; } = new();
}

// ------------------------------------------------------------------ Drugs



public class Prescription
{
    public int PrescriptionId { get; set; }
    public int VisitId { get; set; }
    public int DrugId { get; set; }
    [MaxLength(50)] public string Dosage { get; set; } = "";     // e.g. 1 tablet
    [MaxLength(50)] public string Frequency { get; set; } = "";  // e.g. 3 times a day
    public int? DurationDays { get; set; }
    public int Quantity { get; set; }
    [MaxLength(300)] public string? Instructions { get; set; }   // e.g. after meals

    public MedicalVisit? Visit { get; set; }
    [MaxLength(11)]
    public Drug? Drug { get; set; }

}


// --------------------------------------------------------------- Payments

public static class InvoiceStatus
{
    public const string Unpaid = "Unpaid", PartiallyPaid = "PartiallyPaid", Paid = "Paid", Cancelled = "Cancelled";
    public static readonly string[] All = { Unpaid, PartiallyPaid, Paid, Cancelled };
}

public static class PaymentStatus
{
    public const string Pending = "Pending", Successful = "Successful", Failed = "Failed", Refunded = "Refunded", Cancelled = "Cancelled";
    public static readonly string[] All = { Pending, Successful, Failed, Refunded, Cancelled };
    public static readonly string[] Methods = { "Cash", "Card", "FPX", "EWallet", "Panel" };
}

public static class InvoiceItemType
{
    public const string Consultation = "Consultation", Drug = "Drug", Other = "Other";
    public static readonly string[] All = { Consultation, Drug, Other };
}

public class Invoice
{
    public int InvoiceId { get; set; }
    [MaxLength(30)] public string InvoiceNo { get; set; } = "";  // e.g. INV-2026-000123
    public int VisitId { get; set; }
    public int PatientId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    [Column(TypeName = "char(3)")] public string Currency { get; set; } = "MYR";
    [MaxLength(20)] public string Status { get; set; } = InvoiceStatus.Unpaid;
    public DateTime IssuedAt { get; set; }
    public bool IsDeleted { get; set; }

    public MedicalVisit? Visit { get; set; }
    public Patient? Patient { get; set; }
    public List<InvoiceItem> Items { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();
}

public class InvoiceItem
{
    public int InvoiceItemId { get; set; }
    public int InvoiceId { get; set; }
    [MaxLength(20)] public string ItemType { get; set; } = InvoiceItemType.Consultation;
    public int? PrescriptionId { get; set; }
    [MaxLength(200)] public string Description { get; set; } = "";
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }            // copied at billing time so later price changes don't alter old bills
    public decimal LineTotal { get; private set; }    // calculated by SQL Server (Quantity * UnitPrice) - read only

    public Invoice? Invoice { get; set; }
    public Prescription? Prescription { get; set; }
}

public class Payment
{
    public int PaymentId { get; set; }
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
    [MaxLength(20)] public string PaymentMethod { get; set; } = "Cash";
    [MaxLength(20)] public string Status { get; set; } = PaymentStatus.Pending;
    [MaxLength(100)] public string? TransactionRef { get; set; }   // reference from payment gateway
    [MaxLength(30)] public string? ReceiptNo { get; set; }
    public DateTime? PaidAt { get; set; }
    public int? RecordedByUserId { get; set; }        // staff for counter payments; null for online
    public bool IsDeleted { get; set; }

    public Invoice? Invoice { get; set; }
}

public class PaymentCancellationRequest
{
    [Key] public int RequestId { get; set; }
    public int PaymentId { get; set; }
    public int RequestedBy { get; set; }              // UserId
    [MaxLength(500)] public string Reason { get; set; } = "";
    [MaxLength(20)] public string Status { get; set; } = LeaveStatus.Pending;   // Pending | Approved | Rejected
    public int? ReviewedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public Payment? Payment { get; set; }
}

// ----------------------------------------------------- Notifications + Audit

public class Notification
{
    public int NotificationId { get; set; }
    public int UserId { get; set; }
    [MaxLength(30)] public string Type { get; set; } = "";       // AppointmentReminder, Cancelled, Receipt, LowStock...
    [MaxLength(150)] public string Title { get; set; } = "";
    [MaxLength(1000)] public string Message { get; set; } = "";
    public bool IsRead { get; set; }
    public bool EmailSent { get; set; }
    public DateTime CreatedAt { get; set; }
}

public static class AuditAction
{
    public const string Create = "Create", Update = "Update", Delete = "Delete", Login = "Login", Export = "Export";
    public static readonly string[] All = { Create, Update, Delete, Login, Export };
}

public class AuditLog
{
    [Key] public long AuditId { get; set; }
    public int? UserId { get; set; }
    [MaxLength(20)] public string Action { get; set; } = "";
    [MaxLength(50)] public string TableName { get; set; } = "";
    public int? RecordId { get; set; }
    public string? OldValues { get; set; }            // JSON snapshot (NVARCHAR(MAX))
    public string? NewValues { get; set; }
    public DateTime CreatedAt { get; set; }
}



