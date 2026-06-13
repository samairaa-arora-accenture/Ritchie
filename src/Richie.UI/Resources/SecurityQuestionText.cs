using Richie.Domain.Authentication;

namespace Richie.UI.Resources;

/// <summary>User-facing wording for each <see cref="SecurityQuestion"/> (PRD §3.3).</summary>
public static class SecurityQuestionText
{
    public static string For(SecurityQuestion question) => question switch
    {
        SecurityQuestion.MothersMaidenName => "What is your mother's maiden name?",
        SecurityQuestion.CityOfBirth => "What city were you born in?",
        SecurityQuestion.FirstSchoolName => "What was the name of your first school?",
        SecurityQuestion.FavouriteFood => "What is your favourite food?",
        SecurityQuestion.ChildhoodPetName => "What was the name of your childhood pet?",
        SecurityQuestion.FathersMiddleName => "What is your father's middle name?",
        _ => question.ToString()
    };

    public sealed record Option(SecurityQuestion Value, string Text);

    public static IReadOnlyList<Option> AllOptions { get; } =
        Enum.GetValues<SecurityQuestion>().Select(q => new Option(q, For(q))).ToList();
}
