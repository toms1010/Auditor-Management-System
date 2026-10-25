namespace AuditorManagementSystem.Application.Interfaces;

using AuditorManagementSystem.Core.DTOs;
using AuditorManagementSystem.Core.Enums;

/// <summary>
/// Authentication and session management service.
/// </summary>
public interface IAuthenticationService
{
    Task<AuthResult> LoginAsync(LoginDto loginDto);
    Task LogoutAsync(int userId);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
}

/// <summary>
/// Role-based authorization checks.
/// </summary>
public interface IAuthorizationService
{
    bool IsInRole(UserDto user, params  UserRole[] roles);
    bool CanAccess(UserDto user, string feature);
}

/// <summary>
/// Session state for the currently signed-in user.
/// </summary>
public interface ISessionService
{
    UserDto? CurrentUser { get; }
    bool IsAuthenticated { get; }
    void SignIn(UserDto user);
    void SignOut();
    event EventHandler? UserChanged;
    event EventHandler? UserLoggedOut;
}

/// <summary>
/// Financial transaction business operations.
/// </summary>
public interface ITransactionService
{
    Task<int> CreateTransactionAsync(CreateTransactionDto dto, int currentUserId);
    Task<TransactionDto?> GetTransactionByIdAsync(int id);
    Task<IEnumerable<TransactionDto>> GetTransactionsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? category = null,
         TransactionType? type = null,
         TransactionStatus? status = null,
        string? search = null);
    Task<bool> UpdateTransactionAsync(UpdateTransactionDto dto, int currentUserId);
    Task<bool> VoidTransactionAsync(int id, string reason, int currentUserId);
    Task<bool> ApproveTransactionAsync(int id, int approverUserId, string? comments);
    Task<bool> RejectTransactionAsync(int id, int approverUserId, string? comments);
}

/// <summary>
/// Receipt upload and management service.
/// </summary>
public interface IReceiptService
{
    Task<int> UploadReceiptAsync(int transactionId, string originalFileName, byte[] fileData, string contentType, int uploadedByUserId);
    Task<byte[]?> DownloadReceiptAsync(int receiptId);
    Task<bool> DeleteReceiptAsync(int receiptId, int currentUserId);
    Task<bool> VerifyReceiptAsync(int receiptId, string verifiedBy, string? notes);
}

/// <summary>
/// Financial calculations used by dashboard, analytics, and reports.
/// </summary>
public interface IFinancialCalculationService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<CategoryTotalDto>> GetCategoryTotalsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<MonthlySummaryDto>> GetMonthlySummariesAsync(int months = 12);
    Task<decimal> GetTotalIncomeAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<decimal> GetTotalExpensesAsync(DateTime? startDate = null, DateTime? endDate = null);
}

/// <summary>
/// Approval workflow operations.
/// </summary>
public interface IApprovalService
{
    Task<IEnumerable<TransactionDto>> GetPendingApprovalsAsync();
    Task<bool> ApproveAsync(int transactionId, int approverUserId, string? comments);
    Task<bool> RejectAsync(int transactionId, int approverUserId, string? comments);
}

/// <summary>
/// Audit logging service.
/// </summary>
public interface IAuditLogService
{
    Task LogAsync(int? userId, string? userName, string action, string entityType,
        int? entityId = null, string? reference = null, string? changeSummary = null,
        string? reason = null, string? ipAddress = null);
    Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null);
}

/// <summary>
/// Report generation and export service.
/// </summary>
public interface IReportService
{
    Task<byte[]> GenerateCsvReportAsync(string reportType, DateTime startDate, DateTime endDate);
    Task<byte[]> GeneratePdfReportAsync(string reportType, DateTime startDate, DateTime endDate);
}

/// <summary>
/// Backup and restore operations.
/// </summary>
public interface IBackupService
{
    Task<string> CreateBackupAsync();
    Task<bool> RestoreBackupAsync(string backupPath);
}

/// <summary>
/// Represents a transaction for transfer across layers.
/// </summary>
public class TransactionDto
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public  TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string? Payee { get; set; }
    public string? PaymentMethod { get; set; }
    public  TransactionStatus Status { get; set; }
    public  ApprovalStatus ApprovalStatus { get; set; }
    public bool IsVoided { get; set; }
    public string? VoidReason { get; set; }
    public int CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public string? ReceiptFileName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Represents an audit log entry for transfer across layers.
/// </summary>
public class AuditLogDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? Reference { get; set; }
    public string? ChangeSummary { get; set; }
    public string? Reason { get; set; }
    public DateTime OccurredAt { get; set; }
}
