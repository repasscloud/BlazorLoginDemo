using System.ComponentModel.DataAnnotations;
using Cinturon360.Shared.Models.Static.Travel;

namespace Cinturon360.Web.Drafts.Platform.Org.Policies.Travel;

public sealed class NewOrgTravelPolicyDraft
{
    // -------------------------
    // Context
    // -------------------------

    [Required, StringLength(25)]
    public string OrgId { get; set; } = string.Empty;

    // -------------------------
    // Core policy info
    // -------------------------

    [Required, StringLength(100)]
    public string PolicyName { get; set; } = string.Empty;

    [Required, RegularExpression(@"^[A-Z]{3}$",
        ErrorMessage = "Currency must be exactly 3 uppercase letters.")]
    public string DefaultCurrencyCode { get; set; } = string.Empty;

    public bool SetAsDefaultPolicy { get; set; } = false;

    [Required]
    public DateTime EffectiveFromUtc { get; set; }

    // Only optional field by design
    public DateTime? EffectiveToUtc { get; set; } = null;

    // -------------------------
    // Flights
    // -------------------------

    public decimal MaxFlightPrice { get; set; } = 0m;

    public FlightTravelClassType DefaultFlightSeating { get; set; }
        = FlightTravelClassType.ECONOMY;

    public FlightTravelClassType MaxFlightSeating { get; set; }
        = FlightTravelClassType.BUSINESS;

    public CabinClassCoverageType CabinClassCoverage { get; set; }
        = CabinClassCoverageType.MOST_SEGMENTS;

    public bool NonStopFlight { get; set; } = false;

    public string[] IncludedAirlineCodes { get; set; } = Array.Empty<string>();
    public string[] ExcludedAirlineCodes { get; set; } = Array.Empty<string>();

    [Required, RegularExpression(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$")]
    public string FlightBookingTimeAvailableFrom { get; set; } = "00:00:00";

    [Required, RegularExpression(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$")]
    public string FlightBookingTimeAvailableTo { get; set; } = "23:59:59";

    public bool EnableSaturdayFlightBookings { get; set; } = false;
    public bool EnableSundayFlightBookings { get; set; } = false;

    public int DefaultCalendarDaysInAdvanceForFlightBooking { get; set; } = 0;

    // -------------------------
    // Hotels
    // -------------------------

    public decimal MaxHotelNightlyRate { get; set; } = 0m;

    public HotelRoomClassType DefaultHotelRoomType { get; set; }
        = HotelRoomClassType.STANDARD;

    public HotelRoomClassType MaxHotelRoomType { get; set; }
        = HotelRoomClassType.DELUXE;

    public string[] IncludedHotelChains { get; set; } = Array.Empty<string>();
    public string[] ExcludedHotelChains { get; set; } = Array.Empty<string>();

    [Required, RegularExpression(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$")]
    public string HotelBookingTimeAvailableFrom { get; set; } = "00:00:00";

    [Required, RegularExpression(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$")]
    public string HotelBookingTimeAvailableTo { get; set; } = "23:59:59";

    public bool EnableSaturdayHotelBookings { get; set; } = false;
    public bool EnableSundayHotelBookings { get; set; } = false;

    // -------------------------
    // Ground transport
    // -------------------------

    public decimal MaxTaxiFarePerRide { get; set; } = 0m;
    public decimal MaxTaxiSurgeMultiplier { get; set; } = 0m;

    public string[] IncludedTaxiVendors { get; set; } = Array.Empty<string>();
    public string[] ExcludedTaxiVendors { get; set; } = Array.Empty<string>();

    public RailTravelClassType DefaultTrainClass { get; set; }
        = RailTravelClassType.SECOND;

    public RailTravelClassType MaxTrainClass { get; set; }
        = RailTravelClassType.FIRST;

    public decimal? MaxTrainPrice { get; set; }

    public string[] IncludedRailOperators { get; set; } = Array.Empty<string>();
    public string[] ExcludedRailOperators { get; set; } = Array.Empty<string>();

    // -------------------------
    // Car hire
    // -------------------------

    public decimal MaxCarDailyRate { get; set; } = 0m;

    public char DefaultCarCategory { get; set; } = 'C';
    public char MaxCarCategory { get; set; } = 'P';

    public char DefaultCarBody { get; set; } = 'C';
    public char MaxCarBody { get; set; } = 'F';

    public char DefaultCarTransmission { get; set; } = 'A';
    public char MaxCarTransmission { get; set; } = 'A';

    public char DefaultCarFuel { get; set; } = 'R';
    public char MaxCarFuel { get; set; } = 'R';

    public bool RequireInclusiveInsurance { get; set; } = true;

    public string[] IncludedCarHireVendors { get; set; } = Array.Empty<string>();
    public string[] ExcludedCarHireVendors { get; set; } = Array.Empty<string>();

    // -------------------------
    // Geography
    // -------------------------

    public int[] RegionIds { get; set; } = Array.Empty<int>();
    public int[] ContinentIds { get; set; } = Array.Empty<int>();
    public int[] CountryIds { get; set; } = Array.Empty<int>();
    public int[] DisabledCountryIds { get; set; } = Array.Empty<int>();

    // -------------------------
    // Approval rules
    // -------------------------

    public bool AutoApproveToPolicyLimit { get; set; } = false;
    public bool RequireManagerApprovalToPolicyLimit { get; set; } = false;

    public bool L1ApprovalRequired { get; set; } = false;
    public decimal L1ApprovalAmount { get; set; } = 0m;

    public bool L2ApprovalRequired { get; set; } = false;
    public decimal L2ApprovalAmount { get; set; } = 0m;

    public bool L3ApprovalRequired { get; set; } = false;
    public decimal L3ApprovalAmount { get; set; } = 0m;

    public bool OrgBillingContactApprovalToPolicyLimit { get; set; } = false;
    public bool OrgBillingContactApprovalAbovePolicyLimit { get; set; } = false;
}
