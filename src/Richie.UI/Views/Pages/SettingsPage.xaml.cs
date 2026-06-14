using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Richie.UI.Services;
using Richie.UI.ViewModels;
using Richie.UI.Views.Expenses;
using Richie.UI.Views.Vault;

namespace Richie.UI.Views.Pages;

public partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        DataContext = ((App)System.Windows.Application.Current).Services.GetRequiredService<SettingsViewModel>();
    }

    private SettingsViewModel Vm => (SettingsViewModel)DataContext;

    private void OnSave(object sender, RoutedEventArgs e)
    {
        Vm.Save();
        ((App)System.Windows.Application.Current).Services.GetRequiredService<ToastService>().Success("Settings saved.");
    }

    private void OnChangeMasterPassword(object sender, RoutedEventArgs e)
    {
        var window = ((App)System.Windows.Application.Current).Services.GetRequiredService<VaultSecurityWindow>();
        window.Owner = Window.GetWindow(this);
        window.ShowDialog();
    }

    private void OnBudgets(object sender, RoutedEventArgs e)
    {
        var window = ((App)System.Windows.Application.Current).Services.GetRequiredService<ExpenseAnalyticsWindow>();
        window.Owner = Window.GetWindow(this);
        window.ShowDialog();
    }
}
