namespace Richie.Domain.Audit;

public enum AgeBand
{
    Young = 1,        // ≤ 35
    MidCareer = 2,    // 36–50
    PreRetirement = 3 // 51+
}

/// <summary>Recommended allocation (percent per class) for an age, plus the band and an
/// over/under tolerance. Numbers are <b>INTERIM placeholders</b> (PRD §22 — team to finalize).</summary>
public interface IBenchmarkProvider
{
    AgeBand BandFor(int age);
    string BandName(AgeBand band);

    /// <summary>Target allocation percent per class for the age (sums to 100).</summary>
    IReadOnlyDictionary<BenchmarkClass, decimal> RecommendedAllocation(int age);

    /// <summary>± tolerance (percentage points) before a class is flagged over/under-allocated.</summary>
    decimal TolerancePoints { get; }

    /// <summary>True when these figures are interim placeholders awaiting team sign-off.</summary>
    bool IsInterim { get; }
}
