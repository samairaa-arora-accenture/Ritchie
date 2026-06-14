using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Richie.UI.ViewModels;
using Wpf.Ui.Controls;

namespace Richie.UI.Views.Insurance;

public partial class InsuranceWindow : FluentWindow
{
    private readonly InsuranceViewModel _vm;

    public InsuranceWindow(InsuranceViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;
    }

    private void OnAddPolicy(object sender, RoutedEventArgs e) => OpenEditor(null);

    private void OnEditPolicy(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: Guid id })
            OpenEditor(id);
    }

    private void OnDeletePolicy(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: Guid id })
            return;

        if (System.Windows.MessageBox.Show("Delete this policy?", "Confirm delete",
                System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning)
            == System.Windows.MessageBoxResult.Yes)
            _vm.Delete(id);
    }

    private void OpenEditor(Guid? id)
    {
        var window = ((App)System.Windows.Application.Current).Services
            .GetRequiredService<AddEditInsuranceWindow>();
        window.Owner = this;
        window.Editor.Initialize(id);
        if (window.ShowDialog() == true)
            _vm.Refresh();
    }
}
