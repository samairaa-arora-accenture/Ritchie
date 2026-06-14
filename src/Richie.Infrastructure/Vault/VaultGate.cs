using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Richie.Application.Abstractions;
using Richie.Application.Authentication;
using Richie.Application.Security;
using Richie.Application.Vault;
using Richie.Domain.Auditing;
using Richie.Domain.Authentication;
using Richie.Domain.Vault;
using Richie.Infrastructure.Auditing;
using Richie.Infrastructure.Persistence;

namespace Richie.Infrastructure.Vault;

/// <summary>
/// Envelope-encryption gate for the vault. A random 256-bit DEK encrypts credentials; the DEK is
/// wrapped by a PBKDF2-derived KEK (master password + salt). Unwrapping is the password check —
/// AES-GCM authentication fails for a wrong password. The DEK lives in memory only while unlocked,
/// and only for the user it was unlocked for (a session change implicitly re-locks).
/// </summary>
public sealed class VaultGate : IVaultGate
{
    private const int DekLength = 32;     // 256-bit data-encryption key
    private const int SaltLength = 16;
    private const int Iterations = 100_000;
    private const int MinMasterPasswordLength = 8;

    private readonly IAppDbContextFactory _factory;
    private readonly IUserSession _session;
    private readonly IKeyDerivation _kdf;
    private readonly IFieldCipher _cipher;
    private readonly IPasswordHasher _hasher;
    private readonly IClock _clock;

    private byte[]? _dek;
    private Guid? _unlockedFor;

    public VaultGate(
        IAppDbContextFactory factory, IUserSession session,
        IKeyDerivation kdf, IFieldCipher cipher, IPasswordHasher hasher, IClock clock)
    {
        _factory = factory;
        _session = session;
        _kdf = kdf;
        _cipher = cipher;
        _hasher = hasher;
        _clock = clock;
    }

    private Guid UserId => _session.UserId ?? throw new InvalidOperationException("No authenticated user.");

    public bool IsUnlocked => _dek is not null && _unlockedFor == _session.UserId;

    public bool IsConfigured()
    {
        Guid userId = UserId;
        using RichieDbContext db = _factory.Create();
        return db.VaultKeys.Any(k => k.UserId == userId);
    }

    public VaultUnlockResult SetupMasterPassword(string masterPassword)
    {
        Guid userId = UserId;
        if (string.IsNullOrEmpty(masterPassword) || masterPassword.Length < MinMasterPasswordLength)
            return new VaultUnlockResult(VaultUnlockStatus.ValidationFailed,
                $"Master password must be at least {MinMasterPasswordLength} characters.");

        using RichieDbContext db = _factory.Create();
        if (db.VaultKeys.Any(k => k.UserId == userId))
            return new VaultUnlockResult(VaultUnlockStatus.ValidationFailed, "A vault master password is already set.");

        byte[] salt = _kdf.GenerateSalt(SaltLength);
        byte[] kek = _kdf.DeriveKey(masterPassword, salt, Iterations, DekLength);
        byte[] dek = RandomNumberGenerator.GetBytes(DekLength);
        string wrappedDek = _cipher.Encrypt(Convert.ToBase64String(dek), kek);

        DateTime now = _clock.UtcNow;
        db.VaultKeys.Add(new VaultKey
        {
            UserId = userId,
            Salt = salt,
            Iterations = Iterations,
            WrappedDek = wrappedDek,
            CreatedUtc = now
        });
        AuditWriter.Add(db, userId, now, "Vault", AuditAction.Create, nameof(VaultKey), userId,
            "Vault master password configured.");
        db.SaveChanges();

        _dek = dek;
        _unlockedFor = userId;
        return new VaultUnlockResult(VaultUnlockStatus.Success);
    }

    public VaultUnlockResult Unlock(string masterPassword)
    {
        if (TryUnwrap(masterPassword, out byte[]? dek))
        {
            _dek = dek;
            _unlockedFor = UserId;
            return new VaultUnlockResult(VaultUnlockStatus.Success);
        }
        return new VaultUnlockResult(VaultUnlockStatus.IncorrectPassword, "Incorrect master password.");
    }

    public bool Verify(string masterPassword) => TryUnwrap(masterPassword, out _);

