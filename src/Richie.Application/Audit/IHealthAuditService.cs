using Richie.Domain.Audit;

namespace Richie.Application.Audit;

public enum BenchmarkStatus { Under, OnTarget, Over }

/// <summary>One asset class compared to its age-based benchmark.</summary>
public sealed record BenchmarkRow(
    string ClassName, decimal ActualPercent, decimal RecommendedPercent, BenchmarkStatus Status);

public sealed record GoalProgressRow(string Name, decimal PercentComplete);

/// <summary>
/// The Financial Health Audit (PRD §9.1): health + risk scores (with factor breakdowns), age-based
/// benchmark comparison, diversification, goal progress, coverage gaps and actionable suggestions.
/// <see cref="ScoresAreInterim"/> drives the "pending team finalization" UI flag (§22 open items).
/// </summary>
public sealed record HealthAuditReport(
    bool HasAssets,
    int HealthScore,
    string HealthRating,
    IReadOnlyList<ScoreFactor> HealthFactors,
    int RiskScore,
    string RiskBand,
    string RiskInterpretation,
    string AgeBandName,
    IReadOnlyList<BenchmarkRow> Benchmark,
    IReadOnlyList<string> MissingClasses,
    int DistinctClassCount,
    IReadOnlyList<GoalProgressRow> Goals,
    IReadOnlyList<string> CoverageGaps,
    IReadOnlyList<string> Suggestions,
    bool ScoresAreInterim);

public interface IHealthAuditService
{
    HealthAuditReport GetReport();
}
