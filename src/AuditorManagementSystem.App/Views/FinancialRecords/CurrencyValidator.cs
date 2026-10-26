using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AuditorManagementSystem.App.Views.FinancialRecords;

public class CurrencyValidator
{
    public static CurrencyValidator Instance { get; } = new();

    public string? Execute(object sender, TextCompositionEventArgs e)
    {
        var allowed = new char[] { '.', ',' };
        if (char.IsDigit(e.Text, 0) || allowed.Contains(e.Text[0]))
        {
            e.Handled = false;
            return null;
        }
        e.Handled = true;
        return e.Text;
    }

    public bool CanExecute(object parameter) => true;

    public object Execute(object parameter, object commandParameter)
    {
        if (commandParameter is TextCompositionEventArgs e)
            return Execute(null, e);
        return parameter;
    }

    public event EventHandler? CanExecuteChanged;
}
