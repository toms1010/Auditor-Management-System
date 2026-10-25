namespace AuditorManagementSystem.Infrastructure.Repositories;

using AuditorManagementSystem.Core.Entities;
using AuditorManagementSystem.Core.Interfaces;
using Microsoft.Data.Sqlite;
using Dapper;

public class CategoryRepository : ICategoryRepository
{
    private readonly string _connectionString;

    public CategoryRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private SqliteConnection CreateConnection() => new SqliteConnection(_connectionString);

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Category>(
            "SELECT * FROM Categories WHERE IsActive = 1 ORDER BY Name");
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Category>(
            "SELECT * FROM Categories WHERE Id = $id", new { id });
    }

    public async Task<int> AddAsync(Category category)
    {
        using var conn = CreateConnection();
        var sql = @"INSERT INTO Categories (Name, Description, IsActive, CreatedAt)
                    VALUES ($name, $desc, $active, $now);
                    SELECT last_insert_rowid();";
        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            name = category.Name,
            desc = category.Description,
            active = category.IsActive ? 1 : 0,
            now = DateTime.UtcNow.ToString("o")
        });
    }

    public async Task<bool> UpdateAsync(Category category)
    {
        using var conn = CreateConnection();
        var sql = @"UPDATE Categories SET Name=$name, Description=$desc, IsActive=$active,
                    UpdatedAt=$now WHERE Id=$id";
        var rows = await conn.ExecuteAsync(sql, new
        {
            name = category.Name,
            desc = category.Description,
            active = category.IsActive ? 1 : 0,
            now = DateTime.UtcNow.ToString("o"),
            id = category.Id
        });
        return rows > 0;
    }
}
