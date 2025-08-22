using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BlazorLoginDemo.Shared.Validation;

namespace BlazorLoginDemo.Shared.Models.ExternalLib.Amadeus.Flight;

public class AmadeusFlightOfferSearchResult
{
    // this was added for debug purposes
    [JsonPropertyName("ErrorMessage")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("meta")]
    [Required]
    public required Meta Meta { get; set; }

    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<FlightOffer>? Data { get; set; }

    [JsonPropertyName("dictionaries")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionaries? Dictionaries { get; set; }
}

public class Meta
{
    [JsonPropertyName("count")]
    [Required]
    public required int Count { get; set; }

    [JsonPropertyName("links")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Links { get; set; }
}

public class FlightOffer
{
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; set; }

    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; set; }

    [JsonPropertyName("source")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Source { get; set; }

    [JsonPropertyName("instantTicketingRequired")]
    public bool InstantTicketingRequired { get; set; } = false;

    [JsonPropertyName("nonHomogeneous")]
    public bool nonHomogeneous { get; set; } = false;

    [JsonPropertyName("oneWay")]
    public bool OneWay { get; set; } = false;

    [JsonPropertyName("isUpsellOffer")]
    public bool IsUpsellOffer { get; set; } = false;

    [JsonPropertyName("lastTicketingDate")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Departure date must be in the format YYYY-MM-DD.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LastTicketingDate { get; set; }

    [JsonPropertyName("lastTicketingDateTime")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Departure date must be in the format YYYY-MM-DD.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LastTicketingDateTime { get; set; }

    [JsonPropertyName("numberOfBookableSeats")]
    public int NumberOfBookableSeats { get; set; }

    // itinerary goes below here
    [JsonPropertyName("itineraries")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Itinerary>? Itineraries { get; set; }

    [JsonPropertyName("price")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Price? Price { get; set; }

    [JsonPropertyName("pricingOptions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PricingOption? PricingOptions { get; set; }

    [JsonPropertyName("validatingAirlineCodes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? ValidatingAirlineCodes { get; set; }

    [JsonPropertyName("travelerPricings")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<TravelerPricing>? TravelerPricings { get; set; }
}

public class Itinerary
{
    [JsonPropertyName("duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Duration { get; set; }

    [JsonPropertyName("segments")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Segment>? Segments { get; set; }
}

public class Segment
{
    [JsonPropertyName("departure")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SegmentDeparture? Departure { get; set; }

    [JsonPropertyName("arrival")]
    public SegmentArrival? Arrival { get; set; }

    [JsonPropertyName("carrierCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CarrierCode { get; set; }

    [JsonPropertyName("number")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Number { get; set; }

    [JsonPropertyName("aircraft")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SegmentAircraft? Aircraft { get; set; }

    [JsonPropertyName("operating")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SegmentOperating? Operating { get; set; }

    [JsonPropertyName("duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? duration { get; set; }

    [JsonPropertyName("stops")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<SegmentStop>? Stops { get; set; }

    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; set; }

    [JsonPropertyName("numberOfStops")]
    public int numberOfStops { get; set; }

    [JsonPropertyName("blacklistedInEU")]
    public bool BlacklistedInEU { get; set; } = false;
}

public class SegmentDeparture
{
    [JsonPropertyName("iataCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IATACode { get; set; }

    [JsonPropertyName("terminal")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Terminal { get; set; }

    [JsonPropertyName("at")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}$", ErrorMessage = "Datetime must be in the format YYYY-MM-DDTHH:mm:ss.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? At { get; set; }
}

public class SegmentArrival
{
    [JsonPropertyName("iataCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IATACode { get; set; }

    [JsonPropertyName("terminal")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Terminal { get; set; }

    [JsonPropertyName("at")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}$", ErrorMessage = "Datetime must be in the format YYYY-MM-DDTHH:mm:ss.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? At { get; set; }
}

public class SegmentAircraft
{
    [JsonPropertyName("code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Code { get; set; }
}

public class SegmentOperating
{
    [JsonPropertyName("carrierCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CarrierCode { get; set; }
}

public class SegmentStop
{
    [JsonPropertyName("iataCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IATACode { get; set; }

    [JsonPropertyName("duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Duration { get; set; }

    [JsonPropertyName("arrivalAt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ArrivalAt { get; set; }

    [JsonPropertyName("departureAt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DepartureAt { get; set; }
}

public class Price
{
    [JsonPropertyName("currency")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Currency { get; set; }

    [JsonPropertyName("total")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Total { get; set; }

    [JsonPropertyName("base")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Base { get; set; }

    [JsonPropertyName("fees")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<PriceFee>? Fees { get; set; }

    [JsonPropertyName("grandTotal")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? GrandTotal { get; set; }
}

public class PriceFee
{
    [JsonPropertyName("amount")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Amount { get; set; }

    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; set; }
}

public class PricingOption
{
    [JsonPropertyName("fareType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? FareType { get; set; }

    [JsonPropertyName("includedCheckedBagsOnly")]
    public bool IncludedCheckedBagsOnly { get; set; } = false;
}

public class TravelerPricing
{
    [JsonPropertyName("travelerId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TravelerId { get; set; }

    [JsonPropertyName("fareOption")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FareOption { get; set; }

    [JsonPropertyName("travelerType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TravelerType { get; set; }

    [JsonPropertyName("price")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TravelerPricingPrice? Price { get; set; }

    [JsonPropertyName("fareDetailsBySegment")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<FareDetailBySegment>? FareDetailsBySegment { get; set; }
}

public class TravelerPricingPrice
{
    [JsonPropertyName("currency")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Currency { get; set; }

    [JsonPropertyName("total")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Total { get; set; }

    [JsonPropertyName("base")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Base { get; set; }
}

public class FareDetailBySegment
{
    [JsonPropertyName("segmentId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SegmentId { get; set; }

    [JsonPropertyName("cabin")]
    [CabinTypeValidation]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Cabin { get; set; }

    [JsonPropertyName("fareBasis")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FareBasis { get; set; }

    [JsonPropertyName("brandedFare")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BrandedFare { get; set; }

    [JsonPropertyName("brandedFareLabel")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BrandedFareLabel { get; set; }

    [JsonPropertyName("class")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Class { get; set; }

    [JsonPropertyName("includedCheckedBags")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FareDetailBySegmentIncludedCheckedBags? IncludedCheckedBags { get; set; }

    [JsonPropertyName("amenities")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<FareDetailBySegmentAmenity>? Amenities { get; set; }
}

public class FareDetailBySegmentIncludedCheckedBags
{
    [JsonPropertyName("weight")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Weight { get; set; }

    [JsonPropertyName("weightUnit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? WeightUnit { get; set; }

    [JsonPropertyName("quantity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Quantity { get; set; }
}

public class FareDetailBySegmentAmenity
{
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    [JsonPropertyName("isChargeable")]
    public bool IsChargeable { get; set; } = false;

    [JsonPropertyName("amenityType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AmenityType { get; set; }

    [JsonPropertyName("amenityProvider")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FareDetailBySegmentAmenityAmenityProvider? AmenityProvider { get; set; }
}

public class FareDetailBySegmentAmenityAmenityProvider
{
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }
}

public class Dictionaries
{
    [JsonPropertyName("locations")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, DictionariesLocation>? Locations { get; set; }

    [JsonPropertyName("aircraft")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Aircraft { get; set; }

    [JsonPropertyName("currencies")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Currencies { get; set; }

    [JsonPropertyName("carriers")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Carriers { get; set; }
}

public class DictionariesLocation
{
    [JsonPropertyName("cityCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CityCode { get; set; }

    [JsonPropertyName("countryCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CountryCode { get; set; }
}
