using System.ComponentModel.DataAnnotations;
using BlazorLoginDemo.Shared.Models.ExternalLib.Amadeus.Flight;
using NanoidDotNet;

namespace BlazorLoginDemo.Shared.Models.ExternalLib.Kernel.Flight;

public sealed class FlightOfferSearchResultRecord
{
    [Key]
    public string Id { get; set; } = default!;

    [Required]
    public required int MetaCount { get; set; } = default!;

    [Required]
    public required string FlightOfferSearchRequestDtoId { get; set; } = default!;

    [Required]
    public required string ClientId { get; set; } = default!;

    [Required]
    public required string AvaUserId { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public SearchResultSource Source { get; set; } = SearchResultSource.Unknown;

    public AmadeusFlightOfferSearchResult? AmadeusPayload { get; set; }
}

public enum SearchResultSource
{
    Unknown = 0,
    Amadeus = 1
}