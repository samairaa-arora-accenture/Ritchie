using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Richie.Application.Insurance;

namespace Richie.UI.ViewModels;

public partial class InsuranceViewModel : ObservableObject
{
    private readonly IInsuranceService _insurance;

    [ObservableProperty] private ObservableCollection<InsurancePolicySummary> _items = [];
    [ObservableProperty] private bool _isEmpty;

    public InsuranceViewModel(IInsuranceService insurance)
    {
        _insurance = insurance;
        Refresh();
    }

    public void Refresh()
    {
        Items = new ObservableCollection<InsurancePolicySummary>(_insurance.GetPolicies());
        IsEmpty = Items.Count == 0;
    }

    public void Delete(Guid id)
    {
        _insurance.Delete(id);
        Refresh();
    }
}
