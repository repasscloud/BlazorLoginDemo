using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Cinturon360.Shared.Validation;

namespace Cinturon360.Shared.Models.DTOs;

public class FlightOfferSearchRequestDto
{
    [Key]
    [Required]
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("clientId")]
    [Required]
    public required string ClientId { get; set; }

    // this is the AspNetUsersId (guid) value
    [JsonPropertyName("customerId")]
    [Required]
    public required string CustomerId { get; set; }

    [JsonPropertyName("travelPolicyId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    // [RegularExpression(@"^[0-9A-F]{10}$", ErrorMessage = "TravelPolicyId must be exactly 10 uppercase hexadecimal characters (0-9, A-F).")]
    [DefaultValue(null)]
    public string? TravelPolicyId { get; set; }

    [JsonPropertyName("originLocationCode")]
    [Required]
    [RegularExpression(@"^[A-Z0-9]{3}$", ErrorMessage = "Origin location code must be exactly 3 uppercase alphanumeric characters (A-Z, 0-9).")]
    public required string OriginLocationCode { get; set; }

    [JsonPropertyName("destinationLocationCode")]
    [Required]
    [RegularExpression(@"^[A-Z0-9]{3}$", ErrorMessage = "Destination location code must be exactly 3 uppercase alphanumeric characters (A-Z, 0-9).")]
    public required string DestinationLocationCode { get; set; }

    [JsonPropertyName("isOneWay")]
    public required bool IsOneWay { get; set; } = false;

    [JsonPropertyName("departureDate")]
    [Required]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Departure date must be in the format YYYY-MM-DD.")]
    public required string DepartureDate { get; set; }

    [JsonPropertyName("departureDateReturn")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Departure date must be in the format YYYY-MM-DD.")]
    [DefaultValue(null)]
    public string? DepartureDateReturn { get; set; }

    [JsonPropertyName("adults")]
    [Range(minimum: 1, maximum: 9, ErrorMessage = "Adults must be a single digit between 1 and 9.")]
    public required int Adults { get; set; }

    [JsonPropertyName("cabinClass")]
    [Required]
    [CabinTypeValidation]
    [DefaultValue("ECONOMY")]
    public required string CabinClass { get; set; } = "ECONOMY";
}
