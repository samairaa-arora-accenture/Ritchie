using Richie.Application.Settings;
using Richie.Domain.Notifications;
using Richie.Infrastructure.Authentication;
using Richie.Infrastructure.Settings;
using Richie.Infrastructure.Tests.Helpers;

namespace Richie.Infrastructure.Tests.Settings;

public sealed class AppSettingsServiceTests : IDisposable
{
    private readonly TempSqlCipherDatabase _db = new();
    private readonly UserSession _session = new();
    private readonly AppSettingsService _sut;

    public AppSettingsServiceTests()
    {
        _session.SignIn(Guid.NewGuid(), "Tester");
        _sut = new AppSettingsService(_db, _session);
    }

    [Fact]
    public void Get_ReturnsDefaults_WhenNoneSaved()
    {
        AppSettingsData data = _sut.Get();
        Assert.Equal("System", data.Theme);
        Assert.Equal(5, data.SessionLockMinutes);
        Assert.True(data.IncludeJewelleryInPortfolio);
        Assert.Empty(data.DisabledNotificationTypes);
    }

    [Fact]
    public void Save_RoundTrips_IncludingDisabledTypes()
    {
        _sut.Save(new AppSettingsData("Dark", 15, false, "Weekly",
            [NotificationType.SipPosted, NotificationType.ExpenseAlert], null));

        AppSettingsData data = _sut.Get();
        Assert.Equal("Dark", data.Theme);
        Assert.Equal(15, data.SessionLockMinutes);
        Assert.False(data.IncludeJewelleryInPortfolio);
        Assert.Equal("Weekly", data.BackupFrequency);
        Assert.Contains(NotificationType.SipPosted, data.DisabledNotificationTypes);

        Assert.False(_sut.IsNotificationEnabled(NotificationType.SipPosted));
        Assert.True(_sut.IsNotificationEnabled(NotificationType.InsuranceRenewal));
    }

    public void Dispose() => _db.Dispose();
}
