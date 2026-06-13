using System.Windows.Controls;
using Richie.UI.ViewModels;

namespace Richie.UI.Views.Auth;

public partial class SignupPage : Page
{
    public SignupPage(SignupViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
