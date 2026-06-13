using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Richie.Application.Authentication;
using Richie.Domain.Authentication;
using Richie.UI.Resources;
using Richie.UI.Services;

namespace Richie.UI.ViewModels;

public partial class ForgotPasswordViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly IAuthNavigation _nav;

    private IReadOnlyList<SecurityQuestion> _loadedQuestions = [];

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private bool _questionsLoaded;

    [ObservableProperty] private string _question1Text = string.Empty;
    [ObservableProperty] private string _question2Text = string.Empty;
    [ObservableProperty] private string _question3Text = string.Empty;
    [ObservableProperty] private string _answer1 = string.Empty;
    [ObservableProperty] private string _answer2 = string.Empty;
    [ObservableProperty] private string _answer3 = string.Empty;

    [ObservableProperty] private string _newPassword = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;
    [ObservableProperty] private string? _error;

    public ForgotPasswordViewModel(IAuthService auth, IAuthNavigation nav)
    {
        _auth = auth;
        _nav = nav;
    }

    [RelayCommand]
    private void LoadQuestions()
    {
        Error = null;
        _loadedQuestions = _auth.GetSecurityQuestions(Username);
        if (_loadedQuestions.Count != 3)
        {
            Error = "No account found for that username.";
            QuestionsLoaded = false;
            return;
        }

        Question1Text = SecurityQuestionText.For(_loadedQuestions[0]);
        Question2Text = SecurityQuestionText.For(_loadedQuestions[1]);
        Question3Text = SecurityQuestionText.For(_loadedQuestions[2]);
        QuestionsLoaded = true;
    }

    [RelayCommand]
    private void Reset()
    {
        Error = null;
        if (NewPassword != ConfirmPassword)
        {
            Error = "Passwords do not match.";
            return;
        }

        PasswordResetResult result = _auth.ResetPassword(Username,
        [
            new SecurityAnswerInput(_loadedQuestions[0], Answer1),
            new SecurityAnswerInput(_loadedQuestions[1], Answer2),
            new SecurityAnswerInput(_loadedQuestions[2], Answer3),
        ], NewPassword);

        switch (result.Status)
        {
            case PasswordResetStatus.Success:
                _nav.ShowLogin();
                break;
            case PasswordResetStatus.LockedOut:
                Error = $"Too many attempts. Try again after {result.LockoutEndUtc?.ToLocalTime():t}.";
                break;
            default:
                Error = "The answers or new password are not valid. Please try again.";
                break;
        }
    }

    [RelayCommand]
    private void BackToLogin() => _nav.ShowLogin();
}
