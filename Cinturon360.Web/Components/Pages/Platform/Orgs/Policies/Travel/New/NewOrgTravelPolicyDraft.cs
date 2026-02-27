using System.ComponentModel.DataAnnotations;
using Cinturon360.Shared.Models.Static.Travel;

namespace Cinturon360.Web.Drafts.Platform.Orgs.Policies.Travel;

public sealed class NewOrgTravelPolicyDraft
{
    // -------------------------
    // Context
    // -------------------------

    [Required, StringLength(25)]
    public string OrgId { get; set; } = default!;

    // -------------------------
    // Core policy info
    // -------------------------

    [Required, StringLength(100)]
    public string PolicyName { get; set; } = string.Empty;

    [Required, RegularExpression(@"^[A-Z]{3}$",
        ErrorMessage = "Currency must be exactly 3 uppercase letters.")]
    public string DefaultCurrencyCode { get; set; } = "AUD";

    public bool SetAsDefaultPolicy { get; set; } = false;

    [Required]
    public DateTime EffectiveFromUtc { get; set; } = DateTime.UtcNow;

    // Only optional field by design
    public DateTime EffectiveToUtc { get; set; } = DateTime.MaxValue;


    // -------------------------
    // Flights
    // -------------------------

    [Range(0, double.MaxValue, ErrorMessage = "Must be 0 or greater")] public decimal MaxFlightPrice { get; set; } = 0m;

    public FlightTravelClassType DefaultFlightSeating { get; set; }
        = FlightTravelClassType.ECONOMY;

    public FlightTravelClassType MaxFlightSeating { get; set; }
        = FlightTravelClassType.BUSINESS;

    public CabinClassCoverageType CabinClassCoverage { get; set; }
        = CabinClassCoverageType.MOST_SEGMENTS;

    public bool NonStopFlight { get; set; } = false;

    public List<string> IncludedAirlineCodes { get; set; } = new();
    public List<string> ExcludedAirlineCodes { get; set; } = new();

