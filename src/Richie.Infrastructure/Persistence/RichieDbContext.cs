using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Richie.Domain.Authentication;

namespace Richie.Infrastructure.Persistence;

/// <summary>
/// The application's EF Core context over the SQLCipher-encrypted database.
/// Entity configurations live in <c>Persistence/Configurations</c> and are applied by convention.
/// </summary>
public class RichieDbContext : DbContext
{
    public RichieDbContext(DbContextOptions<RichieDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
