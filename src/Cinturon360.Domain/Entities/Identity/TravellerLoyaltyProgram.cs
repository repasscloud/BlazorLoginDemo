using Cinturon360.Domain.Common.Base;

namespace Cinturon360.Domain.Entities.Identity;

/// <summary>
/// A loyalty program membership for a traveller (e.g. Qantas FF, Marriott Bonvoy).
/// </summary>
public sealed class TravellerLoyaltyProgram : Entity
{
    public string UserId { get; private set; } = string.Empty;
    public string ProgramCode { get; private set; } = string.Empty;
    public string ProgramName { get; private set; } = string.Empty;
    public string MembershipNumber { get; private set; } = string.Empty;
    public string? TierName { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }

    private TravellerLoyaltyProgram() { }

    public static TravellerLoyaltyProgram Create(
        string id,
        string userId,
        string programCode,
        string programName,
        string membershipNumber,
        string? tierName = null,
        DateOnly? expiryDate = null)
        => new()
        {
            Id = id,
            UserId = userId,
            ProgramCode = programCode,
            ProgramName = programName,
            MembershipNumber = membershipNumber,
            TierName = tierName,
            ExpiryDate = expiryDate,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void Update(string programName, string membershipNumber, string? tierName, DateOnly? expiryDate)
    {
        ProgramName = programName;
        MembershipNumber = membershipNumber;
        TierName = tierName;
        ExpiryDate = expiryDate;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
