using Richie.Application.Insurance;
using Richie.Domain.Insurance;
using Richie.Infrastructure.Authentication;
using Richie.Infrastructure.Insurance;
using Richie.Infrastructure.Notifications;
using Richie.Infrastructure.Tests.Helpers;

namespace Richie.Infrastructure.Tests.Insurance;

public sealed class InsuranceServiceTests : IDisposable
{
    private readonly TempSqlCipherDatabase _db = new();
    private readonly FakeClock _clock = new();
    private readonly UserSession _session = new();
    private readonly InsuranceService _sut;
    private readonly NotificationService _notifications;

    public InsuranceServiceTests()
    {
        _session.SignIn(Guid.NewGuid(), "Tester");
        _sut = new InsuranceService(_db, _session, _clock);
        _notifications = new NotificationService(_db, _session, new Richie.Infrastructure.Settings.AppSettingsService(_db, _session));
    }

    private InsurancePolicyInput Input(InsuranceType type = InsuranceType.Health, DateTime? renewal = null) =>
        new(type, "My Policy", "POL-123", "Acme Insure", 500_000m, 12_000m,
            _clock.UtcNow.AddYears(-1), renewal ?? _clock.UtcNow.AddMonths(6), "Spouse", "notes");

    [Fact]
    public void Create_Then_GetById_RoundTrips()
    {
        Guid id = _sut.Create(Input(InsuranceType.TermLife));

        InsurancePolicyInput? loaded = _sut.GetById(id);
        Assert.NotNull(loaded);
        Assert.Equal(InsuranceType.TermLife, loaded!.Type);
        Assert.Equal(500_000m, loaded.CoverageAmount);

        InsurancePolicySummary summary = Assert.Single(_sut.GetPolicies());
        Assert.Equal("Term Life", summary.TypeName);
    }

    [Fact]
    public void Update_And_Delete_Work()
    {
        Guid id = _sut.Create(Input());
        Assert.True(_sut.Update(id, Input() with { PolicyName = "Renamed" }));
        Assert.Equal("Renamed", _sut.GetPolicies().Single().PolicyName);

        Assert.True(_sut.Delete(id));
        Assert.Empty(_sut.GetPolicies());
    }

    [Fact]
    public void Operations_AreScopedToTheUser()
    {
        Guid id = _sut.Create(Input());

        _session.SignOut();
        _session.SignIn(Guid.NewGuid(), "Other");

        Assert.Empty(_sut.GetPolicies());
        Assert.Null(_sut.GetById(id));
        Assert.False(_sut.Delete(id));
    }

    [Fact]
    public void ProcessDueRenewals_NotifiesOnce_WithinLeadWindow()
    {
        _sut.Create(Input(renewal: _clock.UtcNow.AddDays(20)));   // within 30 days

        Assert.Equal(1, _sut.ProcessDueRenewals(_clock.UtcNow));
        Assert.Equal(0, _sut.ProcessDueRenewals(_clock.UtcNow));  // deduped for the same renewal date
        Assert.Equal(1, _notifications.GetUnreadCount());
    }

    [Fact]
    public void ProcessDueRenewals_IgnoresPoliciesOutsideTheWindow()
    {
        _sut.Create(Input(renewal: _clock.UtcNow.AddDays(60)));   // beyond 30 days

        Assert.Equal(0, _sut.ProcessDueRenewals(_clock.UtcNow));
        Assert.Equal(0, _notifications.GetUnreadCount());
    }

    [Fact]
    public void ChangingRenewalDate_ReArmsTheReminder()
    {
        Guid id = _sut.Create(Input(renewal: _clock.UtcNow.AddDays(20)));
        Assert.Equal(1, _sut.ProcessDueRenewals(_clock.UtcNow));

        // Renewed for another term — moving the date past the window then back re-arms via the changed date.
        _sut.Update(id, Input(renewal: _clock.UtcNow.AddDays(10)));
        Assert.Equal(1, _sut.ProcessDueRenewals(_clock.UtcNow));
        Assert.Equal(2, _notifications.GetUnreadCount());
    }

    public void Dispose() => _db.Dispose();
}
