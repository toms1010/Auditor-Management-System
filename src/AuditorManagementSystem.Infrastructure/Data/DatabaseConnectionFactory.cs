namespace AuditorManagementSystem.Infrastructure.Data;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

/// <summary>
/// Creates SQLite connections using the connection string from configuration.
/// Falls back to a local application-data database file when no connection string is configured.
/// </summary>
public class DatabaseConnectionFactory
{
    private readonly string _connectionString;

    public DatabaseConnectionFactory(IConfiguration configuration)
    {
        var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(configuredConnectionString))
        {
            // Fall back to a local SQLite database in the user's application data folder
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbPath = Path.Combine(appData, "AuditorManagementSystem", "audit.db");
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            _connectionString = $"Data Source={dbPath}";
        }
        else
        {
            _connectionString = configuredConnectionString;
        }
    }

    public SqliteConnection CreateConnection() => new SqliteConnection(_connectionString);

    /// <summary>
    /// Returns the database file path (useful for backups).
    /// </summary>
    public string GetDatabasePath()
    {
        var builder = new SqliteConnectionStringBuilder(_connectionString);
        return builder.DataSource;
    }
}
