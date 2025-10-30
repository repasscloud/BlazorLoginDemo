using System.ComponentModel.DataAnnotations;
using Cinturon360.Shared.Models.Search;
using Cinturon360.Shared.Models.Static.Travel;

namespace Cinturon360.Shared.Models.DTOs;

public sealed class TravelQuoteFlightUIResultPatchDto
{
    public string Id { get; set; } = default!;

    // Customer Query Data
    public TripType? TripType { get; set; }
    [MaxLength(3)] public string? OriginIataCode { get; set; }
    [MaxLength(3)] public string? DestinationIataCode { get; set; }
    [MaxLength(10)] public string? DepartureDate { get; set; }
    [MaxLength(10)] public string? ReturnDate { get; set; }
    [MaxLength(8)] public string? DepartEarliestTime { get; set; }
    [MaxLength(8)] public string? DepartLatestTime { get; set; }
    [MaxLength(8)] public string? ReturnEarliestTime { get; set; }
    [MaxLength(8)] public string? ReturnLatestTime { get; set; }
    public CabinClass? CabinClass { get; set; }
    public CabinClass? MaxCabinClass { get; set; }
    public string[] SelectedAirlines { get; set; } = Array.Empty<string>();
    public List<AirlineAlliance>? Alliances { get; set; }
}