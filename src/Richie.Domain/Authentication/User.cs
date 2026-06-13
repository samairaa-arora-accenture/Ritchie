namespace Richie.Domain.Authentication;

/// <summary>
/// A local user account. The password and security answers are stored only as hashes.
/// Username is stored normalised (lower-cased) so lookups are case-insensitive.
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // Demographics — drive age-based benchmarking elsewhere in the app.
    public int Age { get; set; }
    public string City { get; set; } = string.Empty;

    // Login / lockout tracking.
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
    public DateTime? LastLoginUtc { get; set; }
    public DateTime CreatedUtc { get; set; }

    public List<SecurityAnswer> SecurityAnswers { get; set; } = [];
}
