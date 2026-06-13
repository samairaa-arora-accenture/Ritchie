using Microsoft.EntityFrameworkCore;

namespace Richie.Infrastructure.Persistence;

/// <summary>
/// Ensures local directories exist and applies any pending EF Core migrations to the
/// encrypted database. Run once at application startup (during the splash screen).
/// </summary>
public interface IDatabaseInitializer
{
    void Initialize();
}

public sealed class DatabaseInitializer : IDatabaseInitializer
{
    private readonly IAppDbContextFactory _factory;

    public DatabaseInitializer(IAppDbContextFactory factory) => _factory = factory;

    public void Initialize()
    {
        AppPaths.EnsureDirectories();
        using RichieDbContext context = _factory.Create();
        context.Database.Migrate();
    }
}
