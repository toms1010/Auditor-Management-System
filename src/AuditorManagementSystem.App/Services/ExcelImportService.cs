using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using AuditorManagementSystem.Application.Interfaces;
using AuditorManagementSystem.Core.DTOs;
using AuditorManagementSystem.Core.Entities;
using AuditorManagementSystem.Core.Enums;

namespace AuditorManagementSystem.Application.Services;

/// <summary>
/// Reads and inspects Excel/CSV workbooks without touching the database.
/// Kept free of WPF dependencies so it can be unit-tested on Linux.
/// </summary>
public class ExcelImportService : IExcelImportService
{
    private const int MaxWorkbookSizeBytes = 50 * 1024 * 1024; // 50 MB
    private const int MaxRowCount = 200_000;

    public async Task<ExcelWorkbookAnalysis> AnalyzeAsync(string physicalPath, string? worksheetName = null, CancellationToken ct = default)
    {
        var analysis = new ExcelWorkbookAnalysis
        {
            FileName = Path.GetFileName(physicalPath),
            FileSizeBytes = new FileInfo(physicalPath).Length
        };

        using var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);

        if (!IsSafeWorkbook(stream, physicalPath))
        {
            analysis.HasInvalidFormat = true;
            analysis.Heuristics = new WorkbookHeuristics { FileRejected = true };
            return analysis;
        }

        using var workbook = new XLWorkbook(stream);
        analysis.WorksheetNames.AddRange(workbook.Worksheets.Select(w => w.Name));

        IXLWorksheet sheet = null!;
        if (worksheetName != null)
        {
            sheet = workbook.Worksheets.FirstOrDefault(w => string.Equals(w.Name, worksheetName, StringComparison.OrdinalIgnoreCase));
            if (sheet == null)
            {
                analysis.WorksheetName = worksheetName;
                analysis.WorksheetNames.Add(worksheetName);
                analysis.HasData = false;
                analysis.IsEmpty = true;
                return analysis;
            }
            analysis.WorksheetName = sheet.Name;
            stream.Seek(0, SeekOrigin.Begin);
            using var reset = new MemoryStream();
            sheet.Workbook.SaveAs(reset);
            reset.Seek(0, SeekOrigin.Begin);
            var newWorkbook = new XLWorkbook(reset);
            sheet = newWorkbook.Worksheet(1);
        }
        else
        {
            sheet = workbook.Worksheet(1);
            analysis.WorksheetName = sheet.Name;
        }

        analysis.RowCount = (int)sheet.RangeUsed().RowCount();

        var headerRow = sheet.RangeUsed().Row(1);
        analysis.ColumnIndexMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        analysis.ColumnKindMap = new Dictionary<string, FinancialColumnKind>(StringComparer.OrdinalIgnoreCase);
        analysis.DetectedHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var headerCells = headerRow.CellsUsed().ToList();
        for (int c = 1; c <= headerCells.Count; c++)
        {
            var raw = headerCells[c - 1].GetString();
            if (string.IsNullOrWhiteSpace(raw))
                continue;
            var key = NormalizeHeader(raw.Trim());
            if (analysis.ColumnIndexMap.ContainsKey(key)) continue;
            analysis.ColumnIndexMap[key] = c - 1;
            analysis.DetectedHeaders[key] = raw.Trim();
            analysis.ColumnKindMap[key] = ClassifyHeader(raw.Trim());
        }

        if (analysis.ColumnIndexMap.Count == 0)
        {
            analysis.HeadersMissing = true;
            analysis.HasData = false;
            analysis.IsEmpty = true;
            return analysis;
        }

        var recognizedColumns = analysis.ColumnIndexMap.Keys.ToList();
        analysis.HasData = true;

        var deduped = DedupeByKeyColumn(analysis, recognizedColumns).ToList();
        var remainder = analysis.TotalDataRows - deduped.Count;
        analysis.TotalDataRows = deduped.Count;

        var validation = new WorksheetValidationResult
        {
            TotalRows = analysis.TotalDataRows,
            ColumnMap = analysis.ColumnIndexMap.ToDictionary(k => k.Key, v => (object?)v.Value),
            HasDateColumn = analysis.ColumnKindMap.Values.Contains(FinancialColumnKind.TransactionDate),
            HasAmountColumn = analysis.ColumnKindMap.Values.Contains(FinancialColumnKind.Amount),
            HasCategoryColumn = analysis.ColumnKindMap.Values.Contains(FinancialColumnKind.Category),
            HasSchoolMemberColumn = analysis.ColumnKindMap.Values.Contains(FinancialColumnKind.SchoolMember)
        };

        foreach (var col in recognizedColumns)
        {
            if (analysis.ColumnKindMap[col] == FinancialColumnKind.ReferenceNumber)
                validation.ColumnMap.Add("reference", col);
            if (analysis.ColumnKindMap[col] == FinancialColumnKind.Category)
                validation.ColumnMap.Add("category", col);
            if (analysis.ColumnKindMap[col] == FinancialColumnKind.SchoolMember)
                validation.ColumnMap.Add("schoolMember", col);
        }

