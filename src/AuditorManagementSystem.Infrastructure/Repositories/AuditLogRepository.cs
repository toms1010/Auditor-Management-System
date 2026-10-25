namespace AuditorManagementSystem.Infrastructure.Repositories;

using AuditorManagementSystem.Core.Entities;
using AuditorManagementSystem.Core.Interfaces;
using Microsoft.Data.Sqlite;
using Dapper;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly string _connectionString;

    public AuditLogRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private SqliteConnection CreateConnection() => new SqliteConnection(_connectionString);

    public async Task<int> AddAsync(AuditLog auditLog)
    {
        using var conn = CreateConnection();
        var sql = @"INSERT INTO AuditLogs (UserId, UserName, Action, EntityType, EntityId, Reference,
                    ChangeSummary, Reason, IpAddress, OccurredAt, CreatedAt)
                    VALUES ($uid, $uname, $action, $entity, $eid, $ref, $summary, $reason, $ip, $occurred, $now);
                    SELECT last_insert_rowid();";
        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            uid = auditLog.UserId,
            uname = auditLog.UserName,
            action = auditLog.Action,
            entity = auditLog.EntityType,
            eid = auditLog.EntityId,
            @ref = auditLog.Reference,
            summary = auditLog.ChangeSummary,
            reason = auditLog.Reason,
            ip = auditLog.IpAddress,
            occurred = auditLog.OccurredAt.ToString("o"),
            now = DateTime.UtcNow.ToString("o")
        });
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync()
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<AuditLog>(
            "SELECT * FROM AuditLogs ORDER BY OccurredAt DESC");
    }

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<AuditLog>(
            @"SELECT * FROM AuditLogs
              WHERE date(OccurredAt) >= date($start) AND date(OccurredAt) <= date($end)
              ORDER BY OccurredAt DESC",
            new { start = startDate.ToString("yyyy-MM-dd"), end = endDate.ToString("yyyy-MM-dd") });
    }

    public async Task<IEnumerable<AuditLog>> GetByUserAsync(int userId)
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<AuditLog>(
            "SELECT * FROM AuditLogs WHERE UserId = $uid ORDER BY OccurredAt DESC",
            new { uid = userId });
    }
}
