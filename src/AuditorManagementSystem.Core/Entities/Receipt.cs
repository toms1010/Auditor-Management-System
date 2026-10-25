namespace AuditorManagementSystem.Core.Entities;

/// <summary>
/// Metadata for an uploaded receipt file. The file itself is stored on disk
/// by Infrastructure; this entity records the database reference.
/// </summary>
public class Receipt : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public int TransactionId { get; set; }
    public bool IsVerified { get; set; }
    public string? VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
}
