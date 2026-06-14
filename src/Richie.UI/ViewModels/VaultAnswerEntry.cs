using CommunityToolkit.Mvvm.ComponentModel;
using Richie.Domain.Authentication;
using Richie.UI.Resources;

namespace Richie.UI.ViewModels;

/// <summary>One security-question answer field, used by the recovery setup and forgot-password flows.</summary>
public partial class VaultAnswerEntry : ObservableObject
{
    public SecurityQuestion Question { get; }
    public string QuestionText { get; }

    [ObservableProperty] private string _answer = string.Empty;

    public VaultAnswerEntry(SecurityQuestion question)
    {
        Question = question;
        QuestionText = SecurityQuestionText.For(question);
    }
}
