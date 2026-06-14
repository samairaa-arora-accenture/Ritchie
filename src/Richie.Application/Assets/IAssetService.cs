using Richie.Domain.Assets;

namespace Richie.Application.Assets;

/// <summary>
/// CRUD and portfolio summaries for the signed-in user's assets. Every write is audit-logged.
/// </summary>
public interface IAssetService
{
    IReadOnlyList<AssetSummary> GetAssets();

    /// <summary>The full asset (for the edit form), or null if not found / not the user's.</summary>
    Asset? GetById(Guid id);

    Guid Create(AssetInput input);
    bool Update(Guid id, AssetInput input);
    bool Delete(Guid id);

    /// <summary>Include/exclude a gold-jewellery asset from portfolio valuation (PRD §6.10).
    /// No effect for other asset types. Returns false if not found / not jewellery.</summary>
    bool SetPortfolioExclusion(Guid id, bool excluded);

    /// <summary>Bulk include/exclude ALL gold-jewellery assets from portfolio valuation, for the
    /// global Settings toggle (PRD §15). Returns the number of jewellery assets updated.</summary>
    int SetAllJewelleryExclusion(bool excluded);

    /// <summary>Totals + allocation; excluded gold jewellery is omitted (PRD §6.10).</summary>
    PortfolioSummary GetPortfolioSummary();
}
