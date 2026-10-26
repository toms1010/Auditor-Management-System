using System.Windows.Controls;

namespace AuditorManagementSystem.App.Views;

/// <summary>
/// Base class for all page-style UserControls in the application.
/// </summary>
public abstract class BasePage : Page
{
    protected BasePage()
    {
    }

    public virtual void OnNavigatedTo() { }
    public virtual void OnNavigatedFrom() { }
}
