using System.Globalization;
using AuditorManagementSystem.Application.Interfaces;
using AuditorManagementSystem.Core.Entities;
using AuditorManagementSystem.Core.Enums;

namespace AuditorManagementSystem.Application.Services;

/// <summary>
/// Prepares calculated summary data from an ExcelWorkbookAnalysis.
/// Charts are rendered by the WPF frontend with a compatible charting library.
/// </summary>
public class ExcelAnalyticsDataPreparer
{
    public ExcelAnalyticsChartData PrepareCharts(ExcelWorkbookAnalysis analysis)
    {
        var data = new ExcelAnalyticsChartData
        {
            MaximumTransactions = analysis.Validation.TotalRows,
            Analysis = analysis
        };

        var recognized = analysis.ColumnKindMap.Keys.ToList();

        if (analysis.Validation.HasDateColumn && analysis.Validation.HasAmountColumn)
        {
            var monthly = PrepareMonthlyTrendSeries(analysis).ToList();
            if (monthly.Any())
                data.MonthlyTrend = monthly;

            var byDate = PrepareCategoryOverTimeSeries(analysis).ToList();
            if (byDate.Any())
                data.ByDate = byDate;
        }

        if (analysis.Validation.HasAmountColumn)
        {
            var income = PrepareIncomeBySeries(analysis, recognized).ToList();
            var expense = PrepareExpenseBySeries(analysis, recognized).ToList();
            if (income.Any() || expense.Any())
                data.IncomeExpense = income.Concat(expense).ToList();
        }

        if (analysis.Validation.HasDateColumn || analysis.Validation.HasAmountColumn)
        {
            if (analysis.Validation.HasCategoryColumn)
                data.CategoryTotals = PrepareCategorySummary(analysis).ToList();
            if (analysis.Validation.HasSchoolMemberColumn)
                data.SchoolMemberTotals = PrepareSchoolMemberSummary(analysis).ToList();
        }

        if (analysis.Validation.HasSchoolMemberColumn)
            data.SchoolMemberTotals = PrepareSchoolMemberSummary(analysis).ToList();

        if (analysis.Validation.HasDateColumn && analysis.Validation.HasAmountColumn)
            data.PaymentStatus = PreparePaymentStatusSummary(analysis).ToList();

        data.FinancialTotals = ComputeFinancialTotals(analysis);
        data.RowCount = analysis.Validation.TotalRows;
        data.RecognizedColumns = analysis.ColumnKindMap.Keys.ToList();
        data.DataColumns = analysis.ColumnIndexMap.Keys.ToList();
        data.HasData = analysis.HasData && !analysis.IsEmpty;
        data.Headers = analysis.DetectedHeaders;

        if (analysis.Heuristics.HasMergedCells)
            data.Heuristics.HasMergedCells = true;

        return data;
    }

    private static decimal ComputeTotal(IEnumerable<ExcelRowEntity> rows, string key, bool positiveSum)
    {
        return rows
            .Where(r => r.CleanValues.TryGetValue(key, out var v) && v != null)
            .Sum(r =>
            {
                if (r.CleanValues.TryGetValue(key, out var av) && av != null)
                    return ExcelFinancialValidator.ToDecimalOrDefault(av, 0);
                return 0m;
            });
    }

    private static decimal ComputePositiveSum(IEnumerable<ExcelRowEntity> rows, string key)
    {
        return rows
            .Where(r => r.CleanValues.TryGetValue(key, out var v) && v != null)
            .Where(r => {
                var d = ExcelFinancialValidator.ToDecimalOrDefault(r.CleanValues[key], 0);
                return d > 0;
            })
            .Sum(r => ExcelFinancialValidator.ToDecimalOrDefault(r.CleanValues[key], 0));
    }

    private static decimal ComputeNegativeSum(IEnumerable<ExcelRowEntity> rows, string key)
    {
        return rows
            .Where(r => r.CleanValues.TryGetValue(key, out var v) && v != null)
            .Where(r => {
                var d = ExcelFinancialValidator.ToDecimalOrDefault(r.CleanValues[key], 0);
                return d < 0;
            })
            .Sum(r => ExcelFinancialValidator.ToDecimalOrDefault(r.CleanValues[key], 0));
    }

    private static IEnumerable<MonthlyIncomeExpense> PrepareMonthlyTrendSeries(ExcelWorkbookAnalysis analysis)
    {
        var dateCol = analysis.ColumnKindMap.Keys.FirstOrDefault(c => analysis.ColumnKindMap.TryGetValue(c, out var v) && v == FinancialColumnKind.TransactionDate);
        var amountCol = analysis.ColumnKindMap.Keys.FirstOrDefault(c => analysis.ColumnKindMap.TryGetValue(c, out var v) && v == FinancialColumnKind.Amount);

        if (string.IsNullOrEmpty(dateCol) || string.IsNullOrEmpty(amountCol))
            yield break;

        var groups = analysis.PreviewRows
            .Where(r => r.CleanValues.TryGetValue(dateCol, out var dv) && ExcelFinancialValidator.IsValidDate(dv) &&
                        r.CleanValues.TryGetValue(amountCol, out var av) && av != null)
            .GroupBy(r =>
            {
                var d = ExcelFinancialValidator.ToDateOrNull(r.CleanValues[dateCol])!.Value;
                return d.ToString("yyyy-MM", CultureInfo.InvariantCulture);
            })
            .OrderBy(g => g.Key);

        foreach (var g in groups)
        {
            var income = ComputePositiveSum(g, amountCol);
            var expense = ComputeNegativeSum(g, amountCol);
            yield return new MonthlyIncomeExpense
            {
                Month = g.Key,
                Income = income,
                Expenses = expense,
                Net = income + expense
            };
        }
    }

