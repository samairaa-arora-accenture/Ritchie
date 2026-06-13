using System.Windows;
using Richie.UI.Views.Pages;
using Wpf.Ui.Controls;

namespace Richie.UI;

/// <summary>
/// The application shell: a Fluent (Mica) window hosting the primary NavigationView.
/// </summary>
public partial class MainWindow : FluentWindow
{
    /// <summary>Raised when the user clicks Log out; App returns to the auth window.</summary>
    public event EventHandler? LogoutRequested;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => RootNavigation.Navigate(typeof(DashboardPage));
    }

    private void OnLogoutClick(object sender, RoutedEventArgs e) =>
        LogoutRequested?.Invoke(this, EventArgs.Empty);
}
