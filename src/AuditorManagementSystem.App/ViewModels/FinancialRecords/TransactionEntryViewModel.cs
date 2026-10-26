using AuditorManagementSystem.Core.DTOs;
using AuditorManagementSystem.Core.Enums;
using AuditorManagementSystem.Core.Exceptions;
using AuditorManagementSystem.Core.Interfaces;
using AuditorManagementSystem.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AuditorManagementSystem.App.ViewModels.FinancialRecords;

public partial class BaseViewModel : ObservableObject
{
    protected readonly ISessionService _sessionService;

    protected BaseViewModel(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public UserDto? CurrentUser => _sessionService.CurrentUser;

    [RelayCommand]
    public virtual void Cancel()
    {
        // Override in derived viewmodels
    }

    [RelayCommand]
    public virtual void Delete()
    {
        // Override in derived viewmodels
    }
}

public partial class TransactionEntryViewModel : BaseViewModel
{
    private readonly ITransactionService _transactionService;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IReceiptService _receiptService;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private int _selectedId;

    [ObservableProperty]
    private string _referenceNumber = "";

    [ObservableProperty]
    private DateTime _transactionDate = DateTime.Now;

    [ObservableProperty]
    private string _description = "";

    [ObservableProperty]
    private TransactionType _transactionType = TransactionType.Income;

    [ObservableProperty]
    private string _category = "";

    [ObservableProperty]
    private decimal _amount;

    [ObservableProperty]
    private string _payee = "";

    [ObservableProperty]
    private string _paymentMethod = "";

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string _message = "";
    [ObservableProperty]
    private bool _messageVisible;

    public ObservableCollection<string> Categories { get; } = new();
    public ObservableCollection<string> PaymentMethods { get; } = new() { "Cash", "Card", "Bank Transfer", "Check", "Mobile Payment", "Other" };

    public TransactionEntryViewModel(
        ITransactionService transactionService,
        ICategoryRepository categoryRepository,
        IReceiptService receiptService,
        ISessionService sessionService) : base(sessionService)
    {
        _transactionService = transactionService;
        _categoryRepository = categoryRepository;
        _receiptService = receiptService;
        LoadCategories();
    }

    private async void LoadCategories()
    {
        try
        {
            var categories = await _categoryRepository.GetAllAsync();
            foreach (var c in categories)
                Categories.Add(c.Name);
        }
        catch { }
    }

    public async Task LoadTransactionAsync(int id)
    {
        IsEditMode = true;
        var tx = await _transactionService.GetTransactionByIdAsync(id);
        if (tx == null)
        {
            Message = "Transaction not found.";
            MessageVisible = true;
            return;
        }
        SelectedId = tx.Id;
        ReferenceNumber = tx.ReferenceNumber;
        TransactionDate = tx.TransactionDate;
        Description = tx.Description;
        TransactionType = tx.Type;
        Category = tx.Category;
        Amount = tx.Amount;
        Payee = tx.Payee ?? "";
        PaymentMethod = tx.PaymentMethod ?? "";
        Message = "Transaction loaded successfully.";
        MessageVisible = true;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(ReferenceNumber))
        {
            Message = "Reference number is required.";
            MessageVisible = true;
            return;
        }
        if (string.IsNullOrWhiteSpace(Category))
        {
            Message = "Category is required.";
            MessageVisible = true;
            return;
        }
        if (Amount <= 0)
        {
            Message = "Amount must be greater than zero.";
            MessageVisible = true;
            return;
        }
        IsSaving = true;
        try
        {
            var dto = new CreateTransactionDto
            {
                ReferenceNumber = ReferenceNumber.Trim(),
                TransactionDate = TransactionDate,
                Description = Description.Trim(),
                Category = Category,
                Type = TransactionType,
                Amount = Amount,
                Payee = string.IsNullOrWhiteSpace(Payee) ? null : Payee.Trim(),
                PaymentMethod = string.IsNullOrWhiteSpace(PaymentMethod) ? null : PaymentMethod.Trim(),
                CreatedByUserId = CurrentUser?.Id ?? 1
            };
            var id = await _transactionService.CreateTransactionAsync(dto, CurrentUser?.Id ?? 1);
            Message = "Transaction saved successfully.";
            MessageVisible = true;
        }
        catch (ValidationException ex)
        {
            Message = ex.Message;
            MessageVisible = true;
        }
        catch (Exception ex)
        {
            Message = "Failed to save transaction: " + ex.Message;
            MessageVisible = true;
        }
        finally
        {
            IsSaving = false;
        }
    }

    public override void Cancel()
    {
        MessageVisible = false;
        Message = "";
        ReferenceNumber = "";
        Description = "";
        Category = "";
        Amount = 0;
        Payee = "";
        PaymentMethod = "";
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedId == 0) return;
        try
        {
            await _transactionService.VoidTransactionAsync(SelectedId, "Deleted by user via UI", CurrentUser?.Id ?? 1);
            Message = "Transaction deleted.";
            MessageVisible = true;
        }
        catch (Exception ex)
        {
            Message = "Failed to delete: " + ex.Message;
            MessageVisible = true;
        }
    }
}
