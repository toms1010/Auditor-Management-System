using System.Windows;
using AuditorManagementSystem.App.ViewModels;
using AuditorManagementSystem.App.Services;

namespace AuditorManagementSystem.App.Views.Shell;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = (MainWindowViewModel)DataContext;
        _viewModel.LoadSidebar();
        _viewModel.CurrentView = NavigationService.CreateInstance("Dashboard");
        _viewModel.Initialize();
    }

    private void Navigate(string? page)
    {
        if (page == null) return;
        _viewModel.NavigateCommand?.Execute(page);
    }

    private void Search_OnMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_viewModel.CurrentSearch != null)
        {
            _viewModel.CurrentSearch = null;
            return;
        }
        _viewModel.CurrentSearch = string.Empty;
    }
}
