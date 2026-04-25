using Cinturon360.Domain.Common.Base;

namespace Cinturon360.Domain.Entities.Identity;

/// <summary>
/// Travel-specific profile for a user. Linked 1:1 to a User.
/// Stores passport, DOB, nationality, and travel preferences.
/// </summary>
public sealed class TravellerProfile : Entity
{
    public string UserId { get; private set; } = string.Empty;

    // Personal / passport info
    public string? PassportNumber { get; private set; }
    public string? PassportCountry { get; private set; }
    public DateOnly? PassportExpiry { get; private set; }
    public string? Nationality { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public string? Gender { get; private set; }

    // Known traveller numbers
    public string? TsaPreCheckNumber { get; private set; }
    public string? GlobalEntryNumber { get; private set; }
    public string? RedressNumber { get; private set; }

    // Preferences
    public string? PreferredSeatType { get; private set; }
    public string? PreferredMealType { get; private set; }

    // Service tier
    public string? VipLevel { get; private set; }
    public string? DedicatedConsultantUserId { get; private set; }

    private TravellerProfile() { }

    public static TravellerProfile Create(string id, string userId)
        => new() { Id = id, UserId = userId, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };

    public void UpdatePassport(string? number, string? country, DateOnly? expiry, string? nationality)
    {
        PassportNumber = number;
        PassportCountry = country;
        PassportExpiry = expiry;
        Nationality = nationality;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdatePersonal(DateOnly? dob, string? gender)
    {
        DateOfBirth = dob;
        Gender = gender;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateKnownTraveller(string? tsaPreCheck, string? globalEntry, string? redress)
    {
        TsaPreCheckNumber = tsaPreCheck;
        GlobalEntryNumber = globalEntry;
        RedressNumber = redress;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdatePreferences(string? seatType, string? mealType)
    {
        PreferredSeatType = seatType;
        PreferredMealType = mealType;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetServiceTier(string? vipLevel, string? consultantUserId)
    {
        VipLevel = vipLevel;
        DedicatedConsultantUserId = consultantUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
