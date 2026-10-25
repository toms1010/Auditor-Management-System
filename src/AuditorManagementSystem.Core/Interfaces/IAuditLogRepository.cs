namespace AuditorManagementSystem.Core.Interfaces;

using AuditorManagementSystem.Core.Entities;

/// <summary>
/// Repository contract for audit log entries. Append-only.
/// </summary>
public interface IAuditLogRepository
{
    Task<int> AddAsync(AuditLog auditLog);
    Task<IEnumerable<AuditLog>> GetAllAsync();
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<AuditLog>> GetByUserAsync(int userId);
}