    private static IEnumerable<ByDateGroup> PrepareCategoryOverTimeSeries(ExcelWorkbookAnalysis analysis)
    {
        var recognized = analysis.ColumnKindMap.Keys.ToList();
        var dateCol = recognized.FirstOrDefault(c => analysis.ColumnKindMap.TryGetValue(c, out var v) && v == FinancialColumnKind.TransactionDate);
        var categoryCol = recognized.FirstOrDefault(c => analysis.ColumnKindMap.TryGetValue(c, out var v) && v == FinancialColumnKind.Category);
        var amountCol = recognized.FirstOrDefault(c => analysis.ColumnKindMap.TryGetValue(c, out var v) && v == FinancialColumnKind.Amount);

        if (string.IsNullOrEmpty(dateCol) || string.IsNullOrEmpty(categoryCol) || string.IsNullOrEmpty(amountCol))
            yield break;

        var groups = analysis.PreviewRows
            .Where(r => r.CleanValues.TryGetValue(dateCol, out var dv) && ExcelFinancialValidator.IsValidDate(dv) &&
                        r.CleanValues.TryGetValue(categoryCol, out var cv) && cv != null &&
                        r.CleanValues.TryGetValue(amountCol, out var av) && av != null)
            .GroupBy(r => ExcelFinancialValidator.ToDateOrNull(r.CleanValues[dateCol])!.Value.ToString("yyyy-MM", CultureInfo.InvariantCulture))
            .OrderBy(g => g.Key);

        foreach (var g in groups)
        {
            var income = ComputePositiveSum(g, amountCol);
            var expense = ComputeNegativeSum(g, amountCol);
            yield return new ByDateGroup
            {
                Period = g.Key,
                Category = g.First().CleanValues[categoryCol]?.ToString() ?? "",
                Income = income,
                Expense = expense,
                Net = income + expense
            };
        }
    }

    private static IEnumerable<IncomeByCategory> PrepareIncomeBySeries(ExcelWorkbookAnalysis analysis, List<string> recognized)
    {
        var categoryCol = recognized.FirstOrDefault(c => analysis.ColumnKindMap.TryGetValue(c, out var v) && v == FinancialColumnKind.Category);
        var amountCol = recognized.FirstOrDefault(c => analysis.ColumnKindMap.TryGetValue(c, out var v) && v == FinancialColumnKind.Amount);

        if (string.IsNullOrEmpty(categoryCol) || string.IsNullOrEmpty(amountCol))
            yield break;

        var groups = analysis.PreviewRows
            .Where(r => r.CleanValues.TryGetValue(categoryCol, out var cv) && cv != null &&
                        r.CleanValues.TryGetValue(amountCol, out var av) && av != null)
            .GroupBy(r => r.CleanValues[categoryCol]?.ToString() ?? "")
            .Where(g => g.Key != null && g.Key != "");

        foreach (var g in groups)
        {
            var income = ComputePositiveSum(g, amountCol);
            if (income > 0m)
                yield return new IncomeByCategory
                {
                    Category = g.Key,
                    Income = income,
                    Expense = 0,
                    Net = income,
                    Count = g.Count()
                };
        }
    }

    private static IEnumerable<ExpenseByCategory> PrepareExpenseBySeries(ExcelWorkbookAnalysis analysis, List<string> recognized)
    {
        var categoryCol = recognized.FirstOrDefault(c => analysis.ColumnKindMap.TryGetValue(c, out var v) && v == FinancialColumnKind.Category);
        var amountCol = recognized.FirstOrDefault(c => analysis.ColumnKindMap.TryGetValue(c, out var v) && v == FinancialColumnKind.Amount);

        if (string.IsNullOrEmpty(categoryCol) || string.IsNullOrEmpty(amountCol))
            yield break;

        var groups = analysis.PreviewRows
            .Where(r => r.CleanValues.TryGetValue(categoryCol, out var cv) && cv != null &&
                        r.CleanValues.TryGetValue(amountCol, out var av) && av != null)
            .GroupBy(r => r.CleanValues[categoryCol]?.ToString() ?? "")
            .Where(g => g.Key != null && g.Key != "");

        foreach (var g in groups)
        {
            var expense = ComputeNegativeSum(g, amountCol);
            if (expense < 0m)
                yield return new ExpenseByCategory
                {
                    Category = g.Key,
                    Income = 0,
                    Expense = expense,
                    Net = Math.Abs(expense),
                    Count = g.Count()
                };
        }
    }

