namespace AuditorManagementSystem.Infrastructure.Data;

using AuditorManagementSystem.Core.Entities;
using AuditorManagementSystem.Core.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

/// <summary>
/// Initializes the SQLite database schema on first run and seeds default
/// data (roles, categories, administrator account).
/// </summary>
public class DatabaseInitializer
{
    private readonly DatabaseConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly string _defaultAdminPassword;

    public DatabaseInitializer(
        DatabaseConnectionFactory connectionFactory,
        ILogger<DatabaseInitializer> logger,
        string defaultAdminPassword = "Admin123!")
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        _defaultAdminPassword = defaultAdminPassword;
    }

    public async Task InitializeAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await CreateTablesAsync(connection);
        await SeedDefaultDataAsync(connection);

        _logger.LogInformation("Database initialized at {DbPath}", _connectionFactory.GetDatabasePath());
    }

    private static async Task CreateTablesAsync(SqliteConnection connection)
    {
        var commands = new[]
        {
            // Users
            @"CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE,
                Email TEXT NOT NULL UNIQUE,
                FullName TEXT NOT NULL,
                Role TEXT NOT NULL DEFAULT 'Recorder',
                IsActive INTEGER NOT NULL DEFAULT 1,
                LastLoginAt TEXT,
                PasswordHash TEXT NOT NULL,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt TEXT,
                CreatedBy TEXT,
                UpdatedBy TEXT
            )",

            // Roles
            @"CREATE TABLE IF NOT EXISTS Roles (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL UNIQUE,
                Description TEXT,
                IsSystem INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt TEXT,
                CreatedBy TEXT,
                UpdatedBy TEXT
            )",

            // Categories
            @"CREATE TABLE IF NOT EXISTS Categories (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL UNIQUE,
                Description TEXT,
                IsActive INTEGER NOT NULL DEFAULT 1,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt TEXT,
                CreatedBy TEXT,
                UpdatedBy TEXT
            )",

            // Transactions
            @"CREATE TABLE IF NOT EXISTS Transactions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ReferenceNumber TEXT NOT NULL UNIQUE,
                TransactionDate TEXT NOT NULL,
                Description TEXT NOT NULL,
                Category TEXT NOT NULL,
                Type TEXT NOT NULL,
                Amount REAL NOT NULL,
                Payee TEXT,
                PaymentMethod TEXT,
                Status TEXT NOT NULL DEFAULT 'Pending',
                ApprovalStatus TEXT NOT NULL DEFAULT 'Pending',
                IsVoided INTEGER NOT NULL DEFAULT 0,
                VoidReason TEXT,
                VoidedAt TEXT,
                VoidedBy TEXT,
                CreatedByUserId INTEGER NOT NULL,
                ReceiptFileName TEXT,
                ReceiptFilePath TEXT,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt TEXT,
                CreatedBy TEXT,
                UpdatedBy TEXT,
                FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
            )",

            // Receipts
            @"CREATE TABLE IF NOT EXISTS Receipts (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FileName TEXT NOT NULL,
                OriginalFileName TEXT NOT NULL,
                FilePath TEXT NOT NULL,
                FileSizeBytes INTEGER NOT NULL,
                ContentType TEXT NOT NULL,
                TransactionId INTEGER NOT NULL,
                IsVerified INTEGER NOT NULL DEFAULT 0,
                VerifiedBy TEXT,
                VerifiedAt TEXT,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt TEXT,
                CreatedBy TEXT,
                UpdatedBy TEXT,
                FOREIGN KEY (TransactionId) REFERENCES Transactions(Id)
            )",

            // ApprovalHistory
            @"CREATE TABLE IF NOT EXISTS ApprovalHistory (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                TransactionId INTEGER NOT NULL,
                ApprovedByUserId INTEGER NOT NULL,
                Status TEXT NOT NULL,
                Comments TEXT,
                ApprovedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (TransactionId) REFERENCES Transactions(Id),
                FOREIGN KEY (ApprovedByUserId) REFERENCES Users(Id)
            )",

            // AuditLogs
            @"CREATE TABLE IF NOT EXISTS AuditLogs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER,
                UserName TEXT,
                Action TEXT NOT NULL,
                EntityType TEXT NOT NULL,
                EntityId INTEGER,
                Reference TEXT,
                ChangeSummary TEXT,
                Reason TEXT,
                IpAddress TEXT,
                OccurredAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt TEXT,
                CreatedBy TEXT,
                UpdatedBy TEXT,
                FOREIGN KEY (UserId) REFERENCES Users(Id)
            )",

            "CREATE INDEX IF NOT EXISTS IX_Transactions_ReferenceNumber ON Transactions(ReferenceNumber);",
            "CREATE INDEX IF NOT EXISTS IX_Transactions_Date ON Transactions(TransactionDate);",
            "CREATE INDEX IF NOT EXISTS IX_Transactions_Status ON Transactions(Status);",
            "CREATE INDEX IF NOT EXISTS IX_Receipts_TransactionId ON Receipts(TransactionId);",
            "CREATE INDEX IF NOT EXISTS IX_AuditLogs_OccurredAt ON AuditLogs(OccurredAt);",
            "CREATE INDEX IF NOT EXISTS IX_Users_Username ON Users(Username);",
        };

        using var cmd = connection.CreateCommand();
        foreach (var sql in commands)
        {
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync();
        }
    }

    private async Task SeedDefaultDataAsync(SqliteConnection connection)
    {
        // Seed default categories if empty
        var countCmd = connection.CreateCommand();
        countCmd.CommandText = "SELECT COUNT(*) FROM Categories";
        var categoryCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

        if (categoryCount == 0)
        {
            using var tx = connection.BeginTransaction();
            try
            {
                var categories = new[] { "Office Supplies", "Utilities", "Travel", "Marketing", "Salaries", "Equipment", "Other" };
                foreach (var cat in categories)
                {
                    var cmd = connection.CreateCommand();
                    cmd.Transaction = tx;
                    cmd.CommandText = @"INSERT INTO Categories (Name, Description, IsActive, CreatedAt)
                                        VALUES ($name, $desc, 1, $now)";
                    cmd.Parameters.AddWithValue("$name", cat);
                    cmd.Parameters.AddWithValue("$desc", $"Default category: {cat}");
                    cmd.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("o"));
                    await cmd.ExecuteNonQueryAsync();
                }
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        // Seed default administrator if no users exist
        countCmd.CommandText = "SELECT COUNT(*) FROM Users";
        var userCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

        if (userCount == 0)
        {
            var passwordHasher = new Security.PasswordHasher();
            string passwordHash = passwordHasher.Hash(_defaultAdminPassword);
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"INSERT INTO Users (Username, Email, FullName, Role, IsActive, PasswordHash, CreatedAt)
                                VALUES ($user, $email, $name, $role, 1, $hash, $now)";
            cmd.Parameters.AddWithValue("$user", "admin");
            cmd.Parameters.AddWithValue("$email", "admin@audit.local");
            cmd.Parameters.AddWithValue("$name", "System Administrator");
            cmd.Parameters.AddWithValue("$role", UserRole.Administrator.ToString());
            cmd.Parameters.AddWithValue("$hash", passwordHash);
            cmd.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("o"));
            await cmd.ExecuteNonQueryAsync();

            _logger.LogInformation("Default administrator account created. Username: admin");
        }
        else
        {
            // Ensure existing administrator account has a valid password hash
            // (handles databases created before password hashing was implemented)
            var adminCmd = connection.CreateCommand();
            adminCmd.CommandText = @"SELECT Id, PasswordHash FROM Users WHERE Username = $user AND Role = $role";
            adminCmd.Parameters.AddWithValue("$user", "admin");
            adminCmd.Parameters.AddWithValue("$role", UserRole.Administrator.ToString());
            using var reader = await adminCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                int adminId = reader.GetInt32(0);
                string existingHash = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);

                if (string.IsNullOrWhiteSpace(existingHash))
                {
                    var passwordHasher = new Security.PasswordHasher();
                    string passwordHash = passwordHasher.Hash(_defaultAdminPassword);

                    var updateCmd = connection.CreateCommand();
                    updateCmd.CommandText = @"UPDATE Users SET PasswordHash = $hash WHERE Id = $id";
                    updateCmd.Parameters.AddWithValue("$hash", passwordHash);
                    updateCmd.Parameters.AddWithValue("$id", adminId);
                    await updateCmd.ExecuteNonQueryAsync();

                    _logger.LogInformation("Updated administrator password hash for user ID {AdminId}", adminId);
                }
            }
        }
    }
}
