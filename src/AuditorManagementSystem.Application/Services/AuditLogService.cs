namespace AuditorManagementSystem.Application.Services;

using AuditorManagementSystem.Application.Interfaces;
using AuditorManagementSystem.Core.DTOs;
using AuditorManagementSystem.Core.Entities;
using AuditorManagementSystem.Core.Interfaces;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task LogAsync(int? userId, string? userName, string action, string entityType,
        int? entityId = null, string? reference = null, string? changeSummary = null,
        string? reason = null, string? ipAddress = null)
    {
        var log = new AuditLog
        {
            UserId = userId,
            UserName = userName,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Reference = reference,
            ChangeSummary = changeSummary,
            Reason = reason,
            IpAddress = ipAddress,
            OccurredAt = DateTime.UtcNow
        };
        await _auditLogRepository.AddAsync(log);
    }

    public async Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var logs = startDate.HasValue && endDate.HasValue
            ? await _auditLogRepository.GetByDateRangeAsync(startDate.Value, endDate.Value)
            : await _auditLogRepository.GetAllAsync();

        return logs.Select(l => new AuditLogDto
        {
            Id = l.Id,
            UserId = l.UserId,
            UserName = l.UserName,
            Action = l.Action,
            EntityType = l.EntityType,
            EntityId = l.EntityId,
            Reference = l.Reference,
            ChangeSummary = l.ChangeSummary,
            Reason = l.Reason,
            OccurredAt = l.OccurredAt
        });
    }
}
