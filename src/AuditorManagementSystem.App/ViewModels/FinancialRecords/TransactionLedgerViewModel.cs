using AuditorManagementSystem.Core.DTOs;
using AuditorManagementSystem.Core.Enums;
using AuditorManagementSystem.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace AuditorManagementSystem.App.ViewModels.FinancialRecords;

public partial class TransactionLedgerViewModel : BaseViewModel
{
    private readonly ITransactionService _transactionService;
    private readonly IReceiptService _receiptService;
    private readonly IAuditLogService _auditLogService;

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private TransactionType? _selectedType;

    [ObservableProperty]
    private string _selectedCategory = "";

    [ObservableProperty]
    private TransactionStatus? _selectedStatus;

    [ObservableProperty]
    private DateTime? _startDate;

    [ObservableProperty]
    private DateTime? _endDate;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 25;

    [ObservableProperty]
    private int _totalItems;

    [ObservableProperty]
    private int _totalPages;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasMore;

    [ObservableProperty]
    private ObservableCollection<TransactionDto> _transactions = new();

    [ObservableProperty]
    private string _message = "";
    [ObservableProperty]
    private bool _messageVisible;

    public ObservableCollection<TransactionType?> TypeFilters { get; } = new()
    {
        null, TransactionType.Income, TransactionType.Expense
    };

    public ObservableCollection<TransactionStatus?> StatusFilters { get; } = new()
    {
        null, TransactionStatus.Pending, TransactionStatus.Approved, TransactionStatus.Rejected, TransactionStatus.Voided
    };

    // Separate ICommand properties to avoid RelayCommand attribute conflicts on methods
    public ICommand PageChangedCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand ResetFiltersCommand { get; }

    public TransactionLedgerViewModel(
        ITransactionService transactionService,
        IReceiptService receiptService,
        IAuditLogService auditLogService,
        ISessionService sessionService) : base(sessionService)
    {
        _transactionService = transactionService;
        _receiptService = receiptService;
        _auditLogService = auditLogService;
        PageChangedCommand = new RelayCommand<int>(async p => await LoadPageAsync(p));
        PreviousPageCommand = new RelayCommand(async () => await LoadPageAsync(CurrentPage - 1));
        NextPageCommand = new RelayCommand(async () => await LoadPageAsync(CurrentPage + 1));
        ResetFiltersCommand = new RelayCommand(ResetFilters);
        LoadPageAsync(1).ConfigureAwait(false);
    }

    private async Task LoadPageAsync(int page)
    {
        if (IsLoading) return;
        CurrentPage = page;
        IsLoading = true;
        try
        {
            var start = (page - 1) * PageSize;
            var transactions = await _transactionService.GetTransactionsAsync(
                startDate: StartDate,
                endDate: EndDate,
                category: SelectedCategory,
                type: SelectedType,
                status: SelectedStatus,
                search: string.IsNullOrWhiteSpace(SearchText) ? null : SearchText);

            TotalItems = transactions.Count();
            TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);

            var paged = transactions.Skip(start).Take(PageSize).ToList();
            Transactions = new ObservableCollection<TransactionDto>(paged);
            HasMore = page * PageSize < TotalItems;
        }
        catch (Exception ex)
        {
            Message = "Failed to load transactions: " + ex.Message;
            MessageVisible = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ResetFilters()
    {
        SearchText = "";
        SelectedType = null;
        SelectedCategory = "";
        SelectedStatus = null;
        StartDate = null;
        EndDate = null;
        CurrentPage = 1;
        LoadPageAsync(1).ConfigureAwait(false);
    }
}
