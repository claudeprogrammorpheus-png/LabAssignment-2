using B2B.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace B2B.Tests;

/// <summary>
/// Real relational database (SQLite in-memory) with the same schema, constraints and seed data as production,
/// so transactions, FK and CHECK constraints are really exercised (the EF InMemory provider ignores them).
/// </summary>
public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _conn = new("DataSource=:memory:");
    private readonly DbContextOptions<B2BContext> _options;

    public TestDb()
    {
        _conn.Open();
        _options = new DbContextOptionsBuilder<B2BContext>().UseSqlite(_conn).Options;
        using var db = New();
        db.Database.EnsureCreated();
    }

    public B2BContext New() => new(_options);

    public int Stock(int itemId)
    {
        using var db = New();
        return db.Items.AsNoTracking().Single(i => i.ItemID == itemId).StockQuantity;
    }

    public void Dispose() => _conn.Dispose();
}
