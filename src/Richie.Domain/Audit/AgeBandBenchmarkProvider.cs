namespace Richie.Domain.Audit;

/// <summary>
/// INTERIM age-band benchmark allocations (PRD §18.2 / §22 — "team to finalise"). The directional
/// shape follows the PRD (younger → more equity; older → more debt/stable), with indicative numbers
/// chosen here as a transparent placeholder. <see cref="IsInterim"/> is true so the UI flags it.
/// Swap these tables (or this whole provider) when the team finalizes the methodology.
/// </summary>
public sealed class AgeBandBenchmarkProvider : IBenchmarkProvider
{
    public decimal TolerancePoints => 10m;
    public bool IsInterim => true;

    public AgeBand BandFor(int age) => age <= 35 ? AgeBand.Young : age <= 50 ? AgeBand.MidCareer : AgeBand.PreRetirement;

    public string BandName(AgeBand band) => band switch
    {
        AgeBand.Young => "Young (≤35)",
        AgeBand.MidCareer => "Mid-career (36–50)",
        AgeBand.PreRetirement => "Pre-retirement (51+)",
        _ => band.ToString()
    };

    public IReadOnlyDictionary<BenchmarkClass, decimal> RecommendedAllocation(int age) => BandFor(age) switch
    {
        AgeBand.Young => Allocation(60, 15, 10, 15),
        AgeBand.MidCareer => Allocation(45, 30, 10, 15),
        _ => Allocation(25, 50, 10, 15)
    };

    private static Dictionary<BenchmarkClass, decimal> Allocation(decimal equity, decimal debt, decimal gold, decimal realEstate) =>
        new()
        {
            [BenchmarkClass.Equity] = equity,
            [BenchmarkClass.Debt] = debt,
            [BenchmarkClass.Gold] = gold,
            [BenchmarkClass.RealEstate] = realEstate
        };
}
