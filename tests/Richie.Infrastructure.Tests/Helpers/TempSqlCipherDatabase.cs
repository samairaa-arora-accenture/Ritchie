using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Richie.Infrastructure.Persistence;

namespace Richie.Infrastructure.Tests.Helpers;

/// <summary>
/// A throwaway, migrated SQLCipher database on a temp file that doubles as an
/// <see cref="IAppDbContextFactory"/> for services under test. Disposed at test end.
/// </summary>
internal sealed class TempSqlCipherDatabase : IAppDbContextFactory, IDisposable
{
    private const string Key = "test-key-1234567890";
    private readonly string _path =
        Path.Combine(Path.GetTempPath(), $"richie-test-{Guid.NewGuid():N}.db");

    public TempSqlCipherDatabase()
    {
        using RichieDbContext db = Create();
        db.Database.Migrate();
    }

    public RichieDbContext Create() => SqlCipherDbContextFactory.Build(_path, Key);

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_path))
            File.Delete(_path);
    }
}
