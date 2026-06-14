using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Richie.Application.Profile;

namespace Richie.UI.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly IProfileService _profile;

    [ObservableProperty] private string _fullName = string.Empty;
    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _ageText = string.Empty;
    [ObservableProperty] private string _city = string.Empty;
    [ObservableProperty] private int _securityScore;
    [ObservableProperty] private string _securityScoreNote = string.Empty;
    [ObservableProperty] private string _storageText = string.Empty;
    [ObservableProperty] private string? _error;

    public ProfileViewModel(IProfileService profile) => _profile = profile;

    public void Load()
    {
        ProfileData data = _profile.Get();
        FullName = data.FullName;
        Username = data.Username;
        AgeText = data.Age.ToString(CultureInfo.CurrentCulture);
        City = data.City;
        SecurityScore = data.SecurityScore;
        SecurityScoreNote = data.SecurityScoreNote;
        StorageText = FormatBytes(data.StorageBytes);
    }

    public bool Save()
    {
        Error = null;
        if (!int.TryParse(AgeText, out int age) || age is < 1 or > 120)
        {
            Error = "Enter a valid age.";
            return false;
        }
        if (!_profile.Update(new ProfileUpdate(FullName, age, City)))
        {
            Error = "Could not save the profile.";
            return false;
        }
        Load();
        return true;
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["bytes", "KB", "MB", "GB"];
        double size = bytes;
        int unit = 0;
        while (size >= 1024 && unit < units.Length - 1) { size /= 1024; unit++; }
        return $"{size:0.#} {units[unit]}";
    }
}
