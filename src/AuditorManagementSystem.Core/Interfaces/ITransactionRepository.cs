namespace AuditorManagementSystem.Core.Interfaces;

using AuditorManagementSystem.Core.Entities;
using AuditorManagementSystem.Core.Enums;

/// <summary>
/// Repository contract for financial transactions.
/// </summary>
public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(int id);
    Task<Transaction?> GetByReferenceNumberAsync(string referenceNumber);
    Task<IEnumerable<Transaction>> GetAllAsync();
    Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Transaction>> GetByStatusAsync(TransactionStatus status);
    Task<IEnumerable<Transaction>> GetByCategoryAsync(string category);
    Task<int> AddAsync(Transaction transaction);
    Task<bool> UpdateAsync(Transaction transaction);
    Task<bool> DeleteAsync(int id);
    Task<bool> VoidAsync(int id, string voidReason, string voidedBy);
    Task<IEnumerable<Transaction>> GetPendingApprovalsAsync();
}
