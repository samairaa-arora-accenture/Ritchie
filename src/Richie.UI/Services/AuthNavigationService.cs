namespace Richie.UI.Services;

/// <summary>
/// Singleton bridge between the auth view-models (which request navigation) and the
/// AuthWindow (which performs it). Decouples the page view-models from the window.
/// </summary>
public sealed class AuthNavigationService : IAuthNavigation
{
    public enum AuthPage { Login, Signup, ForgotPassword }

    /// <summary>Set by AuthWindow to perform the actual frame navigation.</summary>
    public Action<AuthPage>? NavigateRequested { get; set; }

    /// <summary>Raised when login succeeds; App swaps to the main shell.</summary>
    public event EventHandler<AuthenticatedEventArgs>? Authenticated;

    public void ShowLogin() => NavigateRequested?.Invoke(AuthPage.Login);
    public void ShowSignup() => NavigateRequested?.Invoke(AuthPage.Signup);
    public void ShowForgotPassword() => NavigateRequested?.Invoke(AuthPage.ForgotPassword);

    public void NotifyAuthenticated(Guid userId, string fullName) =>
        Authenticated?.Invoke(this, new AuthenticatedEventArgs(userId, fullName));
}
