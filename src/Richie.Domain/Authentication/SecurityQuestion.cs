namespace Richie.Domain.Authentication;

/// <summary>
/// The fixed pool of security questions (PRD §3.3). Users pick three at signup;
/// answers are stored hashed, never in plaintext.
/// </summary>
public enum SecurityQuestion
{
    MothersMaidenName = 1,
    CityOfBirth = 2,
    FirstSchoolName = 3,
    FavouriteFood = 4,
    ChildhoodPetName = 5,
    FathersMiddleName = 6
}
