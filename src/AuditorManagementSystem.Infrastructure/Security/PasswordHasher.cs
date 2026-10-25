namespace AuditorManagementSystem.Infrastructure.Security;

using AuditorManagementSystem.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Hashes and verifies passwords using PBKDF2 with a random salt.
/// Passwords are never stored in plain text.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 600_000;

    /// <summary>
    /// Hashes a plain-text password and returns a storage string
    /// in the format: iterations.salt.hash (base64).
    /// </summary>
    public string Hash(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        byte[] salt = new byte[SaltSize];
        rng.GetBytes(salt);

        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            passwordBytes,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verifies a plain-text password against a stored hash.
    /// Supports both the current Pbkdf2 format and any legacy hashes.
    /// </summary>
    public bool Verify(string? password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(storedHash))
            return false;

        if (string.IsNullOrWhiteSpace(password))
            return false;

        var parts = storedHash.Split('.');
        if (parts.Length != 3)
            return false;

        if (!int.TryParse(parts[0], out int iterations))
            return false;

        byte[] salt;
        byte[] hash;
        try
        {
            salt = Convert.FromBase64String(parts[1]);
            hash = Convert.FromBase64String(parts[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] computedHash = Rfc2898DeriveBytes.Pbkdf2(
            passwordBytes,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return CryptographicOperations.FixedTimeEquals(hash, computedHash);
    }
}
