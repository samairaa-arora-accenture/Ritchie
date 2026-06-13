using System.Security.Cryptography;
using Richie.Application.Security;

namespace Richie.Infrastructure.Persistence;

/// <summary>
/// Supplies the SQLCipher passphrase for the database file. On first run a random
/// 256-bit key is generated and stored DPAPI-protected (bound to the Windows user);
/// thereafter it is unwrapped from disk. The database therefore opens automatically
/// for this Windows account without prompting.
/// </summary>
public interface IDatabaseKeyProvider
{
    string GetOrCreateKey();
}

public sealed class DpapiDatabaseKeyProvider : IDatabaseKeyProvider
{
    private readonly IKeyProtector _protector;

    public DpapiDatabaseKeyProvider(IKeyProtector protector) => _protector = protector;

    public string GetOrCreateKey()
    {
        if (File.Exists(AppPaths.DatabaseKeyPath))
        {
            byte[] wrapped = File.ReadAllBytes(AppPaths.DatabaseKeyPath);
            return Convert.ToBase64String(_protector.Unprotect(wrapped));
        }

        byte[] key = RandomNumberGenerator.GetBytes(32);
        AppPaths.EnsureDirectories();
        File.WriteAllBytes(AppPaths.DatabaseKeyPath, _protector.Protect(key));
        return Convert.ToBase64String(key);
    }
}
