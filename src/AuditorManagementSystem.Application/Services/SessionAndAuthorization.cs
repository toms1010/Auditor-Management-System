namespace AuditorManagementSystem.Application.Services;

using AuditorManagementSystem.Application.Interfaces;
using AuditorManagementSystem.Core.DTOs;
using AuditorManagementSystem.Core.Enums;

public class SessionService : ISessionService
{
    public UserDto? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;

    public event EventHandler? UserChanged;
    public event EventHandler? UserLoggedOut;

    public void SignIn(UserDto user)
    {
        CurrentUser = user;
        UserChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SignOut()
    {
        var previous = CurrentUser;
        CurrentUser = null;
        UserLoggedOut?.Invoke(this, EventArgs.Empty);
    }
}

public class AuthorizationService : IAuthorizationService
{
    public bool IsInRole(UserDto user, params  UserRole[] roles)
    {
        return user != null && roles.Contains(user.Role);
    }

    public bool CanAccess(UserDto user, string feature)
    {
        if (user == null || !user.IsActive)
            return false;

        return feature switch
        {
            "Dashboard" => true,
            "Transactions" => true,
            "Receipts" => true,
            "Analytics" => true,
            "Reports" => true,
            "AuditLogs" => IsInRole(user,  UserRole.Administrator,  UserRole.Auditor),
            "UserManagement" => IsInRole(user,  UserRole.Administrator),
            "Settings" => IsInRole(user,  UserRole.Administrator),
            "VoidTransaction" => IsInRole(user,  UserRole.Administrator),
            "ApproveTransaction" => IsInRole(user,  UserRole.Administrator,  UserRole.Auditor),
            _ => false
        };
    }
}

public class ApprovalService : IApprovalService
{
    private readonly ITransactionService _transactionService;
    private readonly IAuditLogService _auditLogService;

    public ApprovalService(ITransactionService transactionService, IAuditLogService auditLogService)
    {
        _transactionService = transactionService;
        _auditLogService = auditLogService;
    }

    public async Task<IEnumerable<TransactionDto>> GetPendingApprovalsAsync()
    {
        return await _transactionService.GetTransactionsAsync(status: TransactionStatus.Pending);
    }

    public async Task<bool> ApproveAsync(int transactionId, int approverUserId, string? comments)
    {
        return await _transactionService.ApproveTransactionAsync(transactionId, approverUserId, comments);
    }

    public async Task<bool> RejectAsync(int transactionId, int approverUserId, string? comments)
    {
        return await _transactionService.RejectTransactionAsync(transactionId, approverUserId, comments);
    }
}
