namespace AuditorManagementSystem.Infrastructure.Export;

using AuditorManagementSystem.Application.Interfaces;
using AuditorManagementSystem.Core.DTOs;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Exports financial data to CSV format.
/// </summary>
public class CsvExporter : ICsvExporter
{
    public byte[] ExportTransactions(IEnumerable<TransactionDto> transactions)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,ReferenceNumber,TransactionDate,Description,Category,Type,Amount,Payee,PaymentMethod,Status,ApprovalStatus,CreatedAt");
        foreach (var t in transactions)
        {
            sb.AppendLine($"{t.Id},{t.ReferenceNumber},{t.TransactionDate:yyyy-MM-dd},{Escape(t.Description)},{Escape(t.Category)},{t.Type},{t.Amount},{Escape(t.Payee)},{Escape(t.PaymentMethod)},{t.Status},{t.ApprovalStatus},{t.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
