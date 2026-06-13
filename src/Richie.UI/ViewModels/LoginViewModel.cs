using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Richie.Application.Authentication;
using Richie.UI.Services;

namespace Richie.UI.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly IUserSession _session;
    private readonly IAuthNavigation _nav;

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string? _error;

    public LoginViewModel(IAuthService auth, IUserSession session, IAuthNavigation nav)
    {
        _auth = auth;
        _session = session;
        _nav = nav;
    }

    [RelayCommand]
    private void Login()
    {
        Error = null;
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrEmpty(Password))
        {
            Error = "Enter your username and password.";
            return;
        }

        LoginResult result = _auth.Login(Username, Password);
        switch (result.Status)
        {
            case LoginStatus.Success:
                string name = result.FullName ?? Username;
                _session.SignIn(result.UserId!.Value, name);
                _nav.NotifyAuthenticated(result.UserId!.Value, name, result.IsFirstLogin);
                break;
            case LoginStatus.LockedOut:
                Error = $"Account locked due to failed attempts. Try again after {result.LockoutEndUtc?.ToLocalTime():t}.";
                break;
            default:
                Error = "Invalid username or password.";
                break;
        }
    }

    [RelayCommand]
    private void GoToSignup() => _nav.ShowSignup();

    [RelayCommand]
    private void GoToForgotPassword() => _nav.ShowForgotPassword();
}
