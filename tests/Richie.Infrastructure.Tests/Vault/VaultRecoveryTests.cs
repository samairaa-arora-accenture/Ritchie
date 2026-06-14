using Richie.Application.Authentication;
using Richie.Application.Vault;
using Richie.Domain.Authentication;
using Richie.Infrastructure.Authentication;
using Richie.Infrastructure.Security;
using Richie.Infrastructure.Tests.Helpers;
using Richie.Infrastructure.Vault;

namespace Richie.Infrastructure.Tests.Vault;

public sealed class VaultRecoveryTests : IDisposable
{
    private readonly TempSqlCipherDatabase _db = new();
    private readonly FakeClock _clock = new();
    private readonly UserSession _session = new();
    private readonly Argon2PasswordHasher _hasher = new();
    private readonly VaultGate _gate;

    private static readonly List<SecurityAnswerInput> CorrectAnswers =
    [
        new(SecurityQuestion.MothersMaidenName, "Smith"),
        new(SecurityQuestion.CityOfBirth, "Paris"),
        new(SecurityQuestion.FavouriteFood, "Pizza")
    ];

    public VaultRecoveryTests()
    {
        var auth = new AuthService(_db, _hasher, _clock);
        auth.Signup(new SignupRequest("Test User", "tester", "password1", 30, "City", CorrectAnswers));
        LoginResult login = auth.Login("tester", "password1");
        _session.SignIn(login.UserId!.Value, "Test User");

        _gate = new VaultGate(_db, _session, new Pbkdf2KeyDerivation(), new AesGcmFieldCipher(), _hasher, _clock);
        _gate.SetupMasterPassword("master-pass-1");
    }

    [Fact]
    public void GetRecoveryQuestions_ReturnsTheUsersThree()
    {
        Assert.Equal(3, _gate.GetRecoveryQuestions().Count);
    }

    [Fact]
    public void EnableRecovery_ThenUnlockWithAnswers_RecoversTheSameKey()
    {
        string cipher = _gate.Encrypt("a-secret");

        Assert.False(_gate.IsRecoveryEnabled());
        Assert.True(_gate.EnableRecovery(CorrectAnswers).IsSuccess);
        Assert.True(_gate.IsRecoveryEnabled());

        _gate.Lock();
        Assert.False(_gate.IsUnlocked);

        Assert.True(_gate.UnlockWithAnswers(CorrectAnswers).IsSuccess);
        Assert.True(_gate.IsUnlocked);
        Assert.Equal("a-secret", _gate.Decrypt(cipher));   // same DEK recovered
    }

    [Fact]
    public void EnableRecovery_WithWrongAnswers_Fails()
    {
        List<SecurityAnswerInput> wrong =
        [
            new(SecurityQuestion.MothersMaidenName, "Wrong"),
            new(SecurityQuestion.CityOfBirth, "Paris"),
            new(SecurityQuestion.FavouriteFood, "Pizza")
        ];

        Assert.Equal(VaultUnlockStatus.IncorrectPassword, _gate.EnableRecovery(wrong).Status);
        Assert.False(_gate.IsRecoveryEnabled());
    }

    [Fact]
    public void UnlockWithAnswers_WrongAnswers_Fails()
    {
        _gate.EnableRecovery(CorrectAnswers);
        _gate.Lock();

        List<SecurityAnswerInput> wrong =
        [
            new(SecurityQuestion.MothersMaidenName, "Nope"),
            new(SecurityQuestion.CityOfBirth, "Nope"),
            new(SecurityQuestion.FavouriteFood, "Nope")
        ];

        Assert.Equal(VaultUnlockStatus.IncorrectPassword, _gate.UnlockWithAnswers(wrong).Status);
        Assert.False(_gate.IsUnlocked);
    }

    [Fact]
    public void RecoveryUnlock_ThenSetNewMasterPassword_AllowsFutureUnlock()
    {
        _gate.EnableRecovery(CorrectAnswers);
        _gate.Lock();

        Assert.True(_gate.UnlockWithAnswers(CorrectAnswers).IsSuccess);
        Assert.True(_gate.SetMasterPassword("new-master-pass").IsSuccess);

        _gate.Lock();
        Assert.True(_gate.Unlock("new-master-pass").IsSuccess);
        Assert.False(_gate.Unlock("master-pass-1").IsSuccess);  // original replaced
    }

    public void Dispose() => _db.Dispose();
}
