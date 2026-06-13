using System.Windows.Controls;
using Richie.UI.ViewModels;

namespace Richie.UI.Views.Auth;

public partial class ForgotPasswordPage : Page
{
    public ForgotPasswordPage(ForgotPasswordViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