        if (analysis.ColumnKindMap.Values.Contains(FinancialColumnKind.TransactionDate))
            validation.ColumnMap.Add("date", recognizedColumns.FirstOrDefault(c => analysis.ColumnKindMap[c] == FinancialColumnKind.TransactionDate));
        if (analysis.ColumnKindMap.Values.Contains(FinancialColumnKind.Amount))
            validation.ColumnMap.Add("amount", recognizedColumns.FirstOrDefault(c => analysis.ColumnKindMap[c] == FinancialColumnKind.Amount));

        if (analysis.ColumnKindMap.Values.Contains(FinancialColumnKind.TransactionDate) && analysis.ColumnKindMap.Values.Contains(FinancialColumnKind.Amount))
            validation.ColumnMap.Add("dateAndAmount", true);

        var malformed = 0;
        var duplicateRefs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenRefs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pending = new List<ExcelRowEntity>();

        var validDateCount = 0;
        var validAmountCount = 0;

        foreach (var row in deduped)
        {
            var values = row.CleanValues;

            var dateValue = values.TryGetValue("date", out var dv) ? dv : null;
            var amountValue = values.TryGetValue("amount", out var av) ? av : null;

            var dateOk = analysis.ColumnKindMap.ContainsValue(FinancialColumnKind.TransactionDate) ? ExcelFinancialValidator.IsValidDate(dateValue) : true;
            var amountOk = analysis.ColumnKindMap.ContainsValue(FinancialColumnKind.Amount) ? ExcelFinancialValidator.IsValidAmount(amountValue) : true;

            if (!dateOk || !amountOk)
            {
                malformed++;
                continue;
            }

            if (analysis.ColumnKindMap.Values.Contains(FinancialColumnKind.TransactionDate) && !dateOk) validDateCount++;
            if (analysis.ColumnKindMap.Values.Contains(FinancialColumnKind.Amount) && !amountOk) validAmountCount++;

            var refText = values.TryGetValue("reference", out var rfValue) ? rfValue?.ToString() : null;
            refText = refText?.Trim();
            var referenceOk = analysis.ColumnKindMap.ContainsValue(FinancialColumnKind.TransactionDate) || analysis.ColumnKindMap.ContainsValue(FinancialColumnKind.Amount) || string.IsNullOrEmpty(refText) || ExcelFinancialValidator.IsValidReference(refText);

            if (!string.IsNullOrEmpty(refText))
            {
                if (!seenRefs.Add(refText))
                {
                    duplicateRefs.Add(refText);
                    validation.DuplicateRows++;
                    continue;
                }
            }

            if (referenceOk && dateOk)
            {
                pending.Add(row);
                validDateCount++;
                validAmountCount++;
            }
            else if (referenceOk && !dateOk)
            {
                validation.Errors.Add(new WorksheetValidationError { RowIndex = row.RowIndex, Field = "date", Message = "Date value is not a valid date." });
            }
            else if (!referenceOk && dateOk)
            {
                validation.Errors.Add(new WorksheetValidationError { RowIndex = row.RowIndex, Field = "reference", Message = "Reference value is invalid." });
            }
            else
            {
                validation.Errors.Add(new WorksheetValidationError { RowIndex = row.RowIndex, Field = "date, reference", Message = "Date and reference are invalid." });
            }
        }

        validation.ValidRows = pending.Count;
        validation.InvalidRows = malformed;
        analysis.Validation = validation;

        var previewCount = Math.Min(10, pending.Count);
        foreach (var row in pending.Take(previewCount))
        {
            analysis.PreviewRows.Add(row);
        }

