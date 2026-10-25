using AuditorManagementSystem.Application.Interfaces;
using AuditorManagementSystem.Core.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace AuditorManagementSystem.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ISessionService _sessionService;
    private readonly IAuthorizationService _authorizationService;

    [ObservableProperty]
    private UserDto? _currentUser;

    [ObservableProperty]
    private string _pageTitle = "Dashboard";

    [ObservableProperty]
    private object? _currentView;

    public ObservableCollection<NavItem> NavigationItems { get; } = new();

    public MainWindowViewModel(
        ISessionService sessionService,
        IAuthorizationService authorizationService)
    {
        _sessionService = sessionService;
        _authorizationService = authorizationService;

        _sessionService.UserChanged += OnUserChanged;
        _sessionService.UserLoggedOut += OnUserLoggedOut;

        RefreshNavigation();
    }

    private void OnUserChanged(object? sender, EventArgs e)
    {
        CurrentUser = _sessionService.CurrentUser;
        RefreshNavigation();
        PageTitle = "Dashboard";
        CurrentView = new Views.Dashboard.DashboardView();
    }

    private void OnUserLoggedOut(object? sender, EventArgs e)
    {
        CurrentUser = null;
        NavigationItems.Clear();
    }

    private void RefreshNavigation()
    {
        NavigationItems.Clear();
        var user = _sessionService.CurrentUser;
        if (user == null) return;

        var allItems = new[]
        {
            new NavItem("Dashboard", "Dashboard", "Dashboard", true),
            new NavItem("Transactions", "Transactions", "Transactions", _authorizationService.CanAccess(user, "Transactions")),
            new NavItem("Receipts", "Receipts", "Receipts", _authorizationService.CanAccess(user, "Receipts")),
            new NavItem("Analytics", "Analytics", "Analytics", _authorizationService.CanAccess(user, "Analytics")),
            new NavItem("Reports", "Reports", "Reports", _authorizationService.CanAccess(user, "Reports")),
            new NavItem("Audit Logs", "AuditLogs", "AuditLogs", _authorizationService.CanAccess(user, "AuditLogs")),
            new NavItem("Users", "Users", "Users", _authorizationService.CanAccess(user, "UserManagement")),
            new NavItem("Settings", "Settings", "Settings", _authorizationService.CanAccess(user, "Settings")),
            new NavItem("About", "About", "About", true),
        };

        foreach (var item in allItems.Where(i => i.IsVisible))
            NavigationItems.Add(item);

        // Logout always visible
        NavigationItems.Add(new NavItem("Logout", "Logout", "Logout", true));
    }

    [RelayCommand]
    private void Navigate(string? page)
    {
        if (page == null) return;

        if (page == "Logout")
        {
            _sessionService.SignOut();
            PageTitle = "Sign In";
            CurrentView = null;
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var login = new Views.Authentication.LoginWindow();
                login.Show();
                System.Windows.Application.Current.MainWindow?.Close();
            });
            return;
        }

        PageTitle = page;
        CurrentView = page switch
        {
            "Dashboard" => new Views.Dashboard.DashboardView(),
            "Transactions" => new Views.Transactions.TransactionsView(),
            "Receipts" => new Views.Receipts.ReceiptsView(),
            "Analytics" => new Views.Analytics.AnalyticsView(),
            "Reports" => new Views.Reports.ReportsView(),
            "AuditLogs" => new Views.AuditLogs.AuditLogsView(),
            "Users" => new Views.Users.UsersView(),
            "Settings" => new Views.Settings.SettingsView(),
            "About" => new Views.Settings.AboutView(),
            _ => new Views.Dashboard.DashboardView()
        };
    }
}

public class NavItem
{
    public string Title { get; set; } = string.Empty;
    public string Page { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsVisible { get; set; }

    public NavItem(string title, string page, string icon, bool isVisible)
    {
        Title = title;
        Page = page;
        Icon = icon;
        IsVisible = isVisible;
    }
}
