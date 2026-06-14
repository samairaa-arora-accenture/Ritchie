using Richie.Application.Assets;
using Richie.Application.Audit;
using Richie.Application.Authentication;
using Richie.Application.Insurance;
using Richie.Domain.Audit;
using Richie.Domain.Insurance;
using Richie.Infrastructure.Persistence;

namespace Richie.Infrastructure.Audit;

/// <summary>
/// Assembles the Financial Health Audit from the user's real data: it groups the asset allocation
/// into broad <see cref="BenchmarkClass"/>es, reads the profile age, checks insurance coverage and
/// goal progress, then runs the (interim, §22) <see cref="IScoringEngine"/> and benchmark provider.
/// </summary>
public sealed class HealthAuditService : IHealthAuditService
{
    private readonly IAssetService _assets;
    private readonly IGoalService _goals;
    private readonly IInsuranceService _insurance;
    private readonly IScoringEngine _scoring;
    private readonly IBenchmarkProvider _benchmark;
    private readonly IUserSession _session;
    private readonly IAppDbContextFactory _factory;

    public HealthAuditService(
        IAssetService assets, IGoalService goals, IInsuranceService insurance,
        IScoringEngine scoring, IBenchmarkProvider benchmark, IUserSession session, IAppDbContextFactory factory)
    {
        _assets = assets;
        _goals = goals;
        _insurance = insurance;
        _scoring = scoring;
        _benchmark = benchmark;
        _session = session;
        _factory = factory;
    }

    private Guid UserId => _session.UserId ?? throw new InvalidOperationException("No authenticated user.");

    public HealthAuditReport GetReport()
    {
        Guid userId = UserId;
        int age = ReadAge(userId);

        PortfolioSummary portfolio = _assets.GetPortfolioSummary();
        bool hasAssets = portfolio.Allocation.Count > 0 && portfolio.TotalCurrentValue > 0;

        // Group the per-type allocation into broad benchmark classes.
        Dictionary<BenchmarkClass, decimal> actual = AssetClassMap.All.ToDictionary(c => c, _ => 0m);
        foreach (AllocationSlice slice in portfolio.Allocation)
            actual[AssetClassMap.For(slice.Type)] += slice.Percent;

        IReadOnlyDictionary<BenchmarkClass, decimal> recommended = _benchmark.RecommendedAllocation(age);
        int distinctClasses = actual.Count(kv => kv.Value > 0);

        IReadOnlyList<InsurancePolicySummary> policies = _insurance.GetPolicies();
        bool hasHealth = policies.Any(p => p.Type == InsuranceType.Health);
        bool hasTermLife = policies.Any(p => p.Type == InsuranceType.TermLife);

        IReadOnlyList<GoalProgress> goals = _goals.GetGoals();
        decimal avgGoalProgress = goals.Count > 0 ? goals.Average(g => g.PercentComplete) : 0;

        var input = new ScoringInput(age, actual, recommended, distinctClasses, hasAssets,
            hasHealth, hasTermLife, goals.Count, avgGoalProgress);

        HealthScoreResult health = _scoring.ComputeHealth(input);
        RiskScoreResult risk = _scoring.ComputeRisk(input);

        var benchmarkRows = new List<BenchmarkRow>();
        var missing = new List<string>();
        foreach (BenchmarkClass cls in AssetClassMap.All)
        {
            decimal a = actual[cls];
            decimal r = recommended.TryGetValue(cls, out decimal rec) ? rec : 0m;
            BenchmarkStatus status = a < r - _benchmark.TolerancePoints ? BenchmarkStatus.Under
                : a > r + _benchmark.TolerancePoints ? BenchmarkStatus.Over
                : BenchmarkStatus.OnTarget;
            benchmarkRows.Add(new BenchmarkRow(AssetClassMap.Display(cls), Math.Round(a, 1), r, status));
            if (hasAssets && a == 0)
                missing.Add(AssetClassMap.Display(cls));
        }

        var goalRows = goals
            .OrderByDescending(g => g.PercentComplete)
            .Select(g => new GoalProgressRow(g.Name, Math.Round(g.PercentComplete, 1)))
            .ToList();

        var coverageGaps = new List<string>();
        if (!hasHealth) coverageGaps.Add("No health insurance on record.");
        if (!hasTermLife) coverageGaps.Add("No term life insurance on record.");

        var suggestions = BuildSuggestions(hasAssets, age, benchmarkRows, missing, distinctClasses, coverageGaps);

        return new HealthAuditReport(
            hasAssets,
            health.Score, health.Rating, health.Factors,
            risk.Score, risk.Band, risk.Interpretation,
            _benchmark.BandName(_benchmark.BandFor(age)),
            benchmarkRows, missing, distinctClasses, goalRows, coverageGaps, suggestions,
            _scoring.IsInterim || _benchmark.IsInterim);
    }

    private List<string> BuildSuggestions(
        bool hasAssets, int age, List<BenchmarkRow> benchmark, List<string> missing,
        int distinctClasses, List<string> coverageGaps)
    {
        var s = new List<string>();
        if (!hasAssets)
        {
            s.Add("Add your assets to see a full health analysis and benchmark comparison.");
            return s;
        }

        foreach (BenchmarkRow row in benchmark)
        {
            if (row.Status == BenchmarkStatus.Over)
                s.Add($"Your {row.ClassName} allocation is {row.ActualPercent:0.#}%, above the recommended {row.RecommendedPercent:0}% for your age group. Consider rebalancing.");
            else if (row.Status == BenchmarkStatus.Under && row.RecommendedPercent > 0)
                s.Add($"Your {row.ClassName} allocation is {row.ActualPercent:0.#}%, below the recommended {row.RecommendedPercent:0}% for your age group.");
        }

        if (missing.Contains("Debt"))
            s.Add("You have no debt instruments in your portfolio. This increases your overall risk.");
        if (distinctClasses < 3)
            s.Add($"Your portfolio spans only {distinctClasses} of 4 broad asset classes — consider diversifying.");

        s.AddRange(coverageGaps.Select(g => g.Replace(" on record.", "") + " — consider adding cover."));

        if (s.Count == 0)
            s.Add("Your portfolio looks well-balanced for your age group. Keep it up.");
        return s;
    }

    private int ReadAge(Guid userId)
    {
        using RichieDbContext db = _factory.Create();
        return db.Users.Where(u => u.Id == userId).Select(u => u.Age).FirstOrDefault();
    }
}
