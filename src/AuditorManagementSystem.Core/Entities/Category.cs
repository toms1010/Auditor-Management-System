namespace AuditorManagementSystem.Core.Entities;

/// <summary>
/// A category used to classify transactions (e.g. Office Supplies, Utilities).
/// </summary>
public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
