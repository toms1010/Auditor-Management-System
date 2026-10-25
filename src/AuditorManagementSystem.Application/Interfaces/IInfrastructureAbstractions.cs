namespace AuditorManagementSystem.Application.Interfaces;

/// <summary>
/// Password hashing abstraction so the Application layer does not depend on Infrastructure.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string storedHash);
}

/// <summary>
/// Receipt file storage abstraction so the Application layer does not depend on Infrastructure.
/// </summary>
public interface IReceiptFileStorage
{
    string SaveFile(byte[] fileData, string originalFileName);
    byte[]? ReadFile(string storedFileName);
    bool DeleteFile(string storedFileName);
    string GetFullPath(string storedFileName);
}

/// <summary>
/// CSV exporter abstraction.
/// </summary>
public interface ICsvExporter
{
    byte[] ExportTransactions(IEnumerable<TransactionDto> transactions);
}
