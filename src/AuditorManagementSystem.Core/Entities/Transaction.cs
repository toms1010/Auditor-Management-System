namespace AuditorManagementSystem.Core.Entities;

using AuditorManagementSystem.Core.Enums;

/// <summary>
/// A financial transaction record. Amounts are stored as decimal to avoid
/// floating-point rounding errors in financial calculations.
/// </summary>
public class Transaction : BaseEntity
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string? Payee { get; set; }
    public string? PaymentMethod { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
    public bool IsVoided { get; set; }
    public string? VoidReason { get; set; }
    public DateTime? VoidedAt { get; set; }
    public string? VoidedBy { get; set; }
    public int CreatedByUserId { get; set; }
    public string? ReceiptFileName { get; set; }
    public string? ReceiptFilePath { get; set; }
}
