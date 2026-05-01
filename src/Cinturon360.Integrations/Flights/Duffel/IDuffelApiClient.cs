namespace Cinturon360.Integrations.Flights.Duffel;

/// <summary>
/// Thin HTTP wrapper for Duffel REST calls.
/// Request payloads are passed as raw JSON so this can support all Duffel endpoints incrementally.
/// </summary>
public interface IDuffelApiClient
{
    Task<string> GetAsync(string apiBaseUrl, string apiToken, string relativePath, CancellationToken ct = default);
    Task<string> PostAsync(string apiBaseUrl, string apiToken, string relativePath, string jsonBody, CancellationToken ct = default);
    Task<string> DeleteAsync(string apiBaseUrl, string apiToken, string relativePath, CancellationToken ct = default);
}
