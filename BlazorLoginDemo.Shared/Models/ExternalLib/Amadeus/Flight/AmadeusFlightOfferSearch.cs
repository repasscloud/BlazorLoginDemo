using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BlazorLoginDemo.Shared.Validation;

namespace BlazorLoginDemo.Shared.Models.ExternalLib.Amadeus;

public class AmadeusFlightOfferSearch
{
    [JsonPropertyName("currencyCode")]
    [Required]
    [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency code must be exactly 3 uppercase letters.")]
    public required string CurrencyCode { get; set; }

    [JsonPropertyName("originDestinations")]
    public required List<OriginDestination> OriginDestinations { get; set; }

    [JsonPropertyName("travelers")]
    public required List<Traveler> Travelers { get; set; }

    [JsonPropertyName("sources")]
    public List<string> Sources { get; set; } = ["GDS"];

    [JsonPropertyName("searchCriteria")]
    public required SearchCriteria SearchCriteria { get; set; }
}

public class OriginDestination
{
    [JsonPropertyName("id")]
    [Required]
    public required string Id { get; set; }

    [JsonPropertyName("originLocationCode")]
    public required string OriginLocationCode { get; set; }

    [JsonPropertyName("destinationLocationCode")]
    public required string DestinationLocationCode { get; set; }

    [JsonPropertyName("departureDateTimeRange")]
    public DepartureDateTimeRange? DateTimeRange { get; set; }
}

public class DepartureDateTimeRange
{
    [JsonPropertyName("date")]
    [Required(AllowEmptyStrings = false)]
    [RegularExpression(
        @"^\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])$",
        ErrorMessage = "Date must be YYYY-MM-DD.")]
    public required string Date { get; set; }  // e.g., "2018-12-25"

    [JsonPropertyName("time")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [RegularExpression(
        @"^([01]\d|2[0-3]):[0-5]\d:[0-5]\d$",
        ErrorMessage = "Time must be hh:mm:ss (24-hour).")]
    public string? Time { get; set; }  // e.g., "10:30:00" or null
}

public class Traveler
{
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; set; }

    [JsonPropertyName("travelerType")]
    [Required]
    [TravelerTypeValidation]
    [DefaultValue("ADULT")]
    public required string TravelerType { get; set; } = "ADULT";

    [JsonPropertyName("fareOptions")]
    [DefaultValue("STANDARD")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? FareOptions { get; set; } = ["STANDARD"];
}

public class SearchCriteria
{
    [JsonPropertyName("maxFlightOffers")]
    public int MaxFlightOffers { get; set; } = 10;

    [JsonPropertyName("flightFilters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FlightFilters? Filters { get; set; }
}

public class FlightFilters
{
    [JsonPropertyName("cabinRestrictions")]
    public List<CabinRestriction>? CabinRestrictions { get; set; }

    [JsonPropertyName("carrierRestrictions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CarrierRestriction? CarrierRestrictions { get; set; }
}

public class CabinRestriction
{
    [JsonPropertyName("cabin")]
    [CabinTypeValidation]
    [DefaultValue("ECONOMY")]
    public required string Cabin { get; set; } = "ECONOMY";

    [JsonPropertyName("coverage")]
    [CoverageTypeValidation]
    [DefaultValue("MOST_SEGMENTS")]
    public required string Coverage { get; set; } = "MOST_SEGMENTS";

    [JsonPropertyName("originDestinationIds")]
    public List<string> OriginDestinationIds { get; set; } = [ "1" ];
}

public class CarrierRestriction
{
    [JsonPropertyName("blacklistedInEUAllowed")]
    public bool BlacklistedInEUAllowed { get; set; } = false;

    [JsonPropertyName("excludedCarrierCodes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? ExcludedCarrierCodes { get; set; }

    [JsonPropertyName("includedCarrierCodes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? IncludedCarrierCodes { get; set; }
}
