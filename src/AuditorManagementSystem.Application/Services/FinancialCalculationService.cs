namespace AuditorManagementSystem.Application.Services;

using AuditorManagementSystem.Application.Interfaces;
using AuditorManagementSystem.Core.DTOs;
using AuditorManagementSystem.Core.Enums;
using AuditorManagementSystem.Core.Interfaces;

public class FinancialCalculationService : IFinancialCalculationService
{
    private readonly ITransactionRepository _transactionRepository;

    public FinancialCalculationService(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var all = await _transactionRepository.GetAllAsync();

        var query = all.Where(t => !t.IsVoided && t.Status == TransactionStatus.Approved);

        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);

        var list = query.ToList();

        var dto = new DashboardSummaryDto
        {
            TotalIncome = list.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
            TotalExpenses = list.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
            TotalTransactions = list.Count,
            PendingApprovals = all.Count(t => t.ApprovalStatus == ApprovalStatus.Pending && !t.IsVoided),
        };

        // Receipts uploaded count is derived from transactions that have a receipt
        dto.ReceiptsUploaded = list.Count(t => !string.IsNullOrEmpty(t.ReceiptFileName));

        return dto;
    }

    public async Task<IEnumerable<CategoryTotalDto>> GetCategoryTotalsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var all = await _transactionRepository.GetAllAsync();
        var query = all.Where(t => !t.IsVoided && t.Status == TransactionStatus.Approved);

        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);

        return query
            .GroupBy(t => t.Category)
            .Select(g => new CategoryTotalDto
            {
                Category = g.Key,
                Income = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                Expenses = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
            })
            .OrderByDescending(c => c.Expenses + c.Income);
    }

    public async Task<IEnumerable<MonthlySummaryDto>> GetMonthlySummariesAsync(int months = 12)
    {
        var all = await _transactionRepository.GetAllAsync();
        var query = all.Where(t => !t.IsVoided && t.Status == TransactionStatus.Approved);

        var fromDate = DateTime.UtcNow.AddMonths(-(months - 1));
        query = query.Where(t => t.TransactionDate >= fromDate);

        return query
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
            .Select(g => new MonthlySummaryDto
            {
                Month = $"{g.Key.Year:D4}-{g.Key.Month:D2}",
                Income = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                Expenses = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
            })
            .OrderBy(m => m.Month);
    }

    public Task<decimal> GetTotalIncomeAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        return GetDashboardSummaryAsync(startDate, endDate).ContinueWith(t => t.Result.TotalIncome);
    }

    public Task<decimal> GetTotalExpensesAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        return GetDashboardSummaryAsync(startDate, endDate).ContinueWith(t => t.Result.TotalExpenses);
    }
}
