namespace AuditorManagementSystem.Application.Services;

using AuditorManagementSystem.Application.Interfaces;
using AuditorManagementSystem.Core.DTOs;
using AuditorManagementSystem.Core.Entities;
using AuditorManagementSystem.Core.Enums;
using AuditorManagementSystem.Core.Exceptions;
using AuditorManagementSystem.Core.Interfaces;
using System.IO;

public class ReceiptService : IReceiptService
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IReceiptFileStorage _fileStorage;
    private readonly IAuditLogService _auditLogService;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png"
    };

    private static readonly long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public ReceiptService(
        IReceiptRepository receiptRepository,
        ITransactionRepository transactionRepository,
        IReceiptFileStorage fileStorage,
        IAuditLogService auditLogService)
    {
        _receiptRepository = receiptRepository;
        _transactionRepository = transactionRepository;
        _fileStorage = fileStorage;
        _auditLogService = auditLogService;
    }

    public async Task<int> UploadReceiptAsync(int transactionId, string originalFileName, byte[] fileData, string contentType, int uploadedByUserId)
    {
        var ext = Path.GetExtension(originalFileName);
        if (!AllowedExtensions.Contains(ext))
            throw new ValidationException($"File type '{ext}' is not allowed. Allowed: .pdf, .jpg, .jpeg, .png");
        if (fileData.Length > MaxFileSizeBytes)
            throw new ValidationException($"File exceeds the {MaxFileSizeBytes / 1024 / 1024} MB size limit.");

        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null)
            throw new NotFoundException($"Transaction {transactionId} not found.");

        var storedName = _fileStorage.SaveFile(fileData, originalFileName);

        var receipt = new Receipt
        {
            FileName = storedName,
            OriginalFileName = originalFileName,
            FilePath = _fileStorage.GetFullPath(storedName),
            FileSizeBytes = fileData.Length,
            ContentType = contentType,
            TransactionId = transactionId,
            CreatedBy = uploadedByUserId.ToString()
        };

        var id = await _receiptRepository.AddAsync(receipt);

        await _auditLogService.LogAsync(
            uploadedByUserId, null, "UploadReceipt", "Receipt",
            entityId: id, reference: transaction.ReferenceNumber,
            changeSummary: $"Uploaded receipt {originalFileName} ({fileData.Length} bytes)");

        return id;
    }

    public Task<byte[]?> DownloadReceiptAsync(int receiptId)
    {
        // Implement using repository + storage
        throw new NotImplementedException();
    }

    public Task<bool> DeleteReceiptAsync(int receiptId, int currentUserId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> VerifyReceiptAsync(int receiptId, string verifiedBy, string? notes)
    {
        throw new NotImplementedException();
    }
}
