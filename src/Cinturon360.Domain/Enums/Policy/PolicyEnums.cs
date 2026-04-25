namespace Cinturon360.Domain.Enums.Policy;

public enum PolicyRuleType
{
    MaxFare               = 1,
    CabinClassRestriction = 2,
    AdvanceBookingWindow  = 3,
    PreferredAirline      = 4,
    PreferredHotel        = 5,
    MaxHotelNightlyRate   = 6,
    RequireApproval       = 7,
    ExcludedAirline       = 8,
    ExcludedHotel         = 9,
    CarRentalRestriction  = 10
}

public enum PolicyAssignmentTarget
{
    Org    = 1,
    User   = 2,
    Role   = 3
}

public enum PolicyViolationAction
{
    Block        = 1,
    RequireApproval = 2,
    Warn         = 3,
    Log          = 4
}
