using Cinturon360.Domain.Common.Base;

namespace Cinturon360.Domain.Entities.Identity;

/// <summary>
/// User-level travel and notification preferences.
/// One row per user. Created on first save; upserted thereafter.
/// </summary>
public sealed class UserPreferences : Entity
{
    public string UserId { get; private set; } = string.Empty;

    // Travel preferences
    public string? PreferredAirportCode { get; private set; }
    public string? PreferredAirlineCode { get; private set; }
    public string? PreferredHotelChain { get; private set; }
    public string? PreferredCarRentalCompany { get; private set; }

    // Notification preferences
    public bool NotifyByEmail { get; private set; } = true;
    public bool NotifyBySms { get; private set; } = false;
    public bool NotifyBookingConfirmation { get; private set; } = true;
    public bool NotifyApprovalRequired { get; private set; } = true;
    public bool NotifyApprovalDecision { get; private set; } = true;
    public bool NotifyTripReminders { get; private set; } = true;

    private UserPreferences() { }

    public static UserPreferences Create(string id, string userId)
        => new() { Id = id, UserId = userId, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };

    public void UpdateTravelPreferences(
        string? airportCode,
        string? airlineCode,
        string? hotelChain,
        string? carRental)
    {
        PreferredAirportCode = airportCode;
        PreferredAirlineCode = airlineCode;
        PreferredHotelChain = hotelChain;
        PreferredCarRentalCompany = carRental;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateNotifications(
        bool email, bool sms,
        bool booking, bool approvalRequired,
        bool approvalDecision, bool tripReminders)
    {
        NotifyByEmail = email;
        NotifyBySms = sms;
        NotifyBookingConfirmation = booking;
        NotifyApprovalRequired = approvalRequired;
        NotifyApprovalDecision = approvalDecision;
        NotifyTripReminders = tripReminders;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
