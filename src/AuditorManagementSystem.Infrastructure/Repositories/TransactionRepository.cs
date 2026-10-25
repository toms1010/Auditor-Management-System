namespace AuditorManagementSystem.Infrastructure.Repositories;

using AuditorManagementSystem.Core.Entities;
using AuditorManagementSystem.Core.Enums;
using AuditorManagementSystem.Core.Interfaces;
using Microsoft.Data.Sqlite;
using Dapper;

public class TransactionRepository : ITransactionRepository
{
    private readonly string _connectionString;

    public TransactionRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private SqliteConnection CreateConnection() => new SqliteConnection(_connectionString);

    public async Task<Transaction?> GetByIdAsync(int id)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Transaction>(
            "SELECT * FROM Transactions WHERE Id = $id", new { id });
    }

    public async Task<Transaction?> GetByReferenceNumberAsync(string referenceNumber)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Transaction>(
            "SELECT * FROM Transactions WHERE ReferenceNumber = $ref", new { @ref = referenceNumber });
    }

    public async Task<IEnumerable<Transaction>> GetAllAsync()
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Transaction>(
            "SELECT * FROM Transactions ORDER BY TransactionDate DESC");
    }

    public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Transaction>(
            @"SELECT * FROM Transactions
              WHERE date(TransactionDate) >= date($start) AND date(TransactionDate) <= date($end)
              ORDER BY TransactionDate DESC",
            new { start = startDate.ToString("yyyy-MM-dd"), end = endDate.ToString("yyyy-MM-dd") });
    }

    public async Task<IEnumerable<Transaction>> GetByStatusAsync(TransactionStatus status)
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Transaction>(
            "SELECT * FROM Transactions WHERE Status = $status ORDER BY TransactionDate DESC",
            new { status = status.ToString() });
    }

    public async Task<IEnumerable<Transaction>> GetByCategoryAsync(string category)
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Transaction>(
            "SELECT * FROM Transactions WHERE Category = $category ORDER BY TransactionDate DESC",
            new { category });
    }

    public async Task<int> AddAsync(Transaction transaction)
    {
        using var conn = CreateConnection();
        var sql = @"INSERT INTO Transactions (ReferenceNumber, TransactionDate, Description, Category, Type, Amount,
                    Payee, PaymentMethod, Status, ApprovalStatus, IsVoided, VoidReason, CreatedByUserId,
                    ReceiptFileName, ReceiptFilePath, CreatedAt, CreatedBy)
                    VALUES ($ref, $date, $desc, $cat, $type, $amount, $payee, $method, $status,
                            $approval, 0, NULL, $user, $receiptName, $receiptPath, $now, $by);
                    SELECT last_insert_rowid();";
        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            @ref = transaction.ReferenceNumber,
            date = transaction.TransactionDate.ToString("o"),
            desc = transaction.Description,
            cat = transaction.Category,
            type = transaction.Type.ToString(),
            amount = transaction.Amount,
            payee = transaction.Payee,
            method = transaction.PaymentMethod,
            status = transaction.Status.ToString(),
            approval = transaction.ApprovalStatus.ToString(),
            user = transaction.CreatedByUserId,
            receiptName = transaction.ReceiptFileName,
            receiptPath = transaction.ReceiptFilePath,
            now = DateTime.UtcNow.ToString("o"),
            by = transaction.CreatedBy
        });
    }

    public async Task<bool> UpdateAsync(Transaction transaction)
    {
        using var conn = CreateConnection();
        var sql = @"UPDATE Transactions SET ReferenceNumber=$ref, TransactionDate=$date, Description=$desc,
                    Category=$cat, Type=$type, Amount=$amount, Payee=$payee, PaymentMethod=$method,
                    Status=$status, ApprovalStatus=$approval, UpdatedAt=$now, UpdatedBy=$by
                    WHERE Id=$id";
        var rows = await conn.ExecuteAsync(sql, new
        {
            @ref = transaction.ReferenceNumber,
            date = transaction.TransactionDate.ToString("o"),
            desc = transaction.Description,
            cat = transaction.Category,
            type = transaction.Type.ToString(),
            amount = transaction.Amount,
            payee = transaction.Payee,
            method = transaction.PaymentMethod,
            status = transaction.Status.ToString(),
            approval = transaction.ApprovalStatus.ToString(),
            now = DateTime.UtcNow.ToString("o"),
            by = transaction.UpdatedBy ?? "system",
            id = transaction.Id
        });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteAsync("DELETE FROM Transactions WHERE Id=$id", new { id });
        return rows > 0;
    }

    public async Task<bool> VoidAsync(int id, string voidReason, string voidedBy)
    {
        using var conn = CreateConnection();
        var sql = @"UPDATE Transactions SET IsVoided = 1, VoidReason = $reason, VoidedAt = $now,
                    VoidedBy = $by, UpdatedAt = $now, UpdatedBy = $by WHERE Id = $id AND IsVoided = 0";
        var rows = await conn.ExecuteAsync(sql, new
        {
            reason = voidReason,
            now = DateTime.UtcNow.ToString("o"),
            by = voidedBy,
            id
        });
        return rows > 0;
    }

    public async Task<IEnumerable<Transaction>> GetPendingApprovalsAsync()
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Transaction>(
            "SELECT * FROM Transactions WHERE ApprovalStatus = 'Pending' AND IsVoided = 0 ORDER BY TransactionDate ASC");
    }
}
