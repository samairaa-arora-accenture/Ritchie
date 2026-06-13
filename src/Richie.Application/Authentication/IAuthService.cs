using Richie.Domain.Authentication;

namespace Richie.Application.Authentication;

/// <summary>
/// Local-account authentication: signup, login (with lockout), and security-question
/// password reset. All passwords and answers are hashed; nothing is stored in plaintext.
/// </summary>
public interface IAuthService
{
    /// <summary>True if at least one account exists (drives first-run signup routing).</summary>
    bool AnyUserExists();

    SignupResult Signup(SignupRequest request);

    LoginResult Login(string username, string password);

    /// <summary>The questions a user chose at signup, or empty if no such user.</summary>
    IReadOnlyList<SecurityQuestion> GetSecurityQuestions(string username);

    PasswordResetResult ResetPassword(
        string username,
        IReadOnlyList<SecurityAnswerInput> answers,
        string newPassword);

    ChangePasswordResult ChangePassword(Guid userId, string currentPassword, string newPassword);
}
