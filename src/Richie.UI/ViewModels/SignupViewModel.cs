using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Richie.Application.Authentication;
using Richie.Domain.Authentication;
using Richie.UI.Resources;
using Richie.UI.Services;

namespace Richie.UI.ViewModels;

public partial class SignupViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly IAuthNavigation _nav;

    [ObservableProperty] private string _fullName = string.Empty;
    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;
    [ObservableProperty] private string _ageText = string.Empty;
    [ObservableProperty] private string _city = string.Empty;

    [ObservableProperty] private SecurityQuestion _question1 = SecurityQuestion.MothersMaidenName;
    [ObservableProperty] private SecurityQuestion _question2 = SecurityQuestion.CityOfBirth;
    [ObservableProperty] private SecurityQuestion _question3 = SecurityQuestion.FirstSchoolName;
    [ObservableProperty] private string _answer1 = string.Empty;
    [ObservableProperty] private string _answer2 = string.Empty;
    [ObservableProperty] private string _answer3 = string.Empty;

    [ObservableProperty] private string? _error;

    public IReadOnlyList<SecurityQuestionText.Option> Questions => SecurityQuestionText.AllOptions;

    public SignupViewModel(IAuthService auth, IAuthNavigation nav)
    {
        _auth = auth;
        _nav = nav;
    }

    [RelayCommand]
    private void Signup()
    {
        Error = null;

        if (Password != ConfirmPassword)
        {
            Error = "Passwords do not match.";
            return;
        }

        if (!int.TryParse(AgeText, out int age))
        {
            Error = "Enter a valid age.";
            return;
        }

        var request = new SignupRequest(FullName, Username, Password, age, City,
        [
            new SecurityAnswerInput(Question1, Answer1),
            new SecurityAnswerInput(Question2, Answer2),
            new SecurityAnswerInput(Question3, Answer3),
        ]);

        SignupResult result = _auth.Signup(request);
        if (result.Succeeded)
            _nav.ShowLogin();
        else
            Error = result.Error;
    }

    [RelayCommand]
    private void BackToLogin() => _nav.ShowLogin();
}
