namespace AuditorManagementSystem.Infrastructure.Storage;

using AuditorManagementSystem.Application.Interfaces;
using System.IO;

/// <summary>
/// Safely stores uploaded receipt files in the application data directory.
/// Files are renamed with a GUID to prevent collisions and path traversal.
/// </summary>
public class ReceiptFileStorage : IReceiptFileStorage
{
    private readonly string _storageRoot;

    public ReceiptFileStorage(string storageRoot)
    {
        _storageRoot = storageRoot;
        Directory.CreateDirectory(_storageRoot);
    }

    /// <summary>
    /// Saves file bytes and returns the stored relative file name.
    /// </summary>
    public string SaveFile(byte[] fileData, string originalFileName)
    {
        var safeName = Path.GetFileName(originalFileName);
        var uniqueName = $"{Guid.NewGuid():N}_{safeName}";
        var fullPath = Path.Combine(_storageRoot, uniqueName);
        File.WriteAllBytes(fullPath, fileData);
        return uniqueName;
    }

    /// <summary>
    /// Reads a stored receipt file's bytes. Returns null if not found.
    /// </summary>
    public byte[]? ReadFile(string storedFileName)
    {
        var fullPath = Path.Combine(_storageRoot, storedFileName);
        if (!File.Exists(fullPath))
            return null;
        return File.ReadAllBytes(fullPath);
    }

    /// <summary>
    /// Deletes a stored receipt file.
    /// </summary>
    public bool DeleteFile(string storedFileName)
    {
        try
        {
            var fullPath = Path.Combine(_storageRoot, storedFileName);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public string GetFullPath(string storedFileName) => Path.Combine(_storageRoot, storedFileName);
}
