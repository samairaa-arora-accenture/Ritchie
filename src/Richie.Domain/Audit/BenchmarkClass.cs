using Richie.Domain.Assets;

namespace Richie.Domain.Audit;

/// <summary>Broad asset classes used for age-based benchmarking (a coarser grouping than the 7
/// <see cref="AssetType"/>s). Every class is named — no catch-all bucket.</summary>
public enum BenchmarkClass
{
    Equity = 1,
    Debt = 2,
    Gold = 3,
    RealEstate = 4
}

/// <summary>Maps each <see cref="AssetType"/> to its broad <see cref="BenchmarkClass"/>.
/// INTERIM grouping (pending team finalization of benchmark methodology, §22).</summary>
public static class AssetClassMap
{
    public static BenchmarkClass For(AssetType type) => type switch
    {
        AssetType.MutualFund => BenchmarkClass.Equity,
        AssetType.Equity => BenchmarkClass.Equity,
        AssetType.SovereignGoldBond => BenchmarkClass.Gold,
        AssetType.DigitalGold => BenchmarkClass.Gold,
        AssetType.GoldJewellery => BenchmarkClass.Gold,
        AssetType.RealEstate => BenchmarkClass.RealEstate,
        AssetType.GuaranteedInvestmentPlan => BenchmarkClass.Debt,
        _ => BenchmarkClass.Equity
    };

    public static string Display(BenchmarkClass cls) => cls switch
    {
        BenchmarkClass.Equity => "Equity",
        BenchmarkClass.Debt => "Debt",
        BenchmarkClass.Gold => "Gold",
        BenchmarkClass.RealEstate => "Real Estate",
        _ => cls.ToString()
    };

    public static IReadOnlyList<BenchmarkClass> All { get; } =
        [BenchmarkClass.Equity, BenchmarkClass.Debt, BenchmarkClass.Gold, BenchmarkClass.RealEstate];
}
