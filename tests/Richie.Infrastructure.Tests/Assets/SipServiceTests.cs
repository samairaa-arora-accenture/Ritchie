using Richie.Application.Assets;
using Richie.Domain.Assets;
using Richie.Domain.Notifications;
using Richie.Infrastructure.Assets;
using Richie.Infrastructure.Authentication;
using Richie.Infrastructure.Notifications;
using Richie.Infrastructure.Tests.Helpers;

namespace Richie.Infrastructure.Tests.Assets;

public sealed class SipServiceTests : IDisposable
{
    private readonly TempSqlCipherDatabase _db = new();
    private readonly FakeClock _clock = new(); // 2026-01-01 00:00 UTC
    private readonly UserSession _session = new();
    private readonly AssetService _assets;
    private readonly SipService _sut;

    public SipServiceTests()
    {
        _session.SignIn(Guid.NewGuid(), "Tester");
        _assets = new AssetService(_db, new ValuationService(), _session, _clock);
        _sut = new SipService(_db, _session, _clock);
    }

    private Guid CreateAsset() => _assets.Create(new AssetInput
    {
        Type = AssetType.MutualFund,
        Name = "HDFC Flexicap",
        InvestmentStartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        InvestedAmount = 1000,
        CurrentValue = 1000,
        InvestmentMode = InvestmentMode.Sip,
    });

    private void EnableMonthlySip(Guid assetId, decimal amount = 500, int day = 15) =>
        _sut.SaveSchedule(assetId, new SipScheduleInput(
            IsEnabled: true, Amount: amount, DayOfMonth: day, Frequency: SipFrequency.Monthly,
            StartDate: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));

    private decimal InvestedOf(Guid id)
    {
        using var db = _db.Create();
        return db.Assets.Single(a => a.Id == id).InvestedAmount;
    }

    [Fact]
    public void SaveSchedule_SetsNextRunToNextDayOfMonth()
    {
        Guid id = CreateAsset();
        EnableMonthlySip(id, day: 15);

        SipScheduleDto schedule = _sut.GetSchedule(id)!;
        Assert.True(schedule.IsEnabled);
        Assert.Equal(new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), schedule.NextRunDateUtc);
    }

    [Fact]
    public void ProcessDueSips_PostsDueInstalment_AndRaisesNotification()
    {
        Guid id = CreateAsset();
        EnableMonthlySip(id, amount: 500, day: 15);

        Assert.Equal(0, _sut.ProcessDueSips(new DateTime(2026, 1, 14, 12, 0, 0, DateTimeKind.Utc)));

        int posted = _sut.ProcessDueSips(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));

        Assert.Equal(1, posted);
        Assert.Equal(1500m, InvestedOf(id));
        Assert.Single(_sut.GetHistory(id));
        Assert.Equal(new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc), _sut.GetSchedule(id)!.NextRunDateUtc);

        var notifications = new NotificationService(_db, _session, new Richie.Infrastructure.Settings.AppSettingsService(_db, _session)).GetRecent();
        Assert.Contains(notifications, n => n.Type == NotificationType.SipPosted);
    }

    [Fact]
    public void ProcessDueSips_CatchesUpMultipleMissedPeriods()
    {
        Guid id = CreateAsset();
        EnableMonthlySip(id, amount: 500, day: 15);

        // Jan 15, Feb 15, Mar 15 are all due as of Mar 20.
        int posted = _sut.ProcessDueSips(new DateTime(2026, 3, 20, 0, 0, 0, DateTimeKind.Utc));

        Assert.Equal(3, posted);
        Assert.Equal(2500m, InvestedOf(id));
        Assert.Equal(3, _sut.GetHistory(id).Count);
    }

    [Fact]
    public void ProcessDueSips_IgnoresDisabledSchedules()
    {
        Guid id = CreateAsset();
        _sut.SaveSchedule(id, new SipScheduleInput(false, 500, 15, SipFrequency.Monthly,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));

        Assert.Equal(0, _sut.ProcessDueSips(new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)));
        Assert.Equal(1000m, InvestedOf(id));
    }

    [Fact]
    public void GetUpcomingInstallments_ProjectsFutureDates()
    {
        Guid id = CreateAsset();
        EnableMonthlySip(id, day: 15);

        IReadOnlyList<DateTime> upcoming = _sut.GetUpcomingInstallments(id, 3);

        Assert.Equal(
        [
            new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc),
        ], upcoming);
    }

    public void Dispose() => _db.Dispose();
}
