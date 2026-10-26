using System.Globalization;
using ClosedXML.Excel;
using AuditorManagementSystem.Application.Interfaces;
using AuditorManagementSystem.Core.DTOs;
using AuditorManagementSystem.Core.Entities;

namespace AuditorManagementSystem.Application.Interfaces;

/// <summary>
/// Reads and inspects Excel/CSV workbooks without touching the database.
/// Kept free of WPF dependencies so it can be unit-tested on Linux.
/// </summary>
public interface IExcelImportService
{
    Task<ExcelWorkbookAnalysis> AnalyzeAsync(string physicalPath, string? worksheetName = null, CancellationToken ct = default);
    Task<WorkbookConflict> CheckWorkbookConflictAsync(string physicalPath, CancellationToken ct = default);
}

public class WorkbookConflict
{
    public bool Exists { get; set; }
    public string? DuplicateFileName { get; set; }
    public List<string> DuplicateReferences { get; set; } = new();
    public string? ConflictDetails { get; set; }
}
