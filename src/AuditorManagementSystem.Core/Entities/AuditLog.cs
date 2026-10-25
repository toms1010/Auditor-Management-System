namespace AuditorManagementSystem.Core.Entities;

/// <summary>
/// Tamper-resistant audit log entry. Records important system actions for
/// accountability and compliance. Must not be editable by ordinary users.
/// </summary>
public class AuditLog : BaseEntity
{
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? Reference { get; set; }
    public string? ChangeSummary { get; set; }
    public string? Reason { get; set; }
    public string? IpAddress { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
