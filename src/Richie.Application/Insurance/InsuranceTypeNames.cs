using Richie.Domain.Insurance;

namespace Richie.Application.Insurance;

/// <summary>Explicit display names for each insurance type (every type named — no generic bucket).</summary>
public static class InsuranceTypeNames
{
    public static string Display(InsuranceType type) => type switch
    {
        InsuranceType.Health => "Health",
        InsuranceType.TermLife => "Term Life",
        InsuranceType.Vehicle => "Vehicle",
        InsuranceType.Home => "Home",
        InsuranceType.Other => "Other",
        _ => type.ToString()
    };
}
