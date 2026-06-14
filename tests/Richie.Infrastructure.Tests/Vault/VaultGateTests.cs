using Richie.Application.Vault;
using Richie.Infrastructure.Authentication;
using Richie.Infrastructure.Persistence;
using Richie.Infrastructure.Security;
using Richie.Infrastructure.Tests.Helpers;
using Richie.Infrastructure.Vault;

namespace Richie.Infrastructure.Tests.Vault;

public sealed class VaultGateTests : IDisposable
{
    private readonly TempSqlCipherDatabase _db = new();
    private readonly FakeClock _clock = new();
    private readonly UserSession _session = new();
    private readonly VaultGate _gate;

    public VaultGateTests()
    {
        _session.SignIn(Guid.NewGuid(), "Tester");
        _gate = new VaultGate(_db, _session, new Pbkdf2KeyDerivation(), new AesGcmFieldCipher(),
            new Argon2PasswordHasher(), _clock);
    }

    [Fact]
    public void Setup_ThenUnlock_Succeeds_AndPersistsKey()
    {
        Assert.False(_gate.IsConfigured());

        VaultUnlockResult setup = _gate.SetupMasterPassword("master-pass-1");
        Assert.True(setup.IsSuccess);
        Assert.True(_gate.IsConfigured());
        Assert.True(_gate.IsUnlocked);   // auto-unlocks after setup

        _gate.Lock();
        Assert.False(_gate.IsUnlocked);

        VaultUnlockResult unlock = _gate.Unlock("master-pass-1");
        Assert.True(unlock.IsSuccess);
        Assert.True(_gate.IsUnlocked);
    }

    [Fact]
    public void Unlock_WithWrongPassword_Fails_AndStaysLocked()
    {
        _gate.SetupMasterPassword("master-pass-1");
        _gate.Lock();

        VaultUnlockResult result = _gate.Unlock("wrong-password");

        Assert.Equal(VaultUnlockStatus.IncorrectPassword, result.Status);
        Assert.False(_gate.IsUnlocked);
        Assert.False(_gate.Verify("wrong-password"));
        Assert.True(_gate.Verify("master-pass-1"));
    }

    [Fact]
    public void Setup_RejectsShortPassword_AndDoesNotConfigure()
    {
        VaultUnlockResult result = _gate.SetupMasterPassword("short");

        Assert.Equal(VaultUnlockStatus.ValidationFailed, result.Status);
        Assert.False(_gate.IsConfigured());
    }

    [Fact]
    public void EncryptDecrypt_RoundTrips_WhenUnlocked_AndThrowsWhenLocked()
    {
        _gate.SetupMasterPassword("master-pass-1");

        string cipher = _gate.Encrypt("hunter2");
        Assert.NotEqual("hunter2", cipher);
        Assert.Equal("hunter2", _gate.Decrypt(cipher));

        _gate.Lock();
        Assert.Throws<InvalidOperationException>(() => _gate.Decrypt(cipher));
        Assert.Throws<InvalidOperationException>(() => _gate.Encrypt("x"));
    }

    [Fact]
    public void ChangeMasterPassword_WrongCurrent_Fails()
    {
        _gate.SetupMasterPassword("master-pass-1");

        VaultUnlockResult result = _gate.ChangeMasterPassword("wrong-current", "master-pass-2");

        Assert.Equal(VaultUnlockStatus.IncorrectPassword, result.Status);
    }

    [Fact]
    public void ChangeMasterPassword_OldStopsWorking_NewWorks_AndDataSurvives()
    {
        _gate.SetupMasterPassword("master-pass-1");
        string cipher = _gate.Encrypt("a-secret");

        VaultUnlockResult result = _gate.ChangeMasterPassword("master-pass-1", "master-pass-2");
        Assert.True(result.IsSuccess);
        Assert.Equal("a-secret", _gate.Decrypt(cipher)); // key preserved, stays unlocked

        _gate.Lock();
        Assert.False(_gate.Unlock("master-pass-1").IsSuccess);   // old password no longer works
        Assert.True(_gate.Unlock("master-pass-2").IsSuccess);    // new one does
        Assert.Equal("a-secret", _gate.Decrypt(cipher));         // same data key underneath
    }

    [Fact]
    public void SetMasterPassword_RequiresUnlocked()
    {
        _gate.SetupMasterPassword("master-pass-1");
        _gate.Lock();

        Assert.Equal(VaultUnlockStatus.ValidationFailed, _gate.SetMasterPassword("master-pass-2").Status);
    }

    public void Dispose() => _db.Dispose();
}
