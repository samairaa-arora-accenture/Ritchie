using Richie.Domain.Insurance;

namespace Richie.Application.Insurance;

/// <summary>Fields for creating/editing a policy (PRD §10).</summary>
public sealed record InsurancePolicyInput(
    InsuranceType Type,
    string PolicyName,
    string? PolicyNumber,
    string? Provider,
    decimal CoverageAmount,
    decimal AnnualPremium,
    DateTime StartDate,
    DateTime RenewalDate,
    string? Nominee,
    string? Notes);

/// <summary>List/detail row for a policy.</summary>
public sealed record InsurancePolicySummary(
    Guid Id,
    InsuranceType Type,
    string TypeName,
    string PolicyName,
    string? Provider,
    decimal CoverageAmount,
    decimal AnnualPremium,
    DateTime RenewalDate,
    int DaysToRenewal);

/// <summary>Insurance CRUD (user-scoped, audited) plus renewal-reminder processing. Insurance is a
/// standalone module and is never joined into asset-allocation queries (CLAUDE.md / PRD §10).</summary>
public interface IInsuranceService
{
    IReadOnlyList<InsurancePolicySummary> GetPolicies();

    InsurancePolicyInput? GetById(Guid id);

    Guid Create(InsurancePolicyInput input);

    bool Update(Guid id, InsurancePolicyInput input);

    bool Delete(Guid id);

    /// <summary>Global catch-up: raise an InsuranceRenewal notification for any policy whose renewal
    /// is within the lead window and not yet notified for that date. Returns the count raised.</summary>
    int ProcessDueRenewals(DateTime nowUtc);
}
