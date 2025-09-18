using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BlazorLoginDemo.Shared.Models.Kernel.Platform;
using BlazorLoginDemo.Shared.Validation;
using NanoidDotNet;

namespace BlazorLoginDemo.Shared.Models.Policies;

public class TravelPolicy
{
    [Key]
    [MaxLength(14)]
    public string Id { get; set; } = Nanoid.Generate(alphabet: Nanoid.Alphabets.LettersAndDigits.ToUpper(), size: 14);
    public required string PolicyName { get; set; }
    public required string OrganizationUnifiedId { get; set; }

    // financial details
    [Required]
    [CurrencyTypeValidation]
    [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency must be exactly 3 uppercase letters.")]
    [JsonPropertyName("currency")]
    [DefaultValue("AUD")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string DefaultCurrencyCode { get; set; } = "AUD";

    public decimal MaxFlightPrice { get; set; } = 0m;

    // flight particulars
    [CabinTypeValidation]
    [DefaultValue("ECONOMY")]
    public string DefaultFlightSeating { get; set; } = "ECONOMY";

    [CabinTypeValidation]
    [DefaultValue("ECONOMY")]
    public string MaxFlightSeating { get; set; } = "ECONOMY";

    public string[] IncludedAirlineCodes { get; set; } = Array.Empty<string>();
    public string[] ExcludedAirlineCodes { get; set; } = Array.Empty<string>();

    [CoverageTypeValidation]
    [DefaultValue("MOST_SEGMENTS")]
    public string CabinClassCoverage { get; set; } = "MOST_SEGMENTS";
    public bool? NonStopFlight { get; set; }

    // amadeus (and other system) specifics [meta]
    //public int MaxResults { get; set; } = 20;

    // times for bookings (business rules)
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [RegularExpression(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$", ErrorMessage = "Time must be in the format hh:mm:ss.")]
    public string? FlightBookingTimeAvailableFrom { get; set; }  // Local time. hh:mm:ss format, e.g 10:30:00

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [RegularExpression(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$", ErrorMessage = "Time must be in the format hh:mm:ss.")]
    public string? FlightBookingTimeAvailableTo { get; set; }  // Local time. hh:mm:ss format, e.g 10:30:00

    public bool? EnableSaturdayFlightBookings { get; set; }

    public bool? EnableSundayFlightBookings { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? DefaultCalendarDaysInAdvanceForFlightBooking { get; set; }

    [JsonIgnore] // Prevent circular reference during serialization.
    public OrganizationUnified Organization { get; set; } = default!;

    // Allowed destinations (can be entire regions, continents, or specific countries)
    public ICollection<Region> Regions { get; set; } = new List<Region>();
    public ICollection<Continent> Continents { get; set; } = new List<Continent>();
    public ICollection<Country> Countries { get; set; } = new List<Country>();

    // If a broader selection is made (e.g. enabling APAC),
    // you can exclude specific countries via this collection.
    public ICollection<TravelPolicyDisabledCountry> DisabledCountries { get; set; } = new List<TravelPolicyDisabledCountry>();

    // Convenience for UI binding / readability (not stored)
    [NotMapped]
    public string IncludedAirlineCodesCsv
    {
        get => string.Join(", ", IncludedAirlineCodes);
        set
        {
            IncludedAirlineCodes = string.IsNullOrWhiteSpace(value)
                ? Array.Empty<string>()
                : value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                       .Select(s => s.Trim().ToUpperInvariant())
                       .Distinct()
                       .ToArray();
        }
    }
    
    // Convenience for UI binding / readability (not stored)
    [NotMapped]
    public string ExcludedAirlineCodesCsv
    {
        get => string.Join(", ", ExcludedAirlineCodes);
        set
        {
            ExcludedAirlineCodes = string.IsNullOrWhiteSpace(value)
                ? Array.Empty<string>()
                : value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                       .Select(s => s.Trim().ToUpperInvariant())
                       .Distinct()
                       .ToArray();
        }
    }
}
