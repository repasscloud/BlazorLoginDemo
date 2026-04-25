namespace Cinturon360.Contracts.Travellers;

// ── Requests ─────────────────────────────────────────────────────────────
public sealed record UpdatePassportRequest(
    string? PassportNumber,
    string? PassportCountry,
    DateOnly? PassportExpiry,
    string? Nationality);

public sealed record UpdatePreferencesRequest(
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

public sealed record AddLoyaltyProgramRequest(
    string ProgramCode,
    string ProgramName,
    string MembershipNumber,
    string? TierName,
    DateOnly? ExpiryDate);

public sealed record AddEmergencyContactRequest(
    string Name,
    string Relationship,
    string Phone,
    string? Email,
    bool IsPrimary);

// ── Responses ────────────────────────────────────────────────────────────
public sealed record TravellerProfileResponse(
    string UserId,
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
    string? PreferredMealType,
    string? VipLevel);

public sealed record LoyaltyProgramResponse(
    string Id,
    string ProgramCode,
    string ProgramName,
    string MembershipNumber,
    string? TierName,
    DateOnly? ExpiryDate);

public sealed record EmergencyContactResponse(
    string Id,
    string Name,
    string Relationship,
    string Phone,
    string? Email,
    bool IsPrimary);

public sealed record UserPreferencesResponse(
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