    public void Lock()
    {
        if (_dek is not null)
            CryptographicOperations.ZeroMemory(_dek);
        _dek = null;
        _unlockedFor = null;
    }

    public VaultUnlockResult ChangeMasterPassword(string currentPassword, string newPassword)
    {
        if (TooShort(newPassword, out VaultUnlockResult? invalid))
            return invalid!;

        Guid userId = UserId;
        using RichieDbContext db = _factory.Create();
        VaultKey? key = db.VaultKeys.FirstOrDefault(k => k.UserId == userId);
        if (key is null)
            return new VaultUnlockResult(VaultUnlockStatus.ValidationFailed, "The vault is not set up.");

        if (!TryUnwrap(currentPassword, key.Salt, key.Iterations, key.WrappedDek, out byte[]? dek))
            return new VaultUnlockResult(VaultUnlockStatus.IncorrectPassword, "Incorrect current master password.");

        ReWrapMaster(key, dek!, newPassword);
        AuditWriter.Add(db, userId, _clock.UtcNow, "Vault", AuditAction.Update, nameof(VaultKey), userId,
            "Vault master password changed.");
        db.SaveChanges();

        _dek = dek;
        _unlockedFor = userId;
        return new VaultUnlockResult(VaultUnlockStatus.Success);
    }

    public VaultUnlockResult SetMasterPassword(string newPassword)
    {
        if (!IsUnlocked)
            return new VaultUnlockResult(VaultUnlockStatus.ValidationFailed, "The vault is locked.");
        if (TooShort(newPassword, out VaultUnlockResult? invalid))
            return invalid!;

        Guid userId = UserId;
        using RichieDbContext db = _factory.Create();
        VaultKey? key = db.VaultKeys.FirstOrDefault(k => k.UserId == userId);
        if (key is null)
            return new VaultUnlockResult(VaultUnlockStatus.ValidationFailed, "The vault is not set up.");

        ReWrapMaster(key, _dek!, newPassword);
        AuditWriter.Add(db, userId, _clock.UtcNow, "Vault", AuditAction.Update, nameof(VaultKey), userId,
            "Vault master password reset via recovery.");
        db.SaveChanges();
        return new VaultUnlockResult(VaultUnlockStatus.Success);
    }

    public bool IsRecoveryEnabled()
    {
        Guid userId = UserId;
        using RichieDbContext db = _factory.Create();
        return db.VaultKeys.Any(k => k.UserId == userId && k.RecoveryWrappedDek != null);
    }

