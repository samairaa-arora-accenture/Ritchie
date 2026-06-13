namespace Richie.Infrastructure.Persistence;

/// <summary>
/// Creates a <see cref="RichieDbContext"/> bound to the SQLCipher-encrypted database file,
/// using the key resolved from <see cref="IDatabaseKeyProvider"/>.
/// </summary>
public interface IAppDbContextFactory
{
    RichieDbContext Create();
}
