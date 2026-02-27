using System.Text.Json;
using Cinturon360.Shared.Models.ExternalLib.Amadeus;
using Cinturon360.Shared.Services.Interfaces.External;

namespace Cinturon360.Shared.Services.External;

public sealed class AmadeusConnectionTestService : IAmadeusConnectionTestService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAmadeusAccountStore _accountStore;
    private readonly JsonSerializerOptions _jsonOptions;

    public AmadeusConnectionTestService(
        IHttpClientFactory httpClientFactory,
        IAmadeusAccountStore accountStore,
        JsonSerializerOptions jsonOptions)
    {
        _httpClientFactory = httpClientFactory;
        _accountStore = accountStore;
        _jsonOptions = jsonOptions;
    }

    public async Task<AmadeusConnectionTestResult> TestAsync(
        string tmcId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tmcId))
            throw new ArgumentNullException(nameof(tmcId));

        var account = await _accountStore.GetByTmcIdAsync(tmcId);

        var tokenEndpoint =
            $"{account.Url.ApiEndpoint}/v1/security/oauth2/token";

        var requestData = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = account.ClientId,
            ["client_secret"] = account.ClientSecret
        };

        using var httpClient = _httpClientFactory.CreateClient();
        using var content = new FormUrlEncodedContent(requestData);

        var response = await httpClient.PostAsync(tokenEndpoint, content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            return new AmadeusConnectionTestResult
            {
                Success = false,
                Message = "OAuth authentication failed",
                Details = body
            };
        }

        var payload = await response.Content.ReadAsStringAsync(ct);
        var oauth =
            JsonSerializer.Deserialize<AmadeusOAuthResponse>(payload, _jsonOptions);

        if (oauth is null || string.IsNullOrEmpty(oauth.AccessToken))
        {
            return new AmadeusConnectionTestResult
            {
                Success = false,
                Message = "Invalid OAuth response"
            };
        }

        return new AmadeusConnectionTestResult
        {
            Success = true,
            Message = "Successfully authenticated with Amadeus"
        };
    }
}
