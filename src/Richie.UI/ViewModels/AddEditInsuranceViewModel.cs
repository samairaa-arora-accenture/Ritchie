using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Richie.Application.Insurance;
using Richie.Domain.Insurance;

namespace Richie.UI.ViewModels;

public partial class AddEditInsuranceViewModel : ObservableObject
{
    private readonly IInsuranceService _insurance;
    private Guid? _editId;

    public sealed record TypeOption(InsuranceType Value, string Text);

    public IReadOnlyList<TypeOption> Types { get; } =
        Enum.GetValues<InsuranceType>().Select(t => new TypeOption(t, InsuranceTypeNames.Display(t))).ToList();

    [ObservableProperty] private string _title = "Add policy";
    [ObservableProperty] private InsuranceType _type = InsuranceType.Health;
    [ObservableProperty] private string _policyName = string.Empty;
    [ObservableProperty] private string? _policyNumber;
    [ObservableProperty] private string? _provider;
    [ObservableProperty] private string _coverageAmount = string.Empty;
    [ObservableProperty] private string _annualPremium = string.Empty;
    [ObservableProperty] private DateTime? _startDate = DateTime.Today;
    [ObservableProperty] private DateTime? _renewalDate = DateTime.Today.AddYears(1);
    [ObservableProperty] private string? _nominee;
    [ObservableProperty] private string? _notes;
    [ObservableProperty] private string? _error;

    public event Action<bool>? CloseRequested;

    public AddEditInsuranceViewModel(IInsuranceService insurance) => _insurance = insurance;

    public void Initialize(Guid? id)
    {
        _editId = id;
        if (id is null)
            return;

        InsurancePolicyInput? p = _insurance.GetById(id.Value);
        if (p is null)
            return;

        Title = "Edit policy";
        Type = p.Type;
        PolicyName = p.PolicyName;
        PolicyNumber = p.PolicyNumber;
        Provider = p.Provider;
        CoverageAmount = p.CoverageAmount.ToString(CultureInfo.CurrentCulture);
        AnnualPremium = p.AnnualPremium.ToString(CultureInfo.CurrentCulture);
        StartDate = p.StartDate;
        RenewalDate = p.RenewalDate;
        Nominee = p.Nominee;
        Notes = p.Notes;
    }

    [RelayCommand]
    private void Save()
    {
        Error = null;
        if (string.IsNullOrWhiteSpace(PolicyName))
        {
            Error = "Policy name is required.";
            return;
        }
        if (StartDate is null || RenewalDate is null)
        {
            Error = "Start and renewal dates are required.";
            return;
        }
        if (!TryMoney(CoverageAmount, out decimal coverage))
        {
            Error = "Enter a valid coverage amount.";
            return;
        }
        if (!TryMoney(AnnualPremium, out decimal premium))
        {
            Error = "Enter a valid annual premium.";
            return;
        }

        var input = new InsurancePolicyInput(Type, PolicyName, PolicyNumber, Provider,
            coverage, premium, StartDate.Value, RenewalDate.Value, Nominee, Notes);

        if (_editId is null)
            _insurance.Create(input);
        else
            _insurance.Update(_editId.Value, input);

        CloseRequested?.Invoke(true);
    }

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(false);

    private static bool TryMoney(string text, out decimal value) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out value) && value >= 0;
}