    [Required, RegularExpression(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$")]
    public string FlightBookingTimeAvailableFrom { get; set; } = "00:00:00";

    [Required, RegularExpression(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$")]
    public string FlightBookingTimeAvailableTo { get; set; } = "23:59:59";

    public bool EnableSaturdayFlightBookings { get; set; } = false;
    public bool EnableSundayFlightBookings { get; set; } = false;

    [Range(0, int.MaxValue, ErrorMessage = "Must be 0 or greater")] public int DefaultCalendarDaysInAdvanceForFlightBooking { get; set; } = 0;

    public FlightTravelClassType MaxFlightSeatingAt6Hours { get; set; }
        = FlightTravelClassType.ECONOMY;
    
    public FlightTravelClassType MaxFlightSeatingAt8Hours { get; set; }
        = FlightTravelClassType.ECONOMY;
    public FlightTravelClassType MaxFlightSeatingAt10Hours { get; set; }
        = FlightTravelClassType.PREMIUM_ECONOMY;
    public FlightTravelClassType MaxFlightSeatingAt14Hours { get; set; }
        = FlightTravelClassType.PREMIUM_ECONOMY;

    [Range(0, double.MaxValue, ErrorMessage = "Must be 0 or greater")] public decimal MaxFlightPriceAt6Hours { get; set; } = 0m;
    [Range(0, double.MaxValue, ErrorMessage = "Must be 0 or greater")] public decimal MaxFlightPriceAt8Hours { get; set; } = 0m;
    [Range(0, double.MaxValue, ErrorMessage = "Must be 0 or greater")] public decimal MaxFlightPriceAt10Hours { get; set; } = 0m;
    [Range(0, double.MaxValue, ErrorMessage = "Must be 0 or greater")] public decimal MaxFlightPriceAt14Hours { get; set; } = 0m;

    
    // -------------------------
    // Hotels
    // -------------------------

    [Range(0, double.MaxValue, ErrorMessage = "Must be 0 or greater")] public decimal MaxHotelNightlyRate { get; set; } = 0m;

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

    [Range(0, double.MaxValue, ErrorMessage = "Must be 0 or greater")] public decimal MaxTaxiFarePerRide { get; set; } = 0m;
    [Range(0, double.MaxValue, ErrorMessage = "Must be 0 or greater")] public decimal MaxTaxiSurgeMultiplier { get; set; } = 0m;

    public List<string> IncludedTaxiVendors { get; set; } = new List<string>();
    public List<string> ExcludedTaxiVendors { get; set; } = new List<string>();

    public RailTravelClassType DefaultTrainClass { get; set; }
        = RailTravelClassType.SECOND;

    public RailTravelClassType MaxTrainClass { get; set; }
        = RailTravelClassType.FIRST;

    [Range(0, double.MaxValue, ErrorMessage = "Must be 0 or greater")] public decimal MaxTrainPrice { get; set; } = 0m;

    public List<string> IncludedRailOperators { get; set; } = new List<string>();
    public List<string> ExcludedRailOperators { get; set; } = new List<string>();


    // -------------------------
    // Car hire
    // -------------------------

    [Range(0, double.MaxValue, ErrorMessage = "Must be 0 or greater")] public decimal MaxCarDailyRate { get; set; } = 0m;

    public char DefaultCarCategory { get; set; } = 'M';
    public char MaxCarCategory { get; set; } = 'P';

    public char DefaultCarBody { get; set; } = 'B';
    public char MaxCarBody { get; set; } = 'F';

    public char DefaultCarTransmission { get; set; } = 'A';
    public char MaxCarTransmission { get; set; } = 'A';

    public char DefaultCarFuel { get; set; } = 'R';
    public char MaxCarFuel { get; set; } = 'R';

    public bool RequireInclusiveInsurance { get; set; } = true;

    public string[] IncludedCarHireVendors { get; set; } = Array.Empty<string>();
    public string[] ExcludedCarHireVendors { get; set; } = Array.Empty<string>();


    // -------------------------
    // Bus / Coach
    // -------------------------
    [Range(0, double.MaxValue, ErrorMessage = "Must be 0 or greater")] public decimal MaxBusFarePerTicket { get; set; } = 0m;
    public string[] IncludedBusOperators { get; set; } = Array.Empty<string>();
    public string[] ExcludedBusOperators { get; set; } = Array.Empty<string>();


    // -------------------------
    // eSIM / SIM cards
    // -------------------------
    [Range(0, double.MaxValue, ErrorMessage = "Must be 0 or greater")] public decimal MaxSimCardPrice { get; set; } = 0m;
    [Range(0, double.MaxValue, ErrorMessage = "Must be 0 or greater")] public decimal MinSimDataAllowanceGb { get; set; } = 0m;
    [Range(0, int.MaxValue, ErrorMessage = "Must be 0 or greater")] public int MinSimValidityDays { get; set; } = 0;
    public bool EnableVoiceSimCards { get; set; } = false;
    public bool EnableDataSimCards { get; set; } = true;
    public bool EnableSimHotspotTethering { get; set; } = true;
    public bool EnablePortableWifiRental { get; set; } = true;
    public string[] IncludedSimVendors { get; set; } = Array.Empty<string>();
    public string[] ExcludedSimVendors { get; set; } = Array.Empty<string>();


    // -------------------------
    // Activities / Experiences
    // -------------------------
    public bool AllowHighRiskActivities { get; set; } = false;
    [Range(0, double.MaxValue, ErrorMessage = "Must be 0 or greater")]  public decimal MaxActivityPricePerBooking { get; set; } = 0m;
    public string[] IncludedActivityProviders { get; set; } = Array.Empty<string>();
    public string[] ExcludedActivityProviders { get; set; } = Array.Empty<string>();


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
    [Range(0, double.MaxValue, ErrorMessage = "Must be 0 or greater")]  public decimal L1ApprovalAmount { get; set; } = 0m;

    public bool L2ApprovalRequired { get; set; } = false;
    [Range(0, double.MaxValue, ErrorMessage = "Must be 0 or greater")]  public decimal L2ApprovalAmount { get; set; } = 0m;

    public bool L3ApprovalRequired { get; set; } = false;
    [Range(0, double.MaxValue, ErrorMessage = "Must be 0 or greater")]  public decimal L3ApprovalAmount { get; set; } = 0m;

    public bool OrgBillingContactApprovalToPolicyLimit { get; set; } = false;
    public bool OrgBillingContactApprovalAbovePolicyLimit { get; set; } = false;
}
