using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace AuditorManagementSystem.App.Converters;

/// <summary>
/// Converts a boolean to a Brush: true -> white (visible/active), false -> gray (hidden/disabled).
/// </summary>
public class BoolToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool visible = value is bool b && b;
        if (parameter?.ToString() == "Text")
            return visible ? Brushes.White : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"));
        return visible ? Brushes.White : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a enum value to a boolean (true if value matches the provided enum constant)
/// </summary>
public class EnumToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not Enum target)
            return false;
        return value?.Equals(target) == true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool b || !b)
            return DependencyProperty.UnsetValue;
        if (parameter is not Enum target)
            throw new InvalidOperationException();
        return target;
    }
}

/// <summary>
/// Converts a transaction status/approval status to a Brush color.
/// </summary>
public class StatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var status = value as string;
        if (string.IsNullOrWhiteSpace(status))
            return Brushes.Gray;

        return status.ToLowerInvariant() switch
        {
            "pending" or "under review" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")),
            "verified" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16A34A")),
            "flagged" or "rejected" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC2626")),
            _ => Brushes.Gray
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a binding direction enum to a bool.
/// </summary>
public class BindingDirectionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Simple passthrough conversion placeholder
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}

/// <summary>
/// Converts a decimal amount to a formatted currency string.
/// </summary>
public class CurrencyFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal d)
            return d.ToString("C2", CultureInfo.CurrentCulture);
        if (value is double dbl)
            return dbl.ToString("C2", CultureInfo.CurrentCulture);
        if (value is float f)
            return f.ToString("C2", CultureInfo.CurrentCulture);
        return value?.ToString() ?? "0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s && decimal.TryParse(s, NumberStyles.Currency, CultureInfo.CurrentCulture, out var d))
            return d;
        return value;
    }
}

/// <summary>
/// Converts a DateTime to a short date string.
/// </summary>
public class DateTimeFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dt)
            return dt.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
        return value?.ToString() ?? "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s && DateTime.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.None, out var dt))
            return dt;
        return value;
    }
}

/// <summary>
/// Converts a boolean to Visibility.
/// </summary>
public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool invert = parameter?.ToString()?.ToLower() == "invert";
        bool b = value is bool v ? v : false;
        return invert ? (b ? Visibility.Collapsed : Visibility.Visible) : (b ? Visibility.Visible : Visibility.Collapsed);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
