namespace AuditorManagementSystem.Infrastructure.Repositories;

using AuditorManagementSystem.Core.Entities;
using AuditorManagementSystem.Core.Interfaces;
using Microsoft.Data.Sqlite;
using Dapper;

public class ReceiptRepository : IReceiptRepository
{
    private readonly string _connectionString;

    public ReceiptRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private SqliteConnection CreateConnection() => new SqliteConnection(_connectionString);

    public async Task<Receipt?> GetByIdAsync(int id)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Receipt>(
            "SELECT * FROM Receipts WHERE Id = $id", new { id });
    }

    public async Task<IEnumerable<Receipt>> GetByTransactionIdAsync(int transactionId)
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Receipt>(
            "SELECT * FROM Receipts WHERE TransactionId = $tid ORDER BY CreatedAt DESC",
            new { tid = transactionId });
    }

    public async Task<int> AddAsync(Receipt receipt)
    {
        using var conn = CreateConnection();
        var sql = @"INSERT INTO Receipts (FileName, OriginalFileName, FilePath, FileSizeBytes,
                    ContentType, TransactionId, IsVerified, VerifiedBy, VerifiedAt, CreatedAt, CreatedBy)
                    VALUES ($name, $orig, $path, $size, $ctype, $tid, 0, NULL, NULL, $now, $by);
                    SELECT last_insert_rowid();";
        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            name = receipt.FileName,
            orig = receipt.OriginalFileName,
            path = receipt.FilePath,
            size = receipt.FileSizeBytes,
            ctype = receipt.ContentType,
            tid = receipt.TransactionId,
            now = DateTime.UtcNow.ToString("o"),
            by = receipt.CreatedBy ?? "system"
        });
    }

    public async Task<bool> UpdateAsync(Receipt receipt)
    {
        using var conn = CreateConnection();
        var sql = @"UPDATE Receipts SET IsVerified=$verified, VerifiedBy=$by, VerifiedAt=$at,
                    UpdatedAt=$now, UpdatedBy=$by WHERE Id=$id";
        var rows = await conn.ExecuteAsync(sql, new
        {
            verified = receipt.IsVerified ? 1 : 0,
            by = receipt.VerifiedBy,
            at = receipt.VerifiedAt?.ToString("o"),
            now = DateTime.UtcNow.ToString("o"),
            id = receipt.Id
        });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteAsync("DELETE FROM Receipts WHERE Id=$id", new { id });
        return rows > 0;
    }
}
