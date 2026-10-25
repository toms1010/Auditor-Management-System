namespace AuditorManagementSystem.Application.Services;

using AuditorManagementSystem.Application.Interfaces;
using AuditorManagementSystem.Core.DTOs;
using System.Collections.Generic;

public class ReportService : IReportService
{
    private readonly ICsvExporter _csvExporter;

    public ReportService(ICsvExporter csvExporter)
    {
        _csvExporter = csvExporter;
    }

    public Task<byte[]> GenerateCsvReportAsync(string reportType, DateTime startDate, DateTime endDate)
    {
        // Placeholder: real implementation queries transactions and exports CSV
        var empty = Enumerable.Empty<TransactionDto>();
        return Task.FromResult(_csvExporter.ExportTransactions(empty));
    }

    public Task<byte[]> GeneratePdfReportAsync(string reportType, DateTime startDate, DateTime endDate)
    {
        throw new NotImplementedException("PDF report generation will be implemented with QuestPDF.");
    }
}

public class BackupService : IBackupService
{
    public Task<string> CreateBackupAsync()
    {
        throw new NotImplementedException("Backup will be implemented in a later phase.");
    }

    public Task<bool> RestoreBackupAsync(string backupPath)
    {
        throw new NotImplementedException("Restore will be implemented in a later phase.");
    }
}
