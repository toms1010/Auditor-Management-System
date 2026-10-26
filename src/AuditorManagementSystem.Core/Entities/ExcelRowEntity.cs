namespace AuditorManagementSystem.Core.Entities;

/// <summary>
/// Represents a single raw row read from an uploaded Excel/CSV worksheet before it is mapped to the live database.
/// </summary>
public class ExcelRowEntity : BaseEntity
{
    /// <summary>
    /// Zero-based row index within the worksheet (1-based for display). Row 1 is the header row.
    /// </summary>
    public int RowIndex { get; set; }

    /// <summary>
    /// Raw values as read from the worksheet, keyed by the column name the application detected.
    /// </summary>
    public Dictionary<string, object?> RawValues { get; set; } = new();

    /// <summary>
    /// Values that survived type detection / conversion. Null for columns that failed conversion.
    /// </summary>
    public Dictionary<string, object?> CleanValues { get; set; } = new();
}
