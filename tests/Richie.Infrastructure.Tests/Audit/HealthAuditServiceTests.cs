using Richie.Application.Assets;
using Richie.Application.Audit;
using Richie.Application.Authentication;
using Richie.Domain.Assets;
using Richie.Domain.Insurance;
using Richie.Application.Insurance;
using Richie.Domain.Audit;
using Richie.Infrastructure.Assets;
using Richie.Infrastructure.Authentication;
using Richie.Infrastructure.Insurance;
using Richie.Infrastructure.Security;
using Richie.Infrastructure.Tests.Helpers;

namespace Richie.Infrastructure.Tests.Audit;

public sealed class HealthAuditServiceTests : IDisposable
{
    private readonly TempSqlCipherDatabase _db = new();
    private readonly FakeClock _clock = new();
    private readonly UserSession _session = new();
    private readonly AssetService _assets;
    private readonly InsuranceService _insurance;
    private readonly Infrastructure.Audit.HealthAuditService _sut;

    public HealthAuditServiceTests()
    {
        var hasher = new Argon2PasswordHasher();
        var auth = new AuthService(_db, hasher, _clock);
        auth.Signup(new SignupRequest("Young User", "young", "password1", 30, "City",
        [
            new(Richie.Domain.Authentication.SecurityQuestion.MothersMaidenName, "a"),
            new(Richie.Domain.Authentication.SecurityQuestion.CityOfBirth, "b"),
            new(Richie.Domain.Authentication.SecurityQuestion.FavouriteFood, "c")
        ]));
        LoginResult login = auth.Login("young", "password1");
        _session.SignIn(login.UserId!.Value, "Young User");

        _assets = new AssetService(_db, new ValuationService(), _session, _clock);
        var goals = new GoalService(_db, _session, _clock);
        _insurance = new InsuranceService(_db, _session, _clock);
        _sut = new Infrastructure.Audit.HealthAuditService(
            _assets, goals, _insurance, new PlaceholderScoringEngine(), new AgeBandBenchmarkProvider(),
            _session, _db);
    }

    private void AddAsset(AssetType type, decimal currentValue) =>
        _assets.Create(new AssetInput
        {
            Type = type,
            Name = type.ToString(),
            InvestmentStartDate = _clock.UtcNow.AddYears(-1),
            InvestedAmount = currentValue,
            CurrentValue = currentValue,
            InvestmentMode = InvestmentMode.LumpSum
        });

    [Fact]
    public void NoAssets_ReportsNoData_AndFlagsInterim_AndCoverageGaps()
    {
        HealthAuditReport report = _sut.GetReport();

        Assert.False(report.HasAssets);
        Assert.Equal("No data", report.HealthRating);
        Assert.True(report.ScoresAreInterim);
        Assert.Equal(2, report.CoverageGaps.Count);                 // no health, no term life
        Assert.Contains(report.Suggestions, s => s.Contains("Add your assets"));
        Assert.Equal("Young (≤35)", report.AgeBandName);
    }

    [Fact]
    public void WithAssets_ComputesBenchmarkComparison_AndSuggestions()
    {
        AddAsset(AssetType.Equity, 100m);                            // 50% Equity
        AddAsset(AssetType.GuaranteedInvestmentPlan, 100m);          // 50% Debt
        _insurance.Create(new InsurancePolicyInput(InsuranceType.Health, "H", null, null, 1, 1,
            _clock.UtcNow, _clock.UtcNow.AddYears(1), null, null));  // health cover present

        HealthAuditReport report = _sut.GetReport();

        Assert.True(report.HasAssets);
        Assert.Equal(4, report.Benchmark.Count);                    // all 4 classes compared
        Assert.Equal(2, report.DistinctClassCount);

        // Debt 50% vs recommended 15% (young) → Over; Gold & Real Estate absent → missing.
        BenchmarkRow debt = report.Benchmark.Single(b => b.ClassName == "Debt");
        Assert.Equal(BenchmarkStatus.Over, debt.Status);
        Assert.Contains("Real Estate", report.MissingClasses);

        Assert.Contains(report.CoverageGaps, g => g.Contains("term life"));   // still missing term life
        Assert.DoesNotContain(report.CoverageGaps, g => g.Contains("health")); // health now covered
        Assert.NotEmpty(report.Suggestions);
        Assert.InRange(report.HealthScore, 0, 100);
        Assert.InRange(report.RiskScore, 0, 100);
    }

    public void Dispose() => _db.Dispose();
}
