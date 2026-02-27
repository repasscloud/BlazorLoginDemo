using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Cinturon360.Shared.Data;
using Cinturon360.Shared.Models.ExternalLib.Amadeus;
using Cinturon360.Shared.Services.Interfaces.External;

namespace Cinturon360.Shared.Services.External;

public sealed class AmadeusAuthService : IAmadeusAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApplicationDbContext _context;
    private readonly IAmadeusAccountStore _accountStore;
    private readonly JsonSerializerOptions _jsonOptions;

    public AmadeusAuthService(
        IHttpClientFactory httpClientFactory,
        ApplicationDbContext context,
        IAmadeusAccountStore accountStore,
        JsonSerializerOptions jsonOptions)
    {
        _httpClientFactory = httpClientFactory;
        _context = context;
        _accountStore = accountStore;
        _jsonOptions = jsonOptions;
    }

    public async Task<AmadeusOAuthToken> GetTokenAsync(string tmcId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tmcId);

        var account = await _accountStore.GetByTmcIdAsync(tmcId);

        var oauthContract =
            AmadeusOAuthClientContractFactory.FromAccount(account);

        var tokenEndpoint =
            $"{oauthContract.Url.ApiEndpoint}/v1/security/oauth2/token";

        var requestData = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = oauthContract.ClientId,
            ["client_secret"] = oauthContract.ClientSecret
        };

        using var requestContent = new FormUrlEncodedContent(requestData);
        using var httpClient = _httpClientFactory.CreateClient();

        var response = await httpClient.PostAsync(tokenEndpoint, requestContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Amadeus OAuth failed for TMC {tmcId}. StatusCode={response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        var authResponse =
            JsonSerializer.Deserialize<AmadeusOAuthResponse>(responseContent, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize Amadeus OAuth response.");

        var token = new AmadeusOAuthToken
        {
            TmcId = tmcId,
            TokenType = authResponse.TokenType,
            AccessToken = authResponse.AccessToken,
            ExpiresIn = authResponse.ExpiresIn,
            CreatedAt = DateTime.UtcNow
        };

        _context.AmadeusOAuthTokens.Add(token);
        await _context.SaveChangesAsync();

        return token;
    }

    public async Task<string> GetAccessTokenAsync(string tmcId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tmcId);

        var existingToken = await _context.AmadeusOAuthTokens
            .Where(t => t.TmcId == tmcId)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();

        if (existingToken is not null && existingToken.ExpiryTime > DateTime.UtcNow)
        {
            return existingToken.AccessToken;
        }

        var newToken = await GetTokenAsync(tmcId);
        return newToken.AccessToken;
    }
}
