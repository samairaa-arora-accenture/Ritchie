using Richie.Domain.Audit;

namespace Richie.Infrastructure.Tests.Audit;

public sealed class ScoringEngineTests
{
    private readonly PlaceholderScoringEngine _engine = new();
    private readonly AgeBandBenchmarkProvider _benchmark = new();

    private static Dictionary<BenchmarkClass, decimal> Alloc(decimal equity, decimal debt, decimal gold, decimal realEstate) =>
        new()
        {
            [BenchmarkClass.Equity] = equity,
            [BenchmarkClass.Debt] = debt,
            [BenchmarkClass.Gold] = gold,
            [BenchmarkClass.RealEstate] = realEstate
        };

    [Theory]
    [InlineData(30, AgeBand.Young)]
    [InlineData(35, AgeBand.Young)]
    [InlineData(36, AgeBand.MidCareer)]
    [InlineData(50, AgeBand.MidCareer)]
    [InlineData(51, AgeBand.PreRetirement)]
    [InlineData(70, AgeBand.PreRetirement)]
    public void Benchmark_BandsByAge(int age, AgeBand expected) => Assert.Equal(expected, _benchmark.BandFor(age));

    [Theory]
    [InlineData(25)]
    [InlineData(45)]
    [InlineData(60)]
    public void Benchmark_AllocationsSumTo100(int age)
    {
        Assert.Equal(100m, _benchmark.RecommendedAllocation(age).Values.Sum());
        Assert.True(_benchmark.IsInterim);
    }

    [Fact]
    public void Health_NoAssets_IsNoData()
    {
        var input = new ScoringInput(30, Alloc(0, 0, 0, 0), _benchmark.RecommendedAllocation(30),
            0, HasAssets: false, false, false, 0, 0);

        HealthScoreResult result = _engine.ComputeHealth(input);
        Assert.Equal(0, result.Score);
        Assert.Equal("No data", result.Rating);
    }

    [Fact]
    public void Health_OnBenchmark_Diversified_Protected_Goals_IsExcellent()
    {
        var recommended = _benchmark.RecommendedAllocation(30);
        var input = new ScoringInput(30, new Dictionary<BenchmarkClass, decimal>(recommended), recommended,
            DistinctClassCount: 4, HasAssets: true, HasHealthInsurance: true, HasTermLife: true,
            GoalCount: 1, AverageGoalProgressPercent: 100);

        HealthScoreResult result = _engine.ComputeHealth(input);
        Assert.Equal(100, result.Score);
        Assert.Equal("Excellent", result.Rating);
        Assert.Equal(4, result.Factors.Count);
    }

    [Fact]
    public void Health_Concentrated_NoProtection_NoGoals_NeedsAttention()
    {
        // 100% equity for a young investor: poorly diversified, off-benchmark, no cover, no goals.
        var input = new ScoringInput(30, Alloc(100, 0, 0, 0), _benchmark.RecommendedAllocation(30),
            DistinctClassCount: 1, HasAssets: true, HasHealthInsurance: false, HasTermLife: false,
            GoalCount: 0, AverageGoalProgressPercent: 0);

        HealthScoreResult result = _engine.ComputeHealth(input);
        Assert.True(result.Score < 60, $"expected <60, got {result.Score}");
        Assert.Equal("Needs attention", result.Rating);
    }

    [Fact]
    public void Risk_AllEquity_IsVeryHigh_AllDebt_IsLow()
    {
        var equityHeavy = new ScoringInput(30, Alloc(100, 0, 0, 0), _benchmark.RecommendedAllocation(30),
            1, true, false, false, 0, 0);
        var debtHeavy = new ScoringInput(30, Alloc(0, 100, 0, 0), _benchmark.RecommendedAllocation(30),
            1, true, false, false, 0, 0);

        RiskScoreResult high = _engine.ComputeRisk(equityHeavy);
        RiskScoreResult low = _engine.ComputeRisk(debtHeavy);

        Assert.Equal("Very High", high.Band);
        Assert.True(high.Score > low.Score);
        Assert.Equal("Low", low.Band);
    }

    [Fact]
    public void Risk_NoAssets_IsNoData()
    {
        var input = new ScoringInput(30, Alloc(0, 0, 0, 0), _benchmark.RecommendedAllocation(30),
            0, HasAssets: false, false, false, 0, 0);
        Assert.Equal("No data", _engine.ComputeRisk(input).Band);
    }
}
