namespace AuditorManagementSystem.Core.Entities;

using AuditorManagementSystem.Core.Enums;

/// <summary>
/// A user account in the system.
/// </summary>
public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Recorder;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
}
