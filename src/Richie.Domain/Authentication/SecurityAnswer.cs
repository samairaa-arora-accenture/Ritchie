namespace Richie.Domain.Authentication;

/// <summary>
/// A user's hashed answer to one chosen security question.
/// </summary>
public class SecurityAnswer
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public SecurityQuestion Question { get; set; }
    public string AnswerHash { get; set; } = string.Empty;
}
