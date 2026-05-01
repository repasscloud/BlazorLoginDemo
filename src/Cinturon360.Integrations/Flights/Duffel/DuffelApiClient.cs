using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace Cinturon360.Integrations.Flights.Duffel;

public sealed class DuffelApiClient(
    IHttpClientFactory httpClientFactory,
    ILogger<DuffelApiClient> logger) : IDuffelApiClient
{
    public Task<string> GetAsync(string apiBaseUrl, string apiToken, string relativePath, CancellationToken ct = default)
        => SendAsync(HttpMethod.Get, apiBaseUrl, apiToken, relativePath, null, ct);

    public Task<string> PostAsync(string apiBaseUrl, string apiToken, string relativePath, string jsonBody, CancellationToken ct = default)
        => SendAsync(HttpMethod.Post, apiBaseUrl, apiToken, relativePath, jsonBody, ct);

    public Task<string> DeleteAsync(string apiBaseUrl, string apiToken, string relativePath, CancellationToken ct = default)
        => SendAsync(HttpMethod.Delete, apiBaseUrl, apiToken, relativePath, null, ct);

    private async Task<string> SendAsync(
        HttpMethod method,
        string apiBaseUrl,
        string apiToken,
        string relativePath,
        string? jsonBody,
        CancellationToken ct)
    {
        using var client = httpClientFactory.CreateClient("DuffelApi");
        using var request = new HttpRequestMessage(method, new Uri(new Uri(NormalizeBaseUrl(apiBaseUrl)), relativePath.TrimStart('/')));

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
        request.Headers.Add("Duffel-Version", "v2");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (jsonBody is not null)
            request.Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");

        var response = await client.SendAsync(request, ct);
        var payload = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "EVT=INT_CALL_END CAT=INT ACT=EXEC OUT=ERR PROV=DUFFEL ST={StatusCode} PATH={Path}",
                (int)response.StatusCode,
                relativePath);
        }

        response.EnsureSuccessStatusCode();
        return payload;
    }

    private static string NormalizeBaseUrl(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            return "https://api.duffel.com/";

        var trimmed = baseUrl.Trim();
        return trimmed.EndsWith('/') ? trimmed : $"{trimmed}/";
    }
}
