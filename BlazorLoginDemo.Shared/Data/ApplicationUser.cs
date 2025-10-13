using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BlazorLoginDemo.Shared.Models.Auth;
using BlazorLoginDemo.Shared.Models.Kernel.Platform;
using BlazorLoginDemo.Shared.Models.Policies;
using BlazorLoginDemo.Shared.Models.Static;
using BlazorLoginDemo.Shared.Models.Static.Platform;
using BlazorLoginDemo.Shared.Models.User;
using BlazorLoginDemo.Shared.Validation;
using Microsoft.AspNetCore.Identity;

namespace BlazorLoginDemo.Shared.Data;

/// <summary>
/// Unified user profile for the platform. This replaces the split between AspNetUsers, AvaUser,
/// and AvaUserSysPreference by bringing everything into a single Identity user entity.
/// </summary>
public class ApplicationUser : IdentityUser
{
    // -----------------------------
    // Core account / directory info
    // -----------------------------
    [Required]
    public bool IsActive { get; set; } = true;

    public DateTimeOffset? LastSeenUtc { get; set; }

    // Display & name parts
    public string? DisplayName { get; set; }

    [PassportNameValidation]
    public string? FirstName { get; set; }

    [PassportNameValidation]
    public string? MiddleName { get; set; }

    [PassportNameValidation]
    public string? LastName { get; set; }

    public string? Department { get; set; }

    // Culture / formatting preference (GDPR PersonalData)
    [PersonalData, MaxLength(16)]
    public string? PreferredCulture { get; set; } = "en-AU"; // e.g. en, en-AU

    // Convenience: computed full name (not mapped)
    [NotMapped]
    public string FullName => string.Join(" ", new[] { FirstName, MiddleName, LastName }
        .Where(s => !string.IsNullOrWhiteSpace(s)));

    // -----------------------------
    // Org / tenancy
    // -----------------------------
    public string? OrganizationId { get; set; }
    public OrganizationUnified? Organization { get; set; }

    public UserCategoryType UserCategory { get; set; }

    // Convenience getters (derived from Organization tree)
    [NotMapped]
    public string? TmcId => Organization?.Type switch
    {
        OrganizationType.Tmc    => Organization.Id,
        OrganizationType.Client => Organization.ParentOrganizationId,
        _ => null
    };

    [NotMapped]
    public string? ClientId => Organization?.Type == OrganizationType.Client ? Organization.Id : null;

    // -----------------------------
    // Identity / auth adjuncts
    // -----------------------------
    public List<RefreshToken> RefreshTokens { get; set; } = new();

    // Manager / org chart (self-referencing)
    public string? ManagerId { get; set; }
    public ApplicationUser? Manager { get; set; }
    public ICollection<ApplicationUser> DirectReports { get; set; } = new List<ApplicationUser>();
    public string? CostCentre { get; set; } = string.Empty;

    // -----------------------------
    // PII & travel-doc basics (ex-AvaUserSysPreference)
    // -----------------------------
    [DataType(DataType.Date)]
    public DateOnly? DateOfBirth { get; set; }

    public GenderType Gender { get; set; } = GenderType.Unspecified;

    public PassportCountry CountryOfIssue { get; set; } = PassportCountry.AUS;

    [DataType(DataType.Date)]
    public DateOnly? PassportExpirationDate { get; set; }

    // -----------------------------
    // Travel defaults & constraints
    // -----------------------------
    [AlphaNumeric3Validation]
    public string? OriginLocationCode { get; set; }

    // Cabin preferences
    [CabinTypeValidation]
    [DefaultValue("ECONOMY")]
    public string DefaultFlightSeating { get; set; } = "ECONOMY";
    public bool DefaultFlightSeatingVisible { get; set; } = false;

    [CabinTypeValidation]
    [DefaultValue("ECONOMY")]
    public string MaxFlightSeating { get; set; } = "ECONOMY";
    public bool MaxFlightSeatingVisible { get; set; } = false;

    public string[] IncludedAirlineCodes { get; set; } = Array.Empty<string>();
    public string[] ExcludedAirlineCodes { get; set; } = Array.Empty<string>();
    public bool AirlineCodesVisible { get; set; } = false;

    [CoverageTypeValidation]
    [DefaultValue("MOST_SEGMENTS")]
    public string CabinClassCoverage { get; set; } = "MOST_SEGMENTS";
    public bool CabinClassCoverageVisible { get; set; } = false;

    public bool? NonStopFlight { get; set; } = false;
    public bool NonStopFlightVisible { get; set; } = false;

    // Financial / search limits
    [CurrencyTypeValidation]
    [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency must be exactly 3 uppercase letters.")]
    [JsonPropertyName("currency")]
    [DefaultValue("AUD")]
    public string DefaultCurrencyCode { get; set; } = "AUD";
    public bool DefaultCurrencyCodeVisible { get; set; } = false;

    public decimal MaxFlightPrice { get; set; } = 0m;
    public bool MaxFlightPriceVisible { get; set; } = false;

    public int MaxResults { get; set; } = 20; // Amadeus and others
    public bool MaxResultsVisible { get; set; } = false;

    // Local booking time windows (normalized to hh:mm:ss)
    private string? _flightBookingTimeAvailableFrom;
    private string? _flightBookingTimeAvailableTo;

    [RegularExpression(@"^(?:[01]\d|2[0-3]):[0-5]\d:[0-5]\d$", ErrorMessage = "Time must be in the format hh:mm:ss.")]
    public string? FlightBookingTimeAvailableFrom
    {
        get => _flightBookingTimeAvailableFrom;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                _flightBookingTimeAvailableFrom = null;
            else if (TimeSpan.TryParse(value, out var ts))
                _flightBookingTimeAvailableFrom = ts.ToString(@"hh\:mm\:ss");
            else
                _flightBookingTimeAvailableFrom = value; // let validation surface issues
        }
    }

    [RegularExpression(@"^(?:[01]\d|2[0-3]):[0-5]\d:[0-5]\d$", ErrorMessage = "Time must be in the format hh:mm:ss.")]
    public string? FlightBookingTimeAvailableTo
    {
        get => _flightBookingTimeAvailableTo;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                _flightBookingTimeAvailableTo = null;
            else if (TimeSpan.TryParse(value, out var ts))
                _flightBookingTimeAvailableTo = ts.ToString(@"hh\:mm\:ss");
            else
                _flightBookingTimeAvailableTo = value;
        }
    }
    public bool FlightBookingTimeAvailableVisible { get; set; } = false;

    // Policy-driven toggles
    [DefaultValue(false)]
    public bool? EnableSaturdayFlightBookings { get; set; } = false;
    
    [DefaultValue(false)]
    public bool? EnableSundayFlightBookings { get; set; } = false;

    public bool EnableWeekendFlightBookingsVisible { get; set; } = false;
    public int? DefaultCalendarDaysInAdvanceForFlightBooking { get; set; }
    public bool CalendarDaysInAdvanceForFlightBookingVisible { get; set; } = false;
    
    // -----------------------------
    // Policy & expense linkage
    // -----------------------------
    public string? TravelPolicyId { get; set; }
    public string? TravelPolicyName { get; set; }

    [JsonIgnore]
    public TravelPolicy? TravelPolicy { get; set; }

    public string? ExpensePolicyId { get; set; }
    public string? ExpensePolicyName { get; set; }

    // -----------------------------
    // Loyalty programs (1:N)
    // -----------------------------
    public ICollection<AvaUserLoyaltyAccount> LoyaltyAccounts { get; set; } = new List<AvaUserLoyaltyAccount>();
}
