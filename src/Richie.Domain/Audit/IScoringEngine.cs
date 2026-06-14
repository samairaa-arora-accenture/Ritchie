namespace Richie.Domain.Audit;

/// <summary>Inputs for the health/risk scoring, gathered from the user's real data.</summary>
public sealed record ScoringInput(
    int Age,
    IReadOnlyDictionary<BenchmarkClass, decimal> ActualPercents,
    IReadOnlyDictionary<BenchmarkClass, decimal> RecommendedPercents,
    int DistinctClassCount,
    bool HasAssets,
    bool HasHealthInsurance,
    bool HasTermLife,
    int GoalCount,
    decimal AverageGoalProgressPercent);

/// <summary>One contributing factor of a score, with the points it added and a plain explanation.</summary>
public sealed record ScoreFactor(string Name, int Points, int OutOf, string Explanation);

public sealed record HealthScoreResult(int Score, string Rating, IReadOnlyList<ScoreFactor> Factors);

public sealed record RiskScoreResult(int Score, string Band, string Interpretation);

/// <summary>
/// Computes the Portfolio Health Score and Risk Score. The concrete formulas are <b>INTERIM
/// placeholders</b> (PRD §22 — inputs/weights/scales to be finalized by the team); they live behind
/// this interface so they're swappable without touching the rest of the app. Every result ships
/// with its factor breakdown so the logic is visible to the user (PRD §9.1).
/// </summary>
public interface IScoringEngine
{
    /// <summary>True while the formulas are interim placeholders awaiting team sign-off.</summary>
    bool IsInterim { get; }

    HealthScoreResult ComputeHealth(ScoringInput input);
    RiskScoreResult ComputeRisk(ScoringInput input);
}