        return analysis;
    }

    private static IReadOnlyList<ExcelRowEntity> DedupeByKeyColumn(ExcelWorkbookAnalysis analysis, List<string> recognizedColumns)
    {
        var keyCandidate = recognizedColumns.FirstOrDefault(c =>
        {
            if (c.Equals("reference", StringComparison.OrdinalIgnoreCase)) return true;
            if (c.Equals("referenceNumber", StringComparison.OrdinalIgnoreCase)) return true;
            if (c.Equals("transactionReference", StringComparison.OrdinalIgnoreCase)) return true;
            if (c.Equals("txnReference", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        });

        var deduped = new List<ExcelRowEntity>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in analysis.PreviewRows)
        {
            var keyValue = keyCandidate != null && row.CleanValues.TryGetValue(keyCandidate, out var v) ? v?.ToString()?.Trim() : row.RowIndex.ToString(CultureInfo.InvariantCulture);
            if (string.IsNullOrEmpty(keyValue)) { deduped.Add(row); continue; }
            if (seen.Add(keyValue)) deduped.Add(row);
        }
        return deduped;
    }

    private static FinancialColumnKind ClassifyHeader(string header)
    {
        var h = header.ToLowerInvariant();
        if (h.Contains("date") || h.Contains("transact") || h.Contains("posted") || h.Contains("calendar")) return FinancialColumnKind.TransactionDate;
        if (h.Contains("reference") || h.Contains("ref") || h.Contains("inv") || h.Contains("num")) return FinancialColumnKind.ReferenceNumber;
        if (h.Contains("type") || h.Contains("kind") || h.Contains("category") || h.Contains("income") || h.Contains("expense")) return FinancialColumnKind.TransactionType;
        if (h.Contains("desc") || h.Contains("memo") || h.Contains("note") || h.Contains("detail") || h.Contains("narrative")) return FinancialColumnKind.Description;
        if (h.Contains("category") || h.Contains("type") || h.Contains("income") || h.Contains("expense") || h.Contains("class")) return FinancialColumnKind.Category;
        if (h.Contains("school") || h.Contains("member") || h.Contains("participant") || h.Contains("payee") || h.Contains("person") || h.Contains("org")) return FinancialColumnKind.SchoolMember;
        if (h.Contains("amount") || h.Contains("value") || h.Contains("cost") || h.Contains("total") || h.Contains("sum") || h.Contains("money") || h.Contains("balance")) return FinancialColumnKind.Amount;
        if (h.Contains("collection") || h.Contains("income") || h.Contains("received") || h.Contains("deposit") || h.Contains("payment")) return FinancialColumnKind.Income;
        if (h.Contains("expense") || h.Contains("payment") || h.Contains("spend") || h.Contains("withdrawal") || h.Contains("disbursement")) return FinancialColumnKind.Expense;
        if (h.Contains("event") || h.Contains("project") || h.Contains("fund") || h.Contains("program")) return FinancialColumnKind.EventProject;
        if (h.Contains("receipt") || h.Contains("invoice")) return FinancialColumnKind.ReceiptReference;
        if (h.Contains("payment") && (h.Contains("status") || h.Contains("status"))) return FinancialColumnKind.PaymentStatus;
        if (h.Contains("audit") || h.Contains("verify") || h.Contains("approved") || h.Contains("review") || h.Contains("status")) return FinancialColumnKind.AuditStatus;
        return FinancialColumnKind.Other;
    }

    private static string NormalizeHeader(string header)
    {
        return string.Join(" ", header.Split(new[] { ' ', '_', '-', '/', '\\', '.' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => char.IsUpper(s[0]) ? s[0].ToString().ToUpperInvariant() + s.Substring(1).ToLowerInvariant() : s.ToLowerInvariant()));
    }

    private bool IsSafeWorkbook(Stream stream, string physicalPath)
    {
        stream.Position = 0;
        var header = new byte[8];
        if (stream.Read(header, 0, header.Length) != header.Length) return false;
        stream.Position = 0;

        var ext = Path.GetExtension(physicalPath).ToLowerInvariant();
        if (ext == ".xls")
        {
            if (header[0] == 0x00 && header[1] == 0x09 && header[2] == 0x08) return false;
            return true;
        }

        if (ext == ".xlsx")
        {
            return header[0] == 0x50 && header[1] == 0x4B && header[2] == 0x03 && header[3] == 0x04;
        }

        if (ext == ".csv")
        {
            var peek = new byte[16];
            if (stream.Read(peek, 0, peek.Length) != peek.Length) return false;
            stream.Position = 0;
            var peekStr = System.Text.Encoding.UTF8.GetString(peek).TrimStart();
            return !peekStr.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) && !peekStr.StartsWith("<?mso", StringComparison.OrdinalIgnoreCase);
        }

        try
        {
            using var fb = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
            var ar = new byte[16];
            if (fb.Read(ar, 0, ar.Length) != ar.Length) return false;
            var sig = System.Text.Encoding.UTF8.GetString(ar, 0, 8);
            return !sig.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) && !sig.StartsWith("<?mso", StringComparison.OrdinalIgnoreCase) && !sig.StartsWith("<mso");
        }
        catch { return false; }
    }

    public async Task<WorkbookConflict> CheckWorkbookConflictAsync(string physicalPath, CancellationToken ct = default)
    {
        var conflict = new WorkbookConflict();
        var fileName = Path.GetFileName(physicalPath);
        var storagePath = Path.Combine(AppContext.BaseDirectory, "UploadedWorkbooks");
        if (!Directory.Exists(storagePath))
            return conflict;

        var existingFiles = Directory.GetFiles(storagePath, fileName);
        if (existingFiles.Length > 0)
        {
            conflict.Exists = true;
            conflict.DuplicateFileName = existingFiles[0];
            try
            {
                using var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
                using var workbook = new XLWorkbook(stream);
                foreach (var sheet in workbook.Worksheets)
                {
                    foreach (var row in sheet.Rows())
                    {
                        var refVal = row.Cell(1).GetValue<string>();
                        if (!string.IsNullOrEmpty(refVal) && refVal != "Reference Number" && refVal.ToUpperInvariant().StartsWith("FIN-"))
                        {
                            conflict.DuplicateReferences.Add(refVal);
                        }
                    }
                }
            }
            catch { }
        }

        return conflict;
    }
}
