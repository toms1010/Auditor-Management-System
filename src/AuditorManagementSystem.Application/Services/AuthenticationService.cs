namespace AuditorManagementSystem.Application.Services;

using AuditorManagementSystem.Application.Interfaces;
using AuditorManagementSystem.Core.DTOs;
using AuditorManagementSystem.Core.Entities;
using AuditorManagementSystem.Core.Enums;
using AuditorManagementSystem.Core.Exceptions;
using AuditorManagementSystem.Core.Interfaces;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthenticationService(IUserRepository userRepository, IAuditLogService auditLogService, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _auditLogService = auditLogService;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResult> LoginAsync(LoginDto loginDto)
    {
        if (string.IsNullOrWhiteSpace(loginDto.Password))
        {
            await _auditLogService.LogAsync(null, loginDto.Username, "LoginFailed", "User", reason: "Empty password");
            return new AuthResult { Success = false, ErrorMessage = "Please enter a password." };
        }

        var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
        if (user == null || !user.IsActive)
        {
            await _auditLogService.LogAsync(null, loginDto.Username, "LoginFailed", "User", reason: "Invalid credentials or inactive account");
            return new AuthResult { Success = false, ErrorMessage = "Invalid username or password." };
        }

        // Verify the password hash using PBKDF2
        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            await _auditLogService.LogAsync(user.Id, user.Username, "LoginFailed", "User", reason: "No password hash stored");
            return new AuthResult { Success = false, ErrorMessage = "Account has no password set. Contact administrator." };
        }

        if (!_passwordHasher.Verify(loginDto.Password, user.PasswordHash))
        {
            await _auditLogService.LogAsync(user.Id, user.Username, "LoginFailed", "User", reason: "Invalid password");
            return new AuthResult { Success = false, ErrorMessage = "Invalid username or password." };
        }

        // Update last login timestamp
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        await _auditLogService.LogAsync(user.Id, user.Username, "Login", "User", reason: "Successful login");
        return new AuthResult { Success = true, User = Map(user) };
    }

    public async Task LogoutAsync(int userId)
    {
        await _auditLogService.LogAsync(userId, null, "Logout", "User", entityId: userId);
    }

    public Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        throw new NotImplementedException("Password change requires hash-store access via Infrastructure.");
    }

    private static UserDto Map(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        FullName = user.FullName,
        Role = user.Role,
        IsActive = user.IsActive
    };
}
