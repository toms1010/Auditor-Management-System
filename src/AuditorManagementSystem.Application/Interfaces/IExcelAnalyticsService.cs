using AuditorManagementSystem.Core.DTOs;
using AuditorManagementSystem.Core.Entities;
using AuditorManagementSystem.Core.Enums;

namespace AuditorManagementSystem.Application.Interfaces;

/// <summary>
/// Result of validating an uploaded worksheet against a column mapping.
/// </summary>
public class WorksheetValidationResult
{
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int InvalidRows { get; set; }
    public int DuplicateRows { get; set; }
    public List<WorksheetValidationError> Errors { get; set; } = new();
    public Dictionary<string, object?> ColumnMap { get; set; } = new();
    public bool HasDateColumn { get; set; }
    public bool HasAmountColumn { get; set; }
    public bool HasCategoryColumn { get; set; }
    public bool HasSchoolMemberColumn { get; set; }
}

public class WorksheetValidationError
{
    public int RowIndex { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// A recognised financial column type used for automatic header matching.
/// </summary>
public enum FinancialColumnKind
{
    None,
    TransactionDate,
    ReferenceNumber,
    TransactionType,
    Description,
    Category,
    Amount,
    Income,
    Expense,
    SchoolMember,
    EventProject,
    ReceiptReference,
    PaymentStatus,
    AuditStatus,
    Other
}

/// <summary>
/// Represents an uploaded workbook that has been inspected and mapped.
/// </summary>
public class ExcelWorkbookAnalysis
{
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string WorksheetName { get; set; } = string.Empty;
    public int TotalDataRows { get; set; }
    public int RowCount { get; set; }
    public List<string> WorksheetNames { get; set; } = new();
    public Dictionary<string, FinancialColumnKind> ColumnKindMap { get; set; } = new();
    public Dictionary<string, int> ColumnIndexMap { get; set; } = new();
    public Dictionary<string, string> DetectedHeaders { get; set; } = new();
    public WorksheetValidationResult Validation { get; set; } = new();
    public List<ExcelRowEntity> PreviewRows { get; set; } = new();
    public bool HasData { get; set; }
    public bool IsEmpty { get; set; }
    public bool HasMergedCells { get; set; }
    public bool HasInvalidFormat { get; set; }
    public bool HeadersMissing { get; set; }

    public WorkbookHeuristics Heuristics { get; set; } = new();
}

public class WorkbookHeuristics
{
    public bool FileRejected { get; set; }
    public string? RejectedReason { get; set; }
    public int EstimatedDataRows { get; set; }
    public bool HasMergedCells { get; set; }
    public bool MergedCells { get; set; }
}
