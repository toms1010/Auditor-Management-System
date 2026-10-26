using System.Globalization;

namespace AuditorManagementSystem.Application.Services;

/// <summary>
/// Financial validation helpers for uploaded Excel rows.
/// Uses decimal arithmetic because financial calculations must not be subject to binary floating point rounding.
/// </summary>
public class ExcelFinancialValidator
{
    private const decimal MinPositiveAmount = 0.01m;
    private const decimal MaxAmount = 999999999999.99m;

    public static bool TryParseDecimal(object? value, out decimal result)
    {
        if (value is null)
        {
            result = 0;
            return false;
        }

        if (value is string s && decimal.TryParse(s.Trim(), NumberStyles.Currency | NumberStyles.Number, CultureInfo.CurrentCulture, out var parsed))
        {
            result = parsed;
            return true;
        }

        if (value is decimal d)
        {
            result = d;
            return true;
        }

        if (value is double d2 && decimal.TryParse(d2.ToString(CultureInfo.CurrentCulture), NumberStyles.Number, CultureInfo.CurrentCulture, out var parsed2))
        {
            result = parsed2;
            return true;
        }

        result = 0;
        return false;
    }

    public static decimal? ToDecimalOrNull(object? value)
    {
        if (value is null) return null;
        if (TryParseDecimal(value, out var d)) return d;
        return null;
    }

    public static decimal ToDecimalOrDefault(object? value, decimal defaultValue = 0)
    {
        if (value is null) return defaultValue;
        if (TryParseDecimal(value, out var d)) return d;
        return defaultValue;
    }

    public static DateTime? ToDateOrNull(object? value)
    {
        if (value is null) return null;

        if (value is DateTime dt) return dt;

        if (value is string s)
        {
            if (DateTime.TryParse(s.Trim(), CultureInfo.CurrentCulture, DateTimeStyles.None, out var dt2))
                return dt2;
            if (DateTime.TryParse(s.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt3))
                return dt3;
        }

        if (value is long l && l >= 10000000000L && l <= 99999999999999L)
        {
            try { return DateTime.FromFileTime(l); } catch { return null; }
        }

        if (value is double d && d > 0)
        {
            try { return DateTime.FromOADate(d); } catch { return null; }
        }

        return null;
    }

    public static bool IsValidDate(object? value)
    {
        return ToDateOrNull(value) != null;
    }

    public static bool IsValidAmount(object? value)
    {
        if (value is null) return false;
        if (TryParseDecimal(value, out var d))
            return d >= MinPositiveAmount && d <= MaxAmount;
        return false;
    }

    public static bool IsPositiveAmount(object? value)
    {
        if (value is null) return false;
        if (TryParseDecimal(value, out var d)) return d > 0;
        return false;
    }

    public static bool IsValidReference(object? value)
    {
        if (value is null) return false;
        var s = value.ToString()?.Trim();
        return !string.IsNullOrWhiteSpace(s) && s.Length >= 2 && s.Length <= 120;
    }

    public static bool IsValidCategory(object? value)
    {
        if (value is null) return false;
        var s = value.ToString()?.Trim();
        return !string.IsNullOrWhiteSpace(s) && s.Length <= 120;
    }

    public static bool IsValidText(object? value)
    {
        if (value is null) return false;
        var s = value.ToString();
        return !string.IsNullOrWhiteSpace(s) && s.Length <= 500;
    }
}