    public IReadOnlyList<SecurityQuestion> GetRecoveryQuestions()
    {
        Guid userId = UserId;
        using RichieDbContext db = _factory.Create();
        return db.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.SecurityAnswers)
            .Select(a => a.Question)
            .OrderBy(q => q)
            .ToList();
    }

    public VaultUnlockResult EnableRecovery(IReadOnlyList<SecurityAnswerInput> answers)
    {
        if (!IsUnlocked)
            return new VaultUnlockResult(VaultUnlockStatus.ValidationFailed, "Unlock the vault first.");

        Guid userId = UserId;
        using RichieDbContext db = _factory.Create();
        User? user = db.Users.Include(u => u.SecurityAnswers).FirstOrDefault(u => u.Id == userId);
        if (user is null)
            return new VaultUnlockResult(VaultUnlockStatus.ValidationFailed, "User not found.");
        if (!AnswersMatch(user, answers))
            return new VaultUnlockResult(VaultUnlockStatus.IncorrectPassword,
                "Those answers don't match your security questions.");

        VaultKey? key = db.VaultKeys.FirstOrDefault(k => k.UserId == userId);
        if (key is null)
            return new VaultUnlockResult(VaultUnlockStatus.ValidationFailed, "The vault is not set up.");

        byte[] salt = _kdf.GenerateSalt(SaltLength);
        byte[] kek = DeriveRecoveryKek(answers, salt);
        key.RecoverySalt = salt;
        key.RecoveryWrappedDek = _cipher.Encrypt(Convert.ToBase64String(_dek!), kek);
        AuditWriter.Add(db, userId, _clock.UtcNow, "Vault", AuditAction.Update, nameof(VaultKey), userId,
            "Vault security-question recovery enabled.");
        db.SaveChanges();
        return new VaultUnlockResult(VaultUnlockStatus.Success);
    }

    public VaultUnlockResult UnlockWithAnswers(IReadOnlyList<SecurityAnswerInput> answers)
    {
        Guid userId = UserId;
        using RichieDbContext db = _factory.Create();
        VaultKey? key = db.VaultKeys.FirstOrDefault(k => k.UserId == userId);
        if (key?.RecoveryWrappedDek is null || key.RecoverySalt is null)
            return new VaultUnlockResult(VaultUnlockStatus.ValidationFailed, "Recovery is not set up.");

        byte[] kek = DeriveRecoveryKek(answers, key.RecoverySalt);
        if (!TryDecryptDek(key.RecoveryWrappedDek, kek, out byte[]? dek))
            return new VaultUnlockResult(VaultUnlockStatus.IncorrectPassword, "Those answers don't match.");

        _dek = dek;
        _unlockedFor = userId;
        return new VaultUnlockResult(VaultUnlockStatus.Success);
    }

    public string Encrypt(string plaintext) => _cipher.Encrypt(plaintext, RequireKey());

    public string Decrypt(string cipher) => _cipher.Decrypt(cipher, RequireKey());

    private byte[] RequireKey() =>
        IsUnlocked ? _dek! : throw new InvalidOperationException("The vault is locked.");

    private void ReWrapMaster(VaultKey key, byte[] dek, string newPassword)
    {
        byte[] salt = _kdf.GenerateSalt(SaltLength);
        byte[] kek = _kdf.DeriveKey(newPassword, salt, Iterations, DekLength);
        key.Salt = salt;
        key.Iterations = Iterations;
        key.WrappedDek = _cipher.Encrypt(Convert.ToBase64String(dek), kek);
    }

    private byte[] DeriveRecoveryKek(IReadOnlyList<SecurityAnswerInput> answers, byte[] salt)
    {
        string combined = string.Join(' ',
            answers.OrderBy(a => a.Question).Select(a => Normalize(a.Answer)));
        return _kdf.DeriveKey(combined, salt, Iterations, DekLength);
    }

    private bool AnswersMatch(User user, IReadOnlyList<SecurityAnswerInput> answers)
    {
        if (answers.Count != user.SecurityAnswers.Count)
            return false;

        return user.SecurityAnswers.All(stored =>
        {
            SecurityAnswerInput? provided = answers.FirstOrDefault(a => a.Question == stored.Question);
            return provided is not null && _hasher.Verify(Normalize(provided.Answer), stored.AnswerHash);
        });
    }

    private bool TryUnwrap(string masterPassword, out byte[]? dek)
    {
        dek = null;
        Guid userId = UserId;

        using RichieDbContext db = _factory.Create();
        VaultKey? key = db.VaultKeys.FirstOrDefault(k => k.UserId == userId);
        return key is not null && TryUnwrap(masterPassword, key.Salt, key.Iterations, key.WrappedDek, out dek);
    }

    private bool TryUnwrap(string password, byte[] salt, int iterations, string wrapped, out byte[]? dek)
    {
        byte[] kek = _kdf.DeriveKey(password, salt, iterations, DekLength);
        return TryDecryptDek(wrapped, kek, out dek);
    }

    private bool TryDecryptDek(string wrapped, byte[] kek, out byte[]? dek)
    {
        dek = null;
        try
        {
            dek = Convert.FromBase64String(_cipher.Decrypt(wrapped, kek));
            return true;
        }
        catch (CryptographicException)
        {
            // Wrong password/answers → KEK is wrong → GCM tag verification fails.
            return false;
        }
    }

    private static bool TooShort(string password, out VaultUnlockResult? invalid)
    {
        if (string.IsNullOrEmpty(password) || password.Length < MinMasterPasswordLength)
        {
            invalid = new VaultUnlockResult(VaultUnlockStatus.ValidationFailed,
                $"Master password must be at least {MinMasterPasswordLength} characters.");
            return true;
        }
        invalid = null;
        return false;
    }

    private static string Normalize(string answer) => answer.Trim().ToLowerInvariant();
}
