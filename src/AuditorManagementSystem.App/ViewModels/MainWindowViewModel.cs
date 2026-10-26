using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AuditorManagementSystem.App.Services;
using AuditorManagementSystem.App.Views;
using AuditorManagementSystem.App.Views.Receipts;
using AuditorManagementSystem.App.Views.Dashboard;
using AuditorManagementSystem.App.Views.Reports;
using AuditorManagementSystem.Core.DTOs;
using AuditorManagementSystem.Core.Enums;
using AuditorManagementSystem.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuditorManagementSystem.App.ViewModels;

public class NavItem : ObservableObject
{
    private bool _isActive;
    public string Title { get; set; } = string.Empty;
    public string Page { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            OnPropertyChanged();
        }
    }

    public NavItem() { }

    public NavItem(string title, string page, string icon, bool isVisible = true)
    {
        Title = title;
        Page = page;
        Icon = icon;
        IsVisible = isVisible;
    }
}

public class SidebarGroup
{
    public string Name { get; set; } = string.Empty;
    public List<NavItem> Items { get; set; } = new();

    public SidebarGroup() { }

    public SidebarGroup(string name) => Name = name;
}

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ISessionService _sessionService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IAuditLogService _auditLogService;
    private readonly ITransactionService _transactionService;

    [ObservableProperty]
    private UserDto? _currentUser;

    [ObservableProperty]
    private string _pageTitle = "Dashboard";

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string? _currentSearch;

    [ObservableProperty]
    private NavItem? _selectedNavItem;

    public ObservableCollection<SidebarGroup> SidebarGroups { get; } = new();

    private readonly ObservableCollection<NavItem> _sidebarItems = new();

    public ObservableCollection<NavItem> SidebarItems => _sidebarItems;

    private void OnUserChanged(object? sender, EventArgs e)
    {
        CurrentUser = _sessionService.CurrentUser;
        LoadSidebar();
        PageTitle = "Dashboard";
        if (CurrentView == null)
            CurrentView = NavigationService.CreateInstance("Dashboard");
    }

    private void OnUserLoggedOut(object? sender, EventArgs e)
    {
        CurrentUser = null;
        SidebarGroups.Clear();
        _sidebarItems.Clear();
        CurrentView = null;
    }

    public void LoadSidebar()
    {
        SidebarGroups.Clear();
        _sidebarItems.Clear();

        if (CurrentUser == null) return;

        var groups = new List<SidebarGroup>
        {
            new SidebarGroup("OVERVIEW")
            {
                Items =
                {
                    new NavItem("Dashboard", "Dashboard", "📊", true) { IsActive = (CurrentView as BasePage)?.GetType().Name == "DashboardView" }
                }
            },
            new SidebarGroup("FINANCIAL OPERATIONS")
            {
                Items =
                {
                    new NavItem("Transaction Entry", "TransactionEntry", "➕", true),
                    new NavItem("Transaction Ledger", "TransactionLedger", "📋", true),
                    new NavItem("Collections", "Collections", "💰", true),
                    new NavItem("Expenses", "Expenses", "📉", true),
                    new NavItem("Cash Balance", "CashBalance", "💵", true),
                    new NavItem("Fund Management", "FundManagement", "🏦", true),
                    new NavItem("Budget Monitoring", "BudgetMonitoring", "📊", true),
                    new NavItem("Reimbursements", "Reimbursements", "🛠️", true),
                    new NavItem("Liquidation", "Liquidations", "📤", true),
                    new NavItem("Bank Reconciliation", "BankReconciliation", "🏦", true),
                    new NavItem("Dues and Payments", "Dues", "📌", true),
                    new NavItem("Financial Adjustments", "FinancialAdjustments", "⚖️", true),
                }
            },
            new SidebarGroup("DOCUMENTS AND AUDITING")
            {
                Items =
                {
                    new NavItem("Receipt Management", "Receipts", "📄", true),
                    new NavItem("Audit Verification", "AuditVerification", "✅", true),
                    new NavItem("Document Archive", "DocumentArchive", "🗂️", true),
                    new NavItem("Receipt Register", "ReceiptRegister", "📋", true),
                    new NavItem("Audit History", "AuditHistory", "📜", true),
                    new NavItem("Audit Trail", "AuditTrail", "🔍", true),
                }
            },
            new SidebarGroup("REPORTS")
            {
                Items =
                {
                    new NavItem("Daily Financial Report", "DailyFinancialReport", "📅", true),
                    new NavItem("Monthly Financial Report", "MonthlyFinancialReport", "📆", true),
                    new NavItem("Audit Report Generator", "AuditReportGenerator", "📑", true),
                    new NavItem("Collection Report", "CollectionReport", "📊", true),
                    new NavItem("Expense Report", "ExpenseReport", "💰", true),
                    new NavItem("Financial Statement", "FinancialStatement", "📊", true),
                    new NavItem("Export and Print", "ExportPrint", "📤", true),
                    new NavItem("Excel Analytics", "ExcelAnalytics", "📈", true),
                }
            },
            new SidebarGroup("RECORDS AND SYSTEM")
            {
                Items =
                {
                    new NavItem("All Records", "AllRecords", "📋", true),
                    new NavItem("Recycle Bin", "RecycleBin", "🗑️", true),
                    new NavItem("Notifications", "Notifications", "🔔", true),
                    new NavItem("Data Validation", "DataValidation", "🔍", true),
                    new NavItem("Backup and Restore", "BackupRestore", "💾", true),
                    new NavItem("Auditor Profile and Settings", "AuditorProfile", "⚙️", true),
                }
            },
            new SidebarGroup("UTILITIES")
            {
                Items =
                {
                    new NavItem("Excel Analytics", "ExcelAnalytics", "📈", true),
                    new NavItem("Unlock", "Unlock", "🔓", true),
                }
            }
        };

        foreach (var group in groups)
        {
            SidebarGroups.Add(group);
            foreach (var item in group.Items)
            {
                _sidebarItems.Add(item);
            }
        }
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
        var pageName = page.Replace(" ", string.Empty);

        // Handle special cases
        CurrentView = page switch
        {
            "Dashboard" => new DashboardView(),
            "TransactionEntry" => new Views.FinancialRecords.TransactionEntryView(),
            "TransactionLedger" => new Views.FinancialRecords.TransactionLedgerView(),
            "Collections" => new Views.FinancialRecords.CollectionsView(),
            "Expenses" => new Views.FinancialRecords.ExpensesView(),
            "CashBalance" => new Views.FinancialRecords.CashBalanceView(),
            "FundManagement" => new Views.FinancialRecords.FundManagementView(),
            "BudgetMonitoring" => new Views.FinancialRecords.BudgetMonitoringView(),
            "Reimbursements" => new Views.FinancialRecords.ReimbursementsView(),
            "Liquidations" => new Views.FinancialRecords.LiquidationsView(),
            "BankReconciliation" => new Views.FinancialRecords.BankReconciliationView(),
            "Dues" => new Views.FinancialRecords.DuesView(),
            "FinancialAdjustments" => new Views.FinancialRecords.FinancialAdjustmentsView(),
            "Receipts" => new ReceiptsView(),
            "AuditVerification" => new Views.AuditVerification.AuditVerificationView(),
            "DocumentArchive" => new Views.Documents.DocumentArchiveView(),
            "ReceiptRegister" => new Views.Documents.ReceiptRegisterView(),
            "AuditHistory" => new Views.Documents.AuditHistoryView(),
            "AuditTrail" => new Views.AuditTrail.AuditTrailView(),
            "DailyFinancialReport" => new Views.Reports.DailyFinancialReportView(),
            "MonthlyFinancialReport" => new Views.Reports.MonthlyFinancialReportView(),
            "AuditReportGenerator" => new Views.Reports.AuditReportGeneratorView(),
            "CollectionReport" => new Views.Reports.CollectionReportView(),
            "ExpenseReport" => new Views.Reports.ExpenseReportView(),
            "FinancialStatement" => new Views.Reports.FinancialStatementView(),
            "ExportPrint" => new Views.Reports.ExportPrintView(),
            "AllRecords" => new Views.Records.AllRecordsView(),
            "RecycleBin" => new Views.Records.RecycleBinView(),
            "Notifications" => new Views.Records.NotificationsView(),
            "DataValidation" => new Views.Records.DataValidationView(),
            "BackupRestore" => new Views.Records.BackupRestoreView(),
            "AuditorProfile" => new Views.Records.AuditorProfileView(),
            "ExcelAnalytics" => new Views.ExcelAnalytics.ExcelAnalyticsView(),
            "Unlock" => new Views.Unlock.UnlockView(),
            _ => new DashboardView()
        };
    }

    public void Initialize()
    {
        if (CurrentUser == null) return;
        // Load recent transactions for dashboard display
    }
}
