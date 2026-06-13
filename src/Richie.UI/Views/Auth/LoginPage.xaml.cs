using System.Windows.Controls;
using Richie.UI.ViewModels;

namespace Richie.UI.Views.Auth;

public partial class LoginPage : Page
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
