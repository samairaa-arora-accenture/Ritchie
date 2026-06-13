using Microsoft.EntityFrameworkCore;
using Richie.Application.Abstractions;
using Richie.Application.Authentication;
using Richie.Application.Security;
using Richie.Domain.Authentication;
using Richie.Infrastructure.Persistence;

namespace Richie.Infrastructure.Authentication;

public sealed class AuthService : IAuthService
{
    // Lockout policy. Made configurable via Settings in a later phase (PRD §15).
    private const int MaxFailedLoginAttempts = 5;
    private const int MaxFailedResetAttempts = 3;
    private const int MinPasswordLength = 8;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(5);

    private readonly IAppDbContextFactory _factory;
    private readonly IPasswordHasher _hasher;
    private readonly IClock _clock;

    public AuthService(IAppDbContextFactory factory, IPasswordHasher hasher, IClock clock)
    {
        _factory = factory;
        _hasher = hasher;
        _clock = clock;
    }

    public bool AnyUserExists()
    {
        using RichieDbContext db = _factory.Create();
        return db.Users.Any();
    }

    public SignupResult Signup(SignupRequest request)
    {
        string? validationError = Validate(request);
        if (validationError is not null)
            return new SignupResult(SignupStatus.ValidationFailed, validationError);

        string username = Normalize(request.Username);

        using RichieDbContext db = _factory.Create();
        if (db.Users.Any(u => u.Username == username))
            return new SignupResult(SignupStatus.UsernameTaken, "That username is already in use.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Username = username,
            PasswordHash = _hasher.Hash(request.Password),
            Age = request.Age,
            City = request.City.Trim(),
            CreatedUtc = _clock.UtcNow,
            SecurityAnswers = request.SecurityAnswers
                .Select(a => new SecurityAnswer
                {
                    Id = Guid.NewGuid(),
                    Question = a.Question,
                    AnswerHash = _hasher.Hash(NormalizeAnswer(a.Answer))
                })
                .ToList()
        };

        db.Users.Add(user);
        db.SaveChanges();
        return new SignupResult(SignupStatus.Success);
    }

    public LoginResult Login(string username, string password)
    {
        using RichieDbContext db = _factory.Create();
        User? user = db.Users.FirstOrDefault(u => u.Username == Normalize(username));
        if (user is null)
            return new LoginResult(LoginStatus.InvalidCredentials);

        if (IsLockedOut(user, out DateTime lockoutEnd))
            return new LoginResult(LoginStatus.LockedOut, LockoutEndUtc: lockoutEnd);

        if (!_hasher.Verify(password, user.PasswordHash))
        {
            RegisterFailure(user, MaxFailedLoginAttempts);
            db.SaveChanges();
            return IsLockedOut(user, out DateTime end)
                ? new LoginResult(LoginStatus.LockedOut, LockoutEndUtc: end)
                : new LoginResult(LoginStatus.InvalidCredentials);
        }

        user.FailedLoginAttempts = 0;
        user.LockoutEndUtc = null;
        user.LastLoginUtc = _clock.UtcNow;
        db.SaveChanges();
        return new LoginResult(LoginStatus.Success, UserId: user.Id, FullName: user.FullName);
    }

    public IReadOnlyList<SecurityQuestion> GetSecurityQuestions(string username)
    {
        using RichieDbContext db = _factory.Create();
        User? user = db.Users
            .Include(u => u.SecurityAnswers)
            .FirstOrDefault(u => u.Username == Normalize(username));

        return user?.SecurityAnswers.Select(a => a.Question).ToList() ?? [];
    }

    public PasswordResetResult ResetPassword(
        string username,
        IReadOnlyList<SecurityAnswerInput> answers,
        string newPassword)
    {
        using RichieDbContext db = _factory.Create();
        User? user = db.Users
            .Include(u => u.SecurityAnswers)
            .FirstOrDefault(u => u.Username == Normalize(username));

        if (user is null)
            return new PasswordResetResult(PasswordResetStatus.UserNotFound);

        if (IsLockedOut(user, out DateTime lockoutEnd))
            return new PasswordResetResult(PasswordResetStatus.LockedOut, lockoutEnd);

        if (newPassword.Length < MinPasswordLength)
            return new PasswordResetResult(PasswordResetStatus.IncorrectAnswers);

        if (!AnswersMatch(user, answers))
        {
            RegisterFailure(user, MaxFailedResetAttempts);
            db.SaveChanges();
            return IsLockedOut(user, out DateTime end)
                ? new PasswordResetResult(PasswordResetStatus.LockedOut, end)
                : new PasswordResetResult(PasswordResetStatus.IncorrectAnswers);
        }

        user.PasswordHash = _hasher.Hash(newPassword);
        user.FailedLoginAttempts = 0;
        user.LockoutEndUtc = null;
        db.SaveChanges();
        return new PasswordResetResult(PasswordResetStatus.Success);
    }

    public ChangePasswordResult ChangePassword(Guid userId, string currentPassword, string newPassword)
    {
        using RichieDbContext db = _factory.Create();
        User? user = db.Users.FirstOrDefault(u => u.Id == userId);
        if (user is null)
            return new ChangePasswordResult(ChangePasswordStatus.ValidationFailed, "User not found.");

        if (!_hasher.Verify(currentPassword, user.PasswordHash))
            return new ChangePasswordResult(ChangePasswordStatus.IncorrectCurrentPassword);

        if (newPassword.Length < MinPasswordLength)
            return new ChangePasswordResult(ChangePasswordStatus.ValidationFailed,
                $"Password must be at least {MinPasswordLength} characters.");

        user.PasswordHash = _hasher.Hash(newPassword);
        db.SaveChanges();
        return new ChangePasswordResult(ChangePasswordStatus.Success);
    }

    private bool AnswersMatch(User user, IReadOnlyList<SecurityAnswerInput> answers)
    {
        if (answers.Count != user.SecurityAnswers.Count)
            return false;

        return user.SecurityAnswers.All(stored =>
        {
            SecurityAnswerInput? provided = answers.FirstOrDefault(a => a.Question == stored.Question);
            return provided is not null
                && _hasher.Verify(NormalizeAnswer(provided.Answer), stored.AnswerHash);
        });
    }

    private bool IsLockedOut(User user, out DateTime lockoutEnd)
    {
        lockoutEnd = user.LockoutEndUtc ?? default;
        return user.LockoutEndUtc is { } end && end > _clock.UtcNow;
    }

    private void RegisterFailure(User user, int threshold)
    {
        user.FailedLoginAttempts++;
        if (user.FailedLoginAttempts >= threshold)
        {
            user.LockoutEndUtc = _clock.UtcNow + LockoutDuration;
            user.FailedLoginAttempts = 0;
        }
    }

    private string? Validate(SignupRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.FullName))
            return "Full name is required.";
        if (string.IsNullOrWhiteSpace(r.Username))
            return "Username is required.";
        if (r.Password.Length < MinPasswordLength)
            return $"Password must be at least {MinPasswordLength} characters.";
        if (r.Age is < 1 or > 120)
            return "Please enter a valid age.";
        if (r.SecurityAnswers.Count != 3)
            return "Exactly three security questions are required.";
        if (r.SecurityAnswers.Select(a => a.Question).Distinct().Count() != 3)
            return "Please choose three different security questions.";
        if (r.SecurityAnswers.Any(a => string.IsNullOrWhiteSpace(a.Answer)))
            return "All security answers are required.";
        return null;
    }

    private static string Normalize(string username) => username.Trim().ToLowerInvariant();

    private static string NormalizeAnswer(string answer) => answer.Trim().ToLowerInvariant();
}
