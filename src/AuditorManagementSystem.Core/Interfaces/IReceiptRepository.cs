namespace AuditorManagementSystem.Core.Interfaces;

using AuditorManagementSystem.Core.Entities;

/// <summary>
/// Repository contract for receipts.
/// </summary>
public interface IReceiptRepository
{
    Task<Receipt?> GetByIdAsync(int id);
    Task<IEnumerable<Receipt>> GetByTransactionIdAsync(int transactionId);
    Task<int> AddAsync(Receipt receipt);
    Task<bool> UpdateAsync(Receipt receipt);
    Task<bool> DeleteAsync(int id);
}
