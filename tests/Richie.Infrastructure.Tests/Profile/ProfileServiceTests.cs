using Richie.Application.Authentication;
using Richie.Application.Profile;
using Richie.Domain.Authentication;
using Richie.Infrastructure.Authentication;
using Richie.Infrastructure.Profile;
using Richie.Infrastructure.Security;
using Richie.Infrastructure.Settings;
using Richie.Infrastructure.Tests.Helpers;
using Richie.Infrastructure.Vault;

namespace Richie.Infrastructure.Tests.Profile;

public sealed class ProfileServiceTests : IDisposable
{
    private readonly TempSqlCipherDatabase _db = new();
    private readonly FakeClock _clock = new();
    private readonly UserSession _session = new();
    private readonly ProfileService _sut;

    public ProfileServiceTests()
    {
        var hasher = new Argon2PasswordHasher();
        var auth = new AuthService(_db, hasher, _clock);
        auth.Signup(new SignupRequest("Jane Doe", "jane", "password1", 28, "Pune",
        [
            new(SecurityQuestion.MothersMaidenName, "a"),
            new(SecurityQuestion.CityOfBirth, "b"),
            new(SecurityQuestion.FavouriteFood, "c")
        ]));
        _session.SignIn(auth.Login("jane", "password1").UserId!.Value, "Jane Doe");

        var gate = new VaultGate(_db, _session, new Pbkdf2KeyDerivation(), new AesGcmFieldCipher(), hasher, _clock);
        var settings = new AppSettingsService(_db, _session);
        var vault = new VaultService(_db, _session, gate, _clock);
        var health = new VaultHealthService(_db, _session, gate, _clock);
        _sut = new ProfileService(_db, _session, gate, health, settings, _clock);
    }

    [Fact]
    public void Get_ReturnsProfile_AndScoreReflectsDefaults()
    {
        ProfileData data = _sut.Get();

        Assert.Equal("Jane Doe", data.FullName);
        Assert.Equal("jane", data.Username);
        Assert.Equal(28, data.Age);
        // No vault yet, default lock 5 min → only the auto-lock factor (+20).
        Assert.Equal(20, data.SecurityScore);
    }

    [Fact]
    public void Update_ChangesEditableFields_AndValidatesAge()
    {
        Assert.True(_sut.Update(new ProfileUpdate("Jane Smith", 31, "Mumbai")));
        ProfileData data = _sut.Get();
        Assert.Equal("Jane Smith", data.FullName);
        Assert.Equal(31, data.Age);
        Assert.Equal("Mumbai", data.City);

        Assert.False(_sut.Update(new ProfileUpdate("X", 0, "Y")));   // invalid age
    }

    public void Dispose() => _db.Dispose();
}
