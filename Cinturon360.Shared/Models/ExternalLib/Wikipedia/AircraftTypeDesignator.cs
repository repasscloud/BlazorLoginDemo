using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Cinturon360.Shared.Models.ExternalLib.Wikipedia;

public class AircraftTypeDesignator
{
    // Primary Key
    [Key]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    // Required by DB and API
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("icao_code")]
    public string? ICAOCode { get; set; }

    // Optional for DB, always serialized (even if empty)
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("iata_type_code")]
    public string? IATATypeCode { get; set; }

    // Required by DB and API
    [Required]
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    // Optional for DB, only serialized when not null
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("wikipedia_link")]
    public string? WikipediaLink { get; set; }
}
