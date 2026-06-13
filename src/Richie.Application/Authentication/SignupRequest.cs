using Richie.Domain.Authentication;

namespace Richie.Application.Authentication;

/// <summary>A plaintext answer to one chosen security question (hashed by the service).</summary>
public sealed record SecurityAnswerInput(SecurityQuestion Question, string Answer);

/// <summary>Everything needed to create a new local account.</summary>
public sealed record SignupRequest(
    string FullName,
    string Username,
    string Password,
    int Age,
    string City,
    IReadOnlyList<SecurityAnswerInput> SecurityAnswers);
