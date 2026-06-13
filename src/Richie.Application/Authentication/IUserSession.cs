namespace Richie.Application.Authentication;

/// <summary>
/// Holds the currently signed-in user for the lifetime of the app session.
/// </summary>
public interface IUserSession
{
    Guid? UserId { get; }
    string? FullName { get; }
    bool IsAuthenticated { get; }

    void SignIn(Guid userId, string fullName);
    void SignOut();
}
