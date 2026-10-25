namespace AuditorManagementSystem.Core.Entities;

/// <summary>
/// Role definition used for authorization.
/// </summary>
public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSystem { get; set; } = false;
}
