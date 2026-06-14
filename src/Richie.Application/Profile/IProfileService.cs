namespace Richie.Application.Profile;

public sealed record ProfileData(
    string FullName, string Username, int Age, string City,
    int SecurityScore, string SecurityScoreNote, long StorageBytes);

public sealed record ProfileUpdate(string FullName, int Age, string City);

/// <summary>Read/update the signed-in user's profile, plus a derived security score and storage usage (PRD §14).</summary>
public interface IProfileService
{
    ProfileData Get();
    bool Update(ProfileUpdate update);
}
