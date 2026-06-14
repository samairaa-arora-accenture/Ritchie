using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Richie.UI.Services;
using Richie.UI.ViewModels;
using Richie.UI.Views.Profile;

namespace Richie.UI.Views.Pages;

public partial class ProfilePage : Page
{
    public ProfilePage()
    {
        InitializeComponent();
        DataContext = ((App)System.Windows.Application.Current).Services.GetRequiredService<ProfileViewModel>();
    }

    private ProfileViewModel Vm => (ProfileViewModel)DataContext;

    private void OnLoaded(object sender, RoutedEventArgs e) => Vm.Load();

    private void OnSave(object sender, RoutedEventArgs e)
    {
        if (Vm.Save())
            ((App)System.Windows.Application.Current).Services.GetRequiredService<ToastService>().Success("Profile saved.");
    }

    private void OnChangePassword(object sender, RoutedEventArgs e)
    {
        var window = ((App)System.Windows.Application.Current).Services.GetRequiredService<ChangePasswordWindow>();
        window.Owner = Window.GetWindow(this);
        if (window.ShowDialog() == true)
            ((App)System.Windows.Application.Current).Services.GetRequiredService<ToastService>().Success("Password changed.");
    }
}
