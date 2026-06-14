namespace Richie.Domain.Insurance;

/// <summary>Insurance categories (PRD §10). "Other" is a named type, not a chart catch-all —
/// insurance never appears in asset-allocation charts.</summary>
public enum InsuranceType
{
    Health = 1,
    TermLife = 2,
    Vehicle = 3,
    Home = 4,
    Other = 5
}

/// <summary>
/// An insurance policy (PRD §10). Insurance is its own module — never an investable asset and
/// never part of asset allocation. It feeds the Financial Health Audit only as coverage-gap data.
/// </summary>
public class InsurancePolicy
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public InsuranceType Type { get; set; }
    public string PolicyName { get; set; } = string.Empty;
    public string? PolicyNumber { get; set; }
    public string? Provider { get; set; }
    public decimal CoverageAmount { get; set; }
    public decimal AnnualPremium { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime RenewalDate { get; set; }
    public string? Nominee { get; set; }
    public string? Notes { get; set; }

    /// <summary>The <see cref="RenewalDate"/> a renewal reminder was last raised for — prevents
    /// duplicate alerts and re-arms when the renewal date changes.</summary>
    public DateTime? RenewalNotifiedForDate { get; set; }

    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}
