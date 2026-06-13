using Richie.Application.Authentication;

namespace Richie.Infrastructure.Authentication;

/// <summary>In-memory holder for the signed-in user (singleton for the app lifetime).</summary>
public sealed class UserSession : IUserSession
{
    public Guid? UserId { get; private set; }
    public string? FullName { get; private set; }
    public bool IsAuthenticated => UserId is not null;

    public void SignIn(Guid userId, string fullName)
    {
        UserId = userId;
        FullName = fullName;
    }

    public void SignOut()
    {
        UserId = null;
        FullName = null;
    }
}
