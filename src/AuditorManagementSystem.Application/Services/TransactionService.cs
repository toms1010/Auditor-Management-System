namespace AuditorManagementSystem.Application.Services;

using AuditorManagementSystem.Application.Interfaces;
using AuditorManagementSystem.Core.DTOs;
using AuditorManagementSystem.Core.Entities;
using AuditorManagementSystem.Core.Enums;
using AuditorManagementSystem.Core.Exceptions;
using AuditorManagementSystem.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IFinancialCalculationService _calculationService;
    private readonly IAuditLogService _auditLogService;

    public TransactionService(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository,
        IFinancialCalculationService calculationService,
        IAuditLogService auditLogService)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _calculationService = calculationService;
        _auditLogService = auditLogService;
    }

    public async Task<int> CreateTransactionAsync(CreateTransactionDto dto, int currentUserId)
    {
        if (dto.Amount <= 0)
            throw new ValidationException("Amount must be greater than zero.");
        if (string.IsNullOrWhiteSpace(dto.ReferenceNumber))
            throw new ValidationException("Reference number is required.");

        var existing = await _transactionRepository.GetByReferenceNumberAsync(dto.ReferenceNumber);
        if (existing != null)
            throw new ValidationException($"Reference number '{dto.ReferenceNumber}' already exists.");

        var transaction = new Transaction
        {
            ReferenceNumber = dto.ReferenceNumber,
            TransactionDate = dto.TransactionDate,
            Description = dto.Description,
            Category = dto.Category,
            Type = dto.Type,
            Amount = dto.Amount,
            Payee = dto.Payee,
            PaymentMethod = dto.PaymentMethod,
            Status = TransactionStatus.Pending,
            ApprovalStatus = ApprovalStatus.Pending,
            CreatedByUserId = currentUserId,
            CreatedBy = currentUserId.ToString(),
            UpdatedBy = currentUserId.ToString()
        };

        var id = await _transactionRepository.AddAsync(transaction);

        await _auditLogService.LogAsync(
            currentUserId, null, "CreateTransaction", "Transaction",
            entityId: id, reference: dto.ReferenceNumber,
            changeSummary: $"Created {dto.Type} of {dto.Amount:C} in category {dto.Category}");

        return id;
    }

    public async Task<TransactionDto?> GetTransactionByIdAsync(int id)
    {
        var t = await _transactionRepository.GetByIdAsync(id);
        return t == null ? null : Map(t);
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? category = null,
        TransactionType? type = null,
        TransactionStatus? status = null,
        string? search = null)
    {
        var all = await _transactionRepository.GetAllAsync();

        var query = all.AsEnumerable();

        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        if (type.HasValue)
            query = query.Where(t => t.Type == type.Value);
        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLowerInvariant();
            query = query.Where(t =>
                t.ReferenceNumber.ToLowerInvariant().Contains(s) ||
                t.Description.ToLowerInvariant().Contains(s) ||
                (t.Payee ?? string.Empty).ToLowerInvariant().Contains(s));
        }

        return query.Select(Map).ToList();
    }

    public async Task<bool> UpdateTransactionAsync(UpdateTransactionDto dto, int currentUserId)
    {
        var existing = await _transactionRepository.GetByIdAsync(dto.Id);
        if (existing == null)
            throw new NotFoundException($"Transaction {dto.Id} not found.");
        if (existing.IsVoided)
            throw new BusinessRuleException("Cannot update a voided transaction.");

        var changeSummary = new List<string>();
        if (existing.Amount != dto.Amount) changeSummary.Add($"Amount: {existing.Amount:C} -> {dto.Amount:C}");
        if (existing.Description != dto.Description) changeSummary.Add($"Description changed");
        if (existing.Category != dto.Category) changeSummary.Add($"Category: {existing.Category} -> {dto.Category}");

        existing.ReferenceNumber = dto.ReferenceNumber;
        existing.TransactionDate = dto.TransactionDate;
        existing.Description = dto.Description;
        existing.Category = dto.Category;
        existing.Type = dto.Type;
        existing.Amount = dto.Amount;
        existing.Payee = dto.Payee;
        existing.PaymentMethod = dto.PaymentMethod;
        existing.UpdatedBy = currentUserId.ToString();

        var result = await _transactionRepository.UpdateAsync(existing);

        if (changeSummary.Any())
        {
            await _auditLogService.LogAsync(
                currentUserId, null, "UpdateTransaction", "Transaction",
                entityId: dto.Id, reference: dto.ReferenceNumber,
                changeSummary: string.Join("; ", changeSummary));
        }

        return result;
    }

    public async Task<bool> VoidTransactionAsync(int id, string reason, int currentUserId)
    {
        var existing = await _transactionRepository.GetByIdAsync(id);
        if (existing == null)
            throw new NotFoundException($"Transaction {id} not found.");
        if (existing.IsVoided)
            throw new BusinessRuleException("Transaction is already voided.");

        var result = await _transactionRepository.VoidAsync(id, reason, currentUserId.ToString());

        await _auditLogService.LogAsync(
            currentUserId, null, "VoidTransaction", "Transaction",
            entityId: id, reference: existing.ReferenceNumber,
            reason: reason, changeSummary: $"Voided transaction {existing.Amount:C}");

        return result;
    }

    public async Task<bool> ApproveTransactionAsync(int id, int approverUserId, string? comments)
    {
        var existing = await _transactionRepository.GetByIdAsync(id);
        if (existing == null)
            throw new NotFoundException($"Transaction {id} not found.");

        var repo = (ITransactionRepository)_transactionRepository;
        var all = await _transactionRepository.GetAllAsync();
        var target = all.FirstOrDefault(t => t.Id == id);
        if (target == null) return false;

        target.Status = TransactionStatus.Approved;
        target.ApprovalStatus = ApprovalStatus.Approved;
        target.UpdatedBy = approverUserId.ToString();

        await _transactionRepository.UpdateAsync(target);

        await _auditLogService.LogAsync(
            approverUserId, null, "ApproveTransaction", "Transaction",
            entityId: id, reference: existing.ReferenceNumber,
            reason: comments, changeSummary: "Transaction approved");

        return true;
    }

    public async Task<bool> RejectTransactionAsync(int id, int approverUserId, string? comments)
    {
        var existing = await _transactionRepository.GetByIdAsync(id);
        if (existing == null)
            throw new NotFoundException($"Transaction {id} not found.");

        var target = (await _transactionRepository.GetAllAsync()).FirstOrDefault(t => t.Id == id);
        if (target == null) return false;

        target.Status = TransactionStatus.Rejected;
        target.ApprovalStatus = ApprovalStatus.Rejected;
        target.UpdatedBy = approverUserId.ToString();

        await _transactionRepository.UpdateAsync(target);

        await _auditLogService.LogAsync(
            approverUserId, null, "RejectTransaction", "Transaction",
            entityId: id, reference: existing.ReferenceNumber,
            reason: comments, changeSummary: "Transaction rejected");

        return true;
    }

    private static TransactionDto Map(Transaction t) => new()
    {
        Id = t.Id,
        ReferenceNumber = t.ReferenceNumber,
        TransactionDate = t.TransactionDate,
        Description = t.Description,
        Category = t.Category,
        Type = t.Type,
        Amount = t.Amount,
        Payee = t.Payee,
        PaymentMethod = t.PaymentMethod,
        Status = t.Status,
        ApprovalStatus = t.ApprovalStatus,
        IsVoided = t.IsVoided,
        VoidReason = t.VoidReason,
        CreatedByUserId = t.CreatedByUserId,
        ReceiptFileName = t.ReceiptFileName,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt
    };
}
