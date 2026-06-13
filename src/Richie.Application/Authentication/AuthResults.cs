namespace Richie.Application.Authentication;

public enum SignupStatus { Success, UsernameTaken, ValidationFailed }

public sealed record SignupResult(SignupStatus Status, string? Error = null)
{
    public bool Succeeded => Status == SignupStatus.Success;
}

public enum LoginStatus { Success, InvalidCredentials, LockedOut }

public sealed record LoginResult(
    LoginStatus Status,
    Guid? UserId = null,
    string? FullName = null,
    DateTime? LockoutEndUtc = null)
{
    public bool Succeeded => Status == LoginStatus.Success;
}

public enum PasswordResetStatus { Success, UserNotFound, IncorrectAnswers, LockedOut }

public sealed record PasswordResetResult(PasswordResetStatus Status, DateTime? LockoutEndUtc = null)
{
    public bool Succeeded => Status == PasswordResetStatus.Success;
}

public enum ChangePasswordStatus { Success, IncorrectCurrentPassword, ValidationFailed }

public sealed record ChangePasswordResult(ChangePasswordStatus Status, string? Error = null)
{
    public bool Succeeded => Status == ChangePasswordStatus.Success;
}
