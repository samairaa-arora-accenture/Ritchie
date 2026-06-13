using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Richie.Application.Assets;

namespace Richie.UI.ViewModels;

public partial class AssetDocumentationViewModel : ObservableObject
{
    private readonly IAssetService _assets;

    [ObservableProperty] private ObservableCollection<AssetSummary> _items = [];
    [ObservableProperty] private ObservableCollection<AllocationSlice> _allocation = [];
    [ObservableProperty] private string _totalCurrentValueText = string.Empty;
    [ObservableProperty] private string _totalInvestedText = string.Empty;
    [ObservableProperty] private string _totalProfitLossText = string.Empty;
    [ObservableProperty] private bool _isEmpty;

    public AssetDocumentationViewModel(IAssetService assets)
    {
        _assets = assets;
        Refresh();
    }

    public void Refresh()
    {
        Items = new ObservableCollection<AssetSummary>(_assets.GetAssets());
        IsEmpty = Items.Count == 0;

        PortfolioSummary summary = _assets.GetPortfolioSummary();
        Allocation = new ObservableCollection<AllocationSlice>(summary.Allocation);
        TotalCurrentValueText = Money(summary.TotalCurrentValue);
        TotalInvestedText = Money(summary.TotalInvested);
        TotalProfitLossText = $"{Money(summary.TotalProfitLoss)} ({summary.TotalProfitLossPercent:+0.0;-0.0;0.0}%)";
    }

    public void Delete(Guid id)
    {
        _assets.Delete(id);
        Refresh();
    }

    private static string Money(decimal value) => value.ToString("N2", CultureInfo.CurrentCulture);
}
