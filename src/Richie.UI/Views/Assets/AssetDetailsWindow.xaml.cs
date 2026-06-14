using Richie.UI.ViewModels;
using Wpf.Ui.Controls;

namespace Richie.UI.Views.Assets;

public partial class AssetDetailsWindow : FluentWindow
{
    public AssetDetailsViewModel Details { get; }

    public AssetDetailsWindow(AssetDetailsViewModel details)
    {
        InitializeComponent();
        Details = details;
        DataContext = details;
        details.CloseRequested += OnCloseRequested;
        Closed += (_, _) => details.CloseRequested -= OnCloseRequested;
    }

    private void OnCloseRequested() => Close();
}
