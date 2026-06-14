namespace Richie.Domain.Audit;

/// <summary>
/// INTERIM scoring formulas (PRD §22). Transparent and documented so the displayed logic is honest,
/// but NOT the finalized methodology — swap freely when the team confirms inputs/weights/scales.
///
/// <para><b>Health Score (0–100)</b> = weighted average of four 0–100 factors:
/// Diversification 25% (distinct classes ÷ 4), Benchmark alignment 35% (100 − ½·Σ|actual−target|),
/// Protection 20% (health 50 + term-life 50), Goal progress 20% (avg %, or neutral 50 with no goals).
/// Scale: 80–100 Excellent · 60–79 Good · &lt;60 Needs attention.</para>
///
/// <para><b>Risk Score (0–100)</b> = volatility-weighted exposure:
/// Equity·1.0 + RealEstate·0.7 + Gold·0.3 + Debt·0.1. Bands: ≤20 Low · ≤40 Moderate ·
/// ≤60 Moderately High · ≤80 High · &gt;80 Very High.</para>
/// </summary>
public sealed class PlaceholderScoringEngine : IScoringEngine
{
    public bool IsInterim => true;

    public HealthScoreResult ComputeHealth(ScoringInput input)
    {
        if (!input.HasAssets)
        {
            return new HealthScoreResult(0, "No data", new List<ScoreFactor>
            {
                new("Portfolio", 0, 100, "Add assets to compute your health score.")
            });
        }

        // Diversification — how many of the 4 broad classes are present.
        int diversification = (int)Math.Round(Math.Min(input.DistinctClassCount, 4) / 4.0 * 100);

        // Benchmark alignment — closeness of actual allocation to the age-based target.
        decimal totalDeviation = 0;
        foreach (BenchmarkClass cls in AssetClassMap.All)
            totalDeviation += Math.Abs(Pct(input.ActualPercents, cls) - Pct(input.RecommendedPercents, cls));
        int alignment = (int)Math.Round(Math.Max(0, 100 - (double)totalDeviation / 2));

        // Protection — health + term-life cover.
        int protection = (input.HasHealthInsurance ? 50 : 0) + (input.HasTermLife ? 50 : 0);

        // Goals — average progress, or a neutral 50 when none are set.
        int goals = input.GoalCount == 0 ? 50 : (int)Math.Round(Math.Min(100, (double)input.AverageGoalProgressPercent));

        int score = (int)Math.Round(diversification * 0.25 + alignment * 0.35 + protection * 0.20 + goals * 0.20);
        score = Math.Clamp(score, 0, 100);

        var factors = new List<ScoreFactor>
        {
            new("Diversification", diversification, 100,
                $"{input.DistinctClassCount} of 4 broad asset classes represented (weight 25%)."),
            new("Benchmark alignment", alignment, 100,
                $"Allocation is {totalDeviation:0}pp away from your age-based target in total (weight 35%)."),
            new("Protection", protection, 100,
                $"Health insurance: {(input.HasHealthInsurance ? "yes" : "no")}, term life: {(input.HasTermLife ? "yes" : "no")} (weight 20%)."),
            new("Goal progress", goals, 100,
                input.GoalCount == 0 ? "No goals set — neutral score (weight 20%)."
                                     : $"Average goal progress {input.AverageGoalProgressPercent:0}% (weight 20%).")
        };

        return new HealthScoreResult(score, Rate(score), factors);
    }

    public RiskScoreResult ComputeRisk(ScoringInput input)
    {
        if (!input.HasAssets)
            return new RiskScoreResult(0, "No data", "Add assets to compute your risk score.");

        decimal equity = Pct(input.ActualPercents, BenchmarkClass.Equity);
        decimal realEstate = Pct(input.ActualPercents, BenchmarkClass.RealEstate);
        decimal gold = Pct(input.ActualPercents, BenchmarkClass.Gold);
        decimal debt = Pct(input.ActualPercents, BenchmarkClass.Debt);

        int score = (int)Math.Round((double)(equity * 1.0m + realEstate * 0.7m + gold * 0.3m + debt * 0.1m));
        score = Math.Clamp(score, 0, 100);

        string band = score switch
        {
            <= 20 => "Low",
            <= 40 => "Moderate",
            <= 60 => "Moderately High",
            <= 80 => "High",
            _ => "Very High"
        };

        decimal growth = equity + realEstate;
        decimal stable = debt + gold;
        string interpretation =
            $"{growth:0}% sits in growth/volatile assets (equity & real estate) vs {stable:0}% in stabler debt & gold.";

        return new RiskScoreResult(score, band, interpretation);
    }

    private static decimal Pct(IReadOnlyDictionary<BenchmarkClass, decimal> map, BenchmarkClass cls) =>
        map.TryGetValue(cls, out decimal v) ? v : 0m;

    private static string Rate(int score) => score >= 80 ? "Excellent" : score >= 60 ? "Good" : "Needs attention";
}