    private static IEnumerable<CategorySummary> PrepareCategorySummary(ExcelWorkbookAnalysis analysis)
    {
        var categoryCol = analysis.ColumnKindMap.Keys.FirstOrDefault(c => analysis.ColumnKindMap.TryGetValue(c, out var v) && v == FinancialColumnKind.Category);
        var amountCol = analysis.ColumnKindMap.Keys.FirstOrDefault(c => analysis.ColumnKindMap.TryGetValue(c, out var v) && v == FinancialColumnKind.Amount);

        if (string.IsNullOrEmpty(categoryCol) || string.IsNullOrEmpty(amountCol))
            yield break;

        var groups = analysis.PreviewRows
            .Where(r => r.CleanValues.TryGetValue(categoryCol, out var cv) && cv != null &&
                        r.CleanValues.TryGetValue(amountCol, out var av) && av != null)
            .GroupBy(r => r.CleanValues[categoryCol]?.ToString() ?? "");

        foreach (var g in groups)
        {
            var income = ComputePositiveSum(g, amountCol);
            var expense = ComputeNegativeSum(g, amountCol);
            if (income > 0m || expense < 0m)
                yield return new CategorySummary
                {
                    Category = g.Key,
                    Income = income,
                    Expenses = expense,
                    Net = Math.Abs(income - expense),
                    Count = g.Count()
                };
        }
    }

    private static IEnumerable<SchoolMemberSummary> PrepareSchoolMemberSummary(ExcelWorkbookAnalysis analysis)
    {
        var schoolCol = analysis.ColumnKindMap.Keys.FirstOrDefault(c => analysis.ColumnKindMap.TryGetValue(c, out var v) && v == FinancialColumnKind.SchoolMember);
        var amountCol = analysis.ColumnKindMap.Keys.FirstOrDefault(c => analysis.ColumnKindMap.TryGetValue(c, out var v) && v == FinancialColumnKind.Amount);

        if (string.IsNullOrEmpty(schoolCol) || string.IsNullOrEmpty(amountCol))
            yield break;

        var groups = analysis.PreviewRows
            .Where(r => r.CleanValues.TryGetValue(schoolCol, out var cv) && cv != null &&
                        r.CleanValues.TryGetValue(amountCol, out var av) && av != null)
            .GroupBy(r => r.CleanValues[schoolCol]?.ToString() ?? "");

        foreach (var g in groups)
        {
            var income = ComputePositiveSum(g, amountCol);
            var expense = ComputeNegativeSum(g, amountCol);
            if (income > 0m || expense < 0m)
                yield return new SchoolMemberSummary
                {
                    SchoolMember = g.Key,
                    Income = income,
                    Expenses = expense,
                    Net = Math.Abs(income - expense),
                    Count = g.Count()
                };
        }
    }

    private static IEnumerable<PaymentStatusSummary> PreparePaymentStatusSummary(ExcelWorkbookAnalysis analysis)
    {
        var statusCol = analysis.ColumnKindMap.Keys.FirstOrDefault(c => analysis.ColumnKindMap.TryGetValue(c, out var v) && v == FinancialColumnKind.PaymentStatus);
        var amountCol = analysis.ColumnKindMap.Keys.FirstOrDefault(c => analysis.ColumnKindMap.TryGetValue(c, out var v) && v == FinancialColumnKind.Amount);

        if (string.IsNullOrEmpty(statusCol) || string.IsNullOrEmpty(amountCol))
            yield break;

        var groups = analysis.PreviewRows
            .Where(r => r.CleanValues.TryGetValue(statusCol, out var cv) && cv != null)
            .GroupBy(r => r.CleanValues[statusCol]?.ToString() ?? "");

        foreach (var g in groups)
        {
            var income = ComputePositiveSum(g, amountCol);
            var expense = ComputeNegativeSum(g, amountCol);
            if (income > 0m || expense < 0m)
                yield return new PaymentStatusSummary
                {
                    PaymentStatus = g.Key,
                    Income = income,
                    Expenses = expense,
                    Net = Math.Abs(income - expense),
                    Count = g.Count()
                };
        }
    }

    private static FinancialTotal ComputeFinancialTotals(ExcelWorkbookAnalysis analysis)
    {
        var totals = new FinancialTotal();

        if (analysis.Validation.HasAmountColumn)
        {
            var amountRows = analysis.PreviewRows.Where(r => r.CleanValues.TryGetValue("amount", out var v) && v != null);
            var income = ComputePositiveSum(amountRows, "amount");
            var expense = ComputeNegativeSum(amountRows, "amount");
            totals.Income += income;
            totals.Expenses += expense;
        }

        if (analysis.Validation.HasDateColumn)
        {
            var dateRows = analysis.PreviewRows.Where(r => r.CleanValues.TryGetValue("date", out var v) && v != null);
            var dates = dateRows
                .Select(r => ExcelFinancialValidator.ToDateOrNull(r.CleanValues["date"]))
                .Where(d => d != null)
                .Select(d => d!.Value);
            if (dates.Any())
            {
                totals.FirstDate = dates.Min();
                totals.LastDate = dates.Max();
            }
        }

        return totals;
    }
}
