namespace AuditorManagementSystem.Core.Entities;

using AuditorManagementSystem.Core.Enums;

/// <summary>
/// Records who approved or rejected a transaction and when.
/// </summary>
public class ApprovalHistory : BaseEntity
{
    public int TransactionId { get; set; }
    public int ApprovedByUserId { get; set; }
    public ApprovalStatus Status { get; set; }
    public string? Comments { get; set; }
    public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;
}
