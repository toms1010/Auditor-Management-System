namespace AuditorManagementSystem.Core.DTOs;

/// <summary>
/// Data required to create a new financial transaction.
/// </summary>
public class CreateTransactionDto
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public Enums.TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string? Payee { get; set; }
    public string? PaymentMethod { get; set; }
    public int CreatedByUserId { get; set; }
}

/// <summary>
/// Data required to update an existing financial transaction.
/// </summary>
public class UpdateTransactionDto
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public Enums.TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string? Payee { get; set; }
    public string? PaymentMethod { get; set; }
}

/// <summary>
/// Data required to register a new user account.
/// </summary>
public class RegisterUserDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Enums.UserRole Role { get; set; } = Enums.UserRole.Recorder;
}

/// <summary>
/// Login credentials.
/// </summary>
public class LoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Result of an authentication attempt.
/// </summary>
public class AuthResult
{
    public bool Success { get; set; }
    public UserDto? User { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Safe, non-sensitive user representation for transfer across layers.
/// Never includes password hashes or secrets.
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Enums.UserRole Role { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Dashboard summary computed from real financial records.
/// </summary>
public class DashboardSummaryDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetMovement => TotalIncome - TotalExpenses;
    public int TotalTransactions { get; set; }
    public int ReceiptsUploaded { get; set; }
    public int PendingApprovals { get; set; }
}

/// <summary>
/// Transaction grouped by category for reporting.
/// </summary>
public class CategoryTotalDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public decimal Net => Income - Expenses;
}

/// <summary>
/// Monthly financial summary for charts.
/// </summary>
public class MonthlySummaryDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public decimal Net => Income - Expenses;
}
