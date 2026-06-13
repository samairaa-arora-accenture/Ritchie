using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Richie.Infrastructure.Persistence;

/// <summary>
/// Used only by the EF Core tools (e.g. <c>dotnet ef migrations add</c>) to construct the
/// context at design time. Migrations scaffold from the model, so no encryption key is
/// needed here — the runtime opens the real encrypted file via <see cref="SqlCipherDbContextFactory"/>.
/// </summary>
public sealed class RichieDbContextDesignTimeFactory : IDesignTimeDbContextFactory<RichieDbContext>
{
    public RichieDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<RichieDbContext> options = new DbContextOptionsBuilder<RichieDbContext>()
            .UseSqlite("Data Source=richie_design.db")
            .Options;

        return new RichieDbContext(options);
    }
}
