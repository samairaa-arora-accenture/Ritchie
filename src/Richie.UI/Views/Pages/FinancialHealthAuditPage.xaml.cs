using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Richie.UI.Views.Insurance;

namespace Richie.UI.Views.Pages;

public partial class FinancialHealthAuditPage : Page
{
    public FinancialHealthAuditPage() => InitializeComponent();

    private void OnManageInsurance(object sender, RoutedEventArgs e)
    {
        var window = ((App)System.Windows.Application.Current).Services
            .GetRequiredService<InsuranceWindow>();
        window.Owner = Window.GetWindow(this);
        window.ShowDialog();
    }
}
