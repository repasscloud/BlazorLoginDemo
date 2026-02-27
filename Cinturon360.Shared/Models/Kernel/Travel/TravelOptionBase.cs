using System.Net;

namespace Cinturon360.Shared.Models.Kernel.Travel;

public abstract class TravelOptionBase
{
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public abstract TravelQuoteType Type { get; }
    public required string QuoteId { get; init; }
    public HttpStatusCode StatusCode { get; init; } = HttpStatusCode.OK;
    public string? Message { get; init; }
}

/// <summary>
/// One provider’s flight results (0..N flights) as a single option
/// </summary>
public sealed class FlightSearchResponse : TravelOptionBase
{
    public override TravelQuoteType Type => TravelQuoteType.flight;
    public ICollection<FlightViewOption> Options { get; init; } = Array.Empty<FlightViewOption>();
    public bool MoreResultsAvailable { get; init; } = false;
}

