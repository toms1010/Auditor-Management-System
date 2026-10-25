namespace AuditorManagementSystem.Infrastructure.Repositories;

using AuditorManagementSystem.Core.Entities;
using AuditorManagementSystem.Core.Enums;
using AuditorManagementSystem.Core.Interfaces;
using Microsoft.Data.Sqlite;
using Dapper;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private SqliteConnection CreateConnection() => new SqliteConnection(_connectionString);

    public async Task<User?> GetByIdAsync(int id)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Id = $id", new { id });
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Username = $username", new { username });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Email = $email", new { email });
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<User>("SELECT * FROM Users ORDER BY Username");
    }

    public async Task<int> AddAsync(User user)
    {
        using var conn = CreateConnection();
        var sql = @"INSERT INTO Users (Username, Email, FullName, Role, IsActive, LastLoginAt, PasswordHash, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
                    VALUES ($user, $email, $name, $role, $active, $login, $hash, $now, $now, $by, $by);
                    SELECT last_insert_rowid();";
        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            user = user.Username,
            email = user.Email,
            name = user.FullName,
            role = user.Role.ToString(),
            active = user.IsActive ? 1 : 0,
            login = user.LastLoginAt?.ToString("o"),
            hash = user.PasswordHash,
            now = DateTime.UtcNow.ToString("o"),
            by = "system"
        });
    }

    public async Task<bool> UpdateAsync(User user)
    {
        using var conn = CreateConnection();
        var sql = @"UPDATE Users SET Email=$email, FullName=$name, Role=$role, IsActive=$active,
                    LastLoginAt=$login, UpdatedAt=$now, UpdatedBy=$by WHERE Id=$id";
        var rows = await conn.ExecuteAsync(sql, new
        {
            email = user.Email,
            name = user.FullName,
            role = user.Role.ToString(),
            active = user.IsActive ? 1 : 0,
            login = user.LastLoginAt?.ToString("o"),
            now = DateTime.UtcNow.ToString("o"),
            by = user.UpdatedBy ?? "system",
            id = user.Id
        });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteAsync("DELETE FROM Users WHERE Id=$id", new { id });
        return rows > 0;
    }

    public async Task<bool> ExistsAsync(string username)
    {
        using var conn = CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Users WHERE Username = $username", new { username });
        return count > 0;
    }
}
