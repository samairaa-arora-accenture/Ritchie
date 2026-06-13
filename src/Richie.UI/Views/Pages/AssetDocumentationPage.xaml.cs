using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Richie.UI.ViewModels;
using Richie.UI.Views.Assets;

namespace Richie.UI.Views.Pages;

public partial class AssetDocumentationPage : Page
{
    public AssetDocumentationPage()
    {
        InitializeComponent();
        DataContext = ((App)System.Windows.Application.Current).Services
            .GetRequiredService<AssetDocumentationViewModel>();
    }

    private AssetDocumentationViewModel Vm => (AssetDocumentationViewModel)DataContext;

    private void OnAddAsset(object sender, RoutedEventArgs e) => OpenEditor(null);

    private void OnEditAsset(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: Guid id })
            OpenEditor(id);
    }

    private void OnDeleteAsset(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: Guid id })
            return;

        MessageBoxResult confirm = MessageBox.Show(
            "Delete this asset? This cannot be undone.", "Confirm delete",
            MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirm == MessageBoxResult.Yes)
            Vm.Delete(id);
    }

    private void OpenEditor(Guid? assetId)
    {
        var window = ((App)System.Windows.Application.Current).Services.GetRequiredService<AddEditAssetWindow>();
        window.Owner = Window.GetWindow(this);
        window.Editor.Initialize(assetId);

        if (window.ShowDialog() == true)
            Vm.Refresh();
    }
}
