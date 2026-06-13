namespace Richie.UI.Services;

public sealed class AuthenticatedEventArgs(Guid userId, string fullName, bool isFirstLogin) : EventArgs
{
    public Guid UserId { get; } = userId;
    public string FullName { get; } = fullName;
    public bool IsFirstLogin { get; } = isFirstLogin;
}

/// <summary>
/// Drives navigation between the auth pages (login / signup / forgot-password) and
/// signals when the user has successfully authenticated.
/// </summary>
public interface IAuthNavigation
{
    void ShowLogin();
    void ShowSignup();
    void ShowForgotPassword();
    void NotifyAuthenticated(Guid userId, string fullName, bool isFirstLogin);
}
