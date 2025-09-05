using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BlazorLoginDemo.Shared.Models.Static;
using BlazorLoginDemo.Shared.Validation;
using NanoidDotNet;

namespace BlazorLoginDemo.Shared.Models.Kernel.User;

public class AvaUserSysPreference
{
    [Key]
    public string Id { get; set; } = Nanoid.Generate();
    
    [Required]
    public required string AspNetUsersId { get; set; }
    
    public bool IsActive { get; set; } = true;

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [PassportNameValidation]
    public required string FirstName { get; set; } = string.Empty;
    
    [PassportNameValidation]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MiddleName { get; set; }

    [Required]
    [PassportNameValidation]
    public required string LastName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)] // Only year, month, day
    public DateOnly DateOfBirth { get; set; } = new DateOnly(1900, 1, 1);

    [Required]
    [PassportNameValidation]
    public required string PassportName { get; set; } = string.Empty;

    [Required]
    public required GenderType Gender { get; set; } = GenderType.Unspecified;

    [Required]
    public required PassportCountry CountryOfIssue { get; set; } = PassportCountry.AUS;

    [Required]
    [DataType(DataType.Date)]
    public DateOnly PassportExpirationDate { get; set; } = new DateOnly(1900, 1, 1);

    // user location defaults
    [AlphaNumeric3Validation]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? OriginLocationCode { get; set; }

    // flight default details
    [Required]
    [CabinTypeValidation]
    [DefaultValue("ECONOMY")]
    public required string DefaultFlightSeating { get; set; } = "ECONOMY";

    [Required]
    [CabinTypeValidation]
    [DefaultValue("ECONOMY")]
    public required string MaxFlightSeating { get; set; } = "ECONOMY";

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IncludedAirlineCodes { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ExcludedAirlineCodes { get; set; }

    [CoverageTypeValidation]
    [DefaultValue("MOST_SEGMENTS")]
    public string CabinClassCoverage { get; set; } = "MOST_SEGMENTS";

    public bool NonStopFlight { get; set; } = false;
    
    // financial considerations for bookings
    [Required]
    [CurrencyTypeValidation]
    [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency must be exactly 3 uppercase letters.")]
    [JsonPropertyName("currency")]
    [DefaultValue("AUD")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string DefaultCurrencyCode { get; set; } = "AUD";

    public int MaxFlightPrice { get; set; } = 0;

    // amadeus (and other system) specifics
    public int MaxResults { get; set; } = 20;

    // earliest time bookable
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [RegularExpression(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$", ErrorMessage = "Time must be in the format hh:mm:ss.")]
    public string? FlightBookingTimeAvailableFrom { get; set; }  // Local time. hh:mm:ss format, e.g 10:30:00

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [RegularExpression(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$", ErrorMessage = "Time must be in the format hh:mm:ss.")]
    public string? FlightBookingTimeAvailableTo { get; set; }  // Local time. hh:mm:ss format, e.g 10:30:00

    // things that come from policy ONLY
    // allow bookings on weekends
    [DefaultValue(false)]
    public bool EnableSaturdayFlightBookings { get; set; } = false;

    [DefaultValue(false)]
    public bool EnableSundayFlightBookings { get; set; } = false;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? DefaultCalendarDaysInAdvanceForFlightBooking { get; set; }

    // travel policy name
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TravelPolicyName { get; set; }

    // travel policy - if this is not provided, it will find out if one should exist from
    // the AvaClientId (if provided) else from the email address (if domain exists)
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TravelPolicyId { get; set; }

    //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonIgnore]
    public TravelPolicy? TravelPolicy { get; set; }

    // Optional link to a Client; not every user must have a Client parent, this will also be
    // updated by the API if it finds a match for the email address domain
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AvaClientId { get; set; }

    [JsonIgnore]
    public AvaClient? AvaClient { get; set; }

    // this will only be filled in by the API, it's completely optional for the user to provide etc
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ClientId { get; set; }
}
