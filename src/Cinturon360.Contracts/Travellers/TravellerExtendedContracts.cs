namespace Cinturon360.Contracts.Travellers;

/// <summary>Request to update traveller profile data (passport + personal).</summary>
public sealed record UpdateTravellerProfileRequest(
    string? PassportNumber,
    string? PassportCountry,
    DateOnly? PassportExpiry,
    string? Nationality,
    DateOnly? DateOfBirth,
    string? Gender,
    string? TsaPreCheckNumber,
    string? GlobalEntryNumber,
    string? RedressNumber,
    string? PreferredSeatType,
    string? PreferredMealType);

/// <summary>Alias kept for compatibility with UpdatePreferences endpoint.</summary>
public sealed record UpdateTravelPreferencesRequest(
    string? PreferredAirportCode,
    string? PreferredAirlineCode,
    string? PreferredHotelChain,
    string? PreferredCarRentalCompany,
    bool NotifyByEmail,
    bool NotifyBySms,
    bool NotifyBookingConfirmation,
    bool NotifyApprovalRequired,
    bool NotifyApprovalDecision,
    bool NotifyTripReminders);

/// <summary>List wrapper for loyalty programs.</summary>
public sealed record LoyaltyProgramsResponse(IReadOnlyList<LoyaltyProgramResponse> Items);

/// <summary>List wrapper for emergency contacts.</summary>
public sealed record EmergencyContactsResponse(IReadOnlyList<EmergencyContactResponse> Items);

/// <summary>Alias for user travel preferences.</summary>
public sealed record TravelPreferencesResponse(
    string? PreferredAirportCode,
    string? PreferredAirlineCode,
    string? PreferredHotelChain,
    string? PreferredCarRentalCompany,
    bool NotifyByEmail,
    bool NotifyBySms,
    bool NotifyBookingConfirmation,
    bool NotifyApprovalRequired,
    bool NotifyApprovalDecision,
    bool NotifyTripReminders);
