namespace Cinturon360.Shared.Models.ExternalLib.Amadeus;

public sealed class AmadeusUrlSettings
{
    /// <summary>
    /// Base API endpoint, e.g. https://test.api.amadeus.com or https://api.amadeus.com
    /// </summary>
    public string ApiEndpoint { get; init; } = string.Empty;

    /// <summary>
    /// Optional: a specific resource path you previously stored (legacy).
    /// Prefer not to store individual endpoints here long-term; put them in the client as constants.
    /// </summary>
    public string FlightOffer { get; init; } = string.Empty;
}
