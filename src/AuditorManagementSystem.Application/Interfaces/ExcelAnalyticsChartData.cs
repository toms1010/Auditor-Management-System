using AuditorManagementSystem.Core.DTOs;
using AuditorManagementSystem.Core.Entities;
using AuditorManagementSystem.Core.Enums;

namespace AuditorManagementSystem.Application.Interfaces;

/// <summary>
/// Represents a single raw row read from an uploaded Excel/CSV worksheet before it is mapped to the live database.
/// </summary>
/// <summary>
/// Structured data prepared for chart rendering and summary cards.
/// </summary>
public class ExcelAnalyticsChartData
{
    public ExcelWorkbookAnalysis? Analysis { get; set; }
    public int MaximumTransactions { get; set; }
    public int RowCount { get; set; }
    public bool HasData { get; set; }
    public List<string> RecognizedColumns { get; set; } = new();
    public List<string> DataColumns { get; set; } = new();
    public Dictionary<string, string> Headers { get; set; } = new();
    public WorkbookHeuristics Heuristics { get; set; } = new();
    public List<MonthlyIncomeExpense>? MonthlyTrend { get; set; }
    public List<ByDateGroup>? ByDate { get; set; }
    public List<IncomeByCategory>? IncomeExpense { get; set; }
    public List<CategorySummary>? CategoryTotals { get; set; }
    public List<SchoolMemberSummary>? SchoolMemberTotals { get; set; }
    public List<PaymentStatusSummary>? PaymentStatus { get; set; }
    public FinancialTotal FinancialTotals { get; set; } = new();
}

public class MonthlyIncomeExpense
{
    public string Month { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public decimal Net { get; set; }
}

public class ByDateGroup
{
    public string Period { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Net { get; set; }
}

public class IncomeByCategory
{
    public string Category { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Net { get; set; }
    public int Count { get; set; }
}

public class ExpenseByCategory : IncomeByCategory
{
    public string Category { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Net { get; set; }
    public int Count { get; set; }
}

public class CategorySummary
{
    public string Category { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public decimal Net { get; set; }
    public int Count { get; set; }
}

public class SchoolMemberSummary
{
    public string SchoolMember { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public decimal Net { get; set; }
    public int Count { get; set; }
}

public class PaymentStatusSummary
{
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public decimal Net { get; set; }
    public int Count { get; set; }
}

public class FinancialTotal
{
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public decimal Net => Income - Expenses;
    public DateTime? FirstDate { get; set; }
    public DateTime? LastDate { get; set; }
    public int TotalTransactions { get; set; }
}
