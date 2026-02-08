using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cinturon360.Shared.Validation;

namespace Cinturon360.Shared.Contracts.Policies;

/// <summary>
/// Contract used to update an existing Travel Policy.
/// This DTO is validation-focused and intentionally verbose to keep
/// business rules explicit at the API boundary.
/// </summary>
public sealed class UpdateTravelPolicyRequest
{
    // =========================================================================
    // Policy identity & ownership
    // =========================================================================

    /// <summary>
    /// Unique travel policy identifier.
    /// </summary>
    [StringLength(25)]
    public string PolicyId { get; set; } = string.Empty;

    /// <summary>
    /// Human-friendly policy name.
    /// Displayed in UI and approval workflows.
    /// </summary>
    public string PolicyName { get; set; } = string.Empty;

    /// <summary>
    /// Owning organisation (Unified org ID).
    /// </summary>
    public string OrganizationId { get; set; } = string.Empty;

    // =========================================================================
    // Currency & effective window
    // =========================================================================

    /// <summary>
    /// Default settlement currency for policy limits.
    /// ISO 4217, enforced as uppercase.
    /// </summary>
    [CurrencyTypeValidation]
    [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency must be exactly 3 uppercase letters.")]
    public string DefaultCurrencyCode { get; set; } = "AUD";

    /// <summary>
    /// Policy start date (UTC).
    /// </summary>
    public DateTime EffectiveFromUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Policy end date (UTC).
    /// Defaults to 12 months.
    /// </summary>
    public DateTime EffectiveToUtc { get; set; } = DateTime.UtcNow.AddYears(1);

    // =========================================================================
    // FLIGHTS – blanket defaults (apply unless overridden)
    // =========================================================================

    /// <summary>
    /// Absolute maximum price for a flight.
    /// 0 = no price ceiling unless approval rules apply.
    /// </summary>
    public decimal MaxFlightPrice { get; set; } = 0m;

    /// <summary>
    /// Default cabin when no restriction applies.
    /// </summary>
    [CabinTypeValidation]
    public string DefaultFlightSeating { get; set; } = "ECONOMY";

    /// <summary>
    /// Maximum allowed cabin if no duration rule applies.
    /// </summary>
    [CabinTypeValidation]
    public string MaxFlightSeating { get; set; } = "ECONOMY";

    /// <summary>
    /// Cabin coverage strategy (e.g., MOST_SEGMENTS, ALL_SEGMENTS).
    /// </summary>
    [CoverageTypeValidation]
    public string CabinClassCoverage { get; set; } = "MOST_SEGMENTS";

    /// <summary>
    /// Restrict to non-stop flights only.
    /// </summary>
    public bool NonStopFlight { get; set; } = false;

    /// <summary>
    /// Airline allow/deny lists.
    /// Empty = no restriction.
    /// </summary>
    public string[] IncludedAirlineCodes { get; set; } = Array.Empty<string>();
    public string[] ExcludedAirlineCodes { get; set; } = Array.Empty<string>();

    // =========================================================================
    // FLIGHT BOOKING TIME RULES (local time)
    // =========================================================================

    [RegularExpression(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$")]
    public string FlightBookingTimeAvailableFrom { get; set; } = "00:00:00";

    [RegularExpression(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$")]
    public string FlightBookingTimeAvailableTo { get; set; } = "23:59:59";

    public bool EnableSaturdayFlightBookings { get; set; } = false;
    public bool EnableSundayFlightBookings { get; set; } = false;

    /// <summary>
    /// Minimum number of calendar days before departure.
    /// 0 = same-day allowed.
    /// </summary>
    public int DefaultCalendarDaysInAdvanceForFlightBooking { get; set; } = 0;

    // =========================================================================
    // ACCOMMODATION (HOTELS)
    // =========================================================================

    [Column(TypeName = "numeric(14,2)")]
    public decimal MaxHotelNightlyRate { get; set; } = 0m;

    public string DefaultHotelRoomType { get; set; } = "STANDARD";
    public string MaxHotelRoomType { get; set; } = "STANDARD";

    public string[] IncludedHotelChains { get; set; } = Array.Empty<string>();
    public string[] ExcludedHotelChains { get; set; } = Array.Empty<string>();

    [RegularExpression(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$")]
    public string HotelBookingTimeAvailableFrom { get; set; } = "00:00:00";

    [RegularExpression(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$")]
    public string HotelBookingTimeAvailableTo { get; set; } = "23:59:59";

    public bool EnableSaturdayHotelBookings { get; set; } = false;
    public bool EnableSundayHotelBookings { get; set; } = false;

    // =========================================================================
    // TAXI / RIDE-HAIL
    // =========================================================================

    [Column(TypeName = "numeric(14,2)")]
    public decimal MaxTaxiFarePerRide { get; set; } = 0m;

    public decimal MaxTaxiSurgeMultiplier { get; set; } = 0m;

    public string[] IncludedTaxiVendors { get; set; } = Array.Empty<string>();
    public string[] ExcludedTaxiVendors { get; set; } = Array.Empty<string>();

    // =========================================================================
    // RAIL / TRAIN
    // =========================================================================

    public string DefaultTrainClass { get; set; } = "STANDARD";
    public string MaxTrainClass { get; set; } = "STANDARD";

    [Column(TypeName = "numeric(14,2)")]
    public decimal? MaxTrainPrice { get; set; }

    public string[] IncludedRailOperators { get; set; } = Array.Empty<string>();
    public string[] ExcludedRailOperators { get; set; } = Array.Empty<string>();

    // =========================================================================
    // CAR HIRE
    // =========================================================================

    [Column(TypeName = "numeric(14,2)")]
    public decimal? MaxCarHireDailyRate { get; set; }

    [Column(TypeName = "numeric(14,2)")]
    public decimal MaxCarDailyRate { get; set; } = 0m; // UI-aligned field

    public string DefaultCarClass { get; set; } = "MBAR";
    public string MaxCarClass { get; set; } = "MBAR";

    public bool RequireInclusiveInsurance { get; set; } = true;

    public string[] AllowedCarHireClasses { get; set; } = Array.Empty<string>();
    public string[] IncludedCarHireVendors { get; set; } = Array.Empty<string>();
    public string[] ExcludedCarHireVendors { get; set; } = Array.Empty<string>();

    // =========================================================================
    // BUS / COACH
    // =========================================================================

    [Column(TypeName = "numeric(14,2)")]
    public decimal MaxBusFarePerTicket { get; set; } = 0m;

    public string[] IncludedBusOperators { get; set; } = Array.Empty<string>();
    public string[] ExcludedBusOperators { get; set; } = Array.Empty<string>();

    // =========================================================================
    // SIM / eSIM
    // =========================================================================

    [Column(TypeName = "numeric(14,2)")]
    public decimal MaxSimBundlePrice { get; set; } = 0m;

    public decimal MinSimDataGb { get; set; } = 5m;
    public int MinSimValidityDays { get; set; } = 30;

    public bool EnableVoiceSupport { get; set; } = false;
    public bool EnableSmsSupport { get; set; } = false;
    public bool EnableHotspotSupport { get; set; } = false;
    public bool Enable5GSupport { get; set; } = false;
    public bool Enable4GSupport { get; set; } = false;
    public bool Enable3GSupport { get; set; } = false;
    public bool EnableWiFiDeviceRental { get; set; } = false;

    public string[] IncludedSimVendors { get; set; } = Array.Empty<string>();
    public string[] ExcludedSimVendors { get; set; } = Array.Empty<string>();

    // =========================================================================
    // ACTIVITIES / EXCURSIONS
    // =========================================================================

    [Column(TypeName = "numeric(14,2)")]
    public decimal? MaxActivityPricePerPerson { get; set; }

    public bool AllowHighRiskActivities { get; set; } = false;

    public string[] IncludedActivityProviders { get; set; } = Array.Empty<string>();
    public string[] ExcludedActivityProviders { get; set; } = Array.Empty<string>();

    // =========================================================================
    // GEOGRAPHY (allow / deny layering)
    // =========================================================================

    /// <summary>
    /// Broad inclusions.
    /// Empty = global.
    /// </summary>
    public int[] RegionIds { get; set; } = Array.Empty<int>();
    public int[] ContinentIds { get; set; } = Array.Empty<int>();
    public int[] CountryIds { get; set; } = Array.Empty<int>();

    /// <summary>
    /// Explicit country exclusions when a parent region/continent is enabled.
    /// </summary>
    public int[] DisabledCountryIds { get; set; } = Array.Empty<int>();

    // =========================================================================
    // FLIGHT DURATION THRESHOLDS (override blanket rules)
    // =========================================================================
    // NOTE:
    // These are optional refinements.
    // If a value is 0 or default, the blanket rule applies.

    [CabinTypeValidation] public string MaxFlightSeatingAt6Hours { get; set; } = "ECONOMY";
    [CabinTypeValidation] public string MaxFlightSeatingAt8Hours { get; set; } = "ECONOMY";
    [CabinTypeValidation] public string MaxFlightSeatingAt10Hours { get; set; } = "ECONOMY";
    [CabinTypeValidation] public string MaxFlightSeatingAt14Hours { get; set; } = "ECONOMY";

    [Column(TypeName = "numeric(14,2)")] public decimal MaxFlightPriceAt6Hours { get; set; } = 0m;
    [Column(TypeName = "numeric(14,2)")] public decimal MaxFlightPriceAt8Hours { get; set; } = 0m;
    [Column(TypeName = "numeric(14,2)")] public decimal MaxFlightPriceAt10Hours { get; set; } = 0m;
    [Column(TypeName = "numeric(14,2)")] public decimal MaxFlightPriceAt14Hours { get; set; } = 0m;

    // =========================================================================
    // APPROVAL WORKFLOW RULES
    // =========================================================================

    /// <summary>
    /// Auto-approve bookings within policy limits.
    /// </summary>
    public bool AutoApproveToPolicyLimit { get; set; } = false;

    public bool RequireManagerApprovalToPolicyLimit { get; set; } = false;

    public bool L1ApprovalRequired { get; set; } = false;
    [Column(TypeName = "numeric(14,2)")] public decimal L1ApprovalAmount { get; set; } = 0m;

    public bool L2ApprovalRequired { get; set; } = false;
    [Column(TypeName = "numeric(14,2)")] public decimal L2ApprovalAmount { get; set; } = 0m;

    public bool L3ApprovalRequired { get; set; } = false;
    [Column(TypeName = "numeric(14,2)")] public decimal L3ApprovalAmount { get; set; } = 0m;

    public bool OrgBillingContactApprovalToPolicyLimit { get; set; } = false;
    public bool OrgBillingContactApprovalAbovePolicyLimit { get; set; } = false;
}
