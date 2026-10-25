namespace AuditorManagementSystem.Application.Tests;

using AuditorManagementSystem.Infrastructure.Security;
using Xunit;

/// <summary>
/// Tests for the PasswordHasher security component.
/// Verifies correct password verification, incorrect password rejection,
/// random salt generation, malformed hash handling, and backward compatibility.
/// </summary>
public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_CorrectPassword_ProducesValidStoredHash()
    {
        // Arrange
        const string password = "SecureP@ssw0rd!";

        // Act
        string storedHash = _hasher.Hash(password);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(storedHash));
        var parts = storedHash.Split('.');
        Assert.Equal(3, parts.Length);
        Assert.Equal("600000", parts[0]); // iteration count
        Assert.NotEmpty(parts[1]); // salt (base64)
        Assert.NotEmpty(parts[2]); // hash (base64)

        // Verify the hash validates against itself
        Assert.True(_hasher.Verify(password, storedHash));
    }

    [Fact]
    public void Hash_SamePasswordTwice_ProducesDifferentSalts()
    {
        // Arrange
        const string password = "SamePassword123!";

        // Act
        string hash1 = _hasher.Hash(password);
        string hash2 = _hasher.Hash(password);

        // Assert
        Assert.NotEqual(hash1, hash2); // different salts produce different hashes

        // Both should still verify correctly
        Assert.True(_hasher.Verify(password, hash1));
        Assert.True(_hasher.Verify(password, hash2));
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        const string password = "CorrectP@ssw0rd!";
        string storedHash = _hasher.Hash(password);

        // Act
        bool result = _hasher.Verify(password, storedHash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Verify_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        const string password = "CorrectP@ssw0rd!";
        const string wrongPassword = "WrongP@ssw0rd!";
        string storedHash = _hasher.Hash(password);

        // Act
        bool result = _hasher.Verify(wrongPassword, storedHash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_EmptyPassword_ReturnsFalse()
    {
        // Arrange
        string storedHash = _hasher.Hash("SomePassword123!");

        // Act
        bool result = _hasher.Verify(string.Empty, storedHash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_NullPassword_ReturnsFalse()
    {
        // Arrange
        string storedHash = _hasher.Hash("SomePassword123!");

        // Act
        bool result = _hasher.Verify(null, storedHash);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Verify_NullOrWhitespaceStoredHash_ReturnsFalse(string? storedHash)
    {
        // Act
        bool result = _hasher.Verify("AnyPassword123!", storedHash!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_MalformedStoredHash_ReturnsFalse()
    {
        // Arrange - various malformed hash formats
        string[] malformedHashes =
        {
            "not.a.valid.hash.format.with.too.many.parts",
            "abc.def", // only 2 parts
            "notanumber.salt.hash", // iterations not a number
            "100.invalidbase64!@#.hash",
            "100.validbase64==.invalidbase64!@#",
            "", // empty string
            ".", // just separators
            "..", // empty parts
        };

        foreach (string malformed in malformedHashes)
        {
            // Act
            bool result = _hasher.Verify("AnyPassword123!", malformed);

            // Assert
            Assert.False(result, $"Expected false for malformed hash: '{malformed}'");
        }
    }

    [Fact]
    public void Hash_DifferentPasswords_ProducesDifferentHashes()
    {
        // Arrange
        const string password1 = "PasswordOne123!";
        const string password2 = "PasswordTwo123!";

        // Act
        string hash1 = _hasher.Hash(password1);
        string hash2 = _hasher.Hash(password2);

        // Assert
        Assert.NotEqual(hash1, hash2);
        Assert.True(_hasher.Verify(password1, hash1));
        Assert.True(_hasher.Verify(password2, hash2));
        Assert.False(_hasher.Verify(password1, hash2));
        Assert.False(_hasher.Verify(password2, hash1));
    }

    [Fact]
    public void Verify_LegacyHashFormat_WithSameParameters_StillWorks()
    {
        // Arrange - simulate a legacy hash created with the same format
        // iterations.salt.hash (base64)
        const string password = "LegacyCompatible123!";
        const int iterations = 600000;

        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        byte[] salt = new byte[16];
        rng.GetBytes(salt);

        byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
        byte[] hash = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
            passwordBytes,
            salt,
            iterations,
            System.Security.Cryptography.HashAlgorithmName.SHA256,
            32);

        string legacyHash = $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";

        // Act
        bool result = _hasher.Verify(password, legacyHash);

        // Assert
        Assert.True(result, "Legacy hash with same parameters should verify correctly");
    }

    [Fact]
    public void Hash_IterationCount_InStorageFormat()
    {
        // Arrange
        const string password = "IterationTest123!";

        // Act
        string storedHash = _hasher.Hash(password);

        // Assert - verify iteration count is embedded in the hash
        string iterationPart = storedHash.Split('.')[0];
        Assert.Equal("600000", iterationPart);
    }

    [Fact]
    public void Verify_WrongIterationCount_InHash_StillWorks()
    {
        // Arrange - create a hash with a different iteration count
        const string password = "IterationFlex123!";
        const int iterations = 100000;

        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        byte[] salt = new byte[16];
        rng.GetBytes(salt);

        byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
        byte[] hash = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
            passwordBytes,
            salt,
            iterations,
            System.Security.Cryptography.HashAlgorithmName.SHA256,
            32);

        string hashWithDifferentIterations = $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";

        // Act - the hasher reads the iteration count from the stored hash
        bool result = _hasher.Verify(password, hashWithDifferentIterations);

        // Assert
        Assert.True(result, "Hash with different iteration count should still verify when read from storage");
    }

    [Fact]
    public void Hash_DoesNotContainPlaintextPassword()
    {
        // Arrange
        const string password = "SuperSecretP@ssw0rd!";

        // Act
        string storedHash = _hasher.Hash(password);

        // Assert
        Assert.DoesNotContain(password, storedHash);
        Assert.DoesNotContain(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password)), storedHash);
    }
}
