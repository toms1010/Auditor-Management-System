namespace AuditorManagementSystem.Core.Interfaces;

using AuditorManagementSystem.Core.Entities;

/// <summary>
/// Repository contract for transaction categories.
/// </summary>
public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<int> AddAsync(Category category);
    Task<bool> UpdateAsync(Category category);
}
