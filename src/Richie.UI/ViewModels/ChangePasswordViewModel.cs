using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Richie.Application.Authentication;

namespace Richie.UI.ViewModels;

/// <summary>Changes the account login password (distinct from the vault master password).</summary>
public partial class ChangePasswordViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly IUserSession _session;

    [ObservableProperty] private string _currentPassword = string.Empty;
    [ObservableProperty] private string _newPassword = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;
    [ObservableProperty] private string? _error;

    public event Action<bool>? CloseRequested;

    public ChangePasswordViewModel(IAuthService auth, IUserSession session)
    {
        _auth = auth;
        _session = session;
    }

    [RelayCommand]
    private void Save()
    {
        Error = null;
        if (NewPassword != ConfirmPassword)
        {
            Error = "New password and confirmation don't match.";
            return;
        }
        if (_session.UserId is not { } userId)
        {
            Error = "No signed-in user.";
            return;
        }

        ChangePasswordResult result = _auth.ChangePassword(userId, CurrentPassword, NewPassword);
        Error = result.Status switch
        {
            ChangePasswordStatus.Success => null,
            ChangePasswordStatus.IncorrectCurrentPassword => "Current password is incorrect.",
            _ => result.Error ?? "Could not change the password."
        };
        if (result.Succeeded)
            CloseRequested?.Invoke(true);
    }

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(false);
}
