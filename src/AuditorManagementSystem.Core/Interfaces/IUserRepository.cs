namespace AuditorManagementSystem.Core.Interfaces;

using AuditorManagementSystem.Core.Entities;

/// <summary>
/// Repository contract for user accounts.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task<int> AddAsync(User user);
    Task<bool> UpdateAsync(User user);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(string username);
}
