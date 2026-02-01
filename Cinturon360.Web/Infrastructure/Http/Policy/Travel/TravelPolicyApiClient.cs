using Cinturon360.Shared.Contracts.Geography;
using Cinturon360.Web.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Cinturon360.Web.Infrastructure.Http.Policy.Travel;

public sealed class TravelPolicyApiClient
{
    private readonly HttpClient _http;

    public TravelPolicyApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<TravelPolicyNoResponseAggregate>
    CreateAsync<TRequest>(
        string rid,
        TRequest payload,
        CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/v1/policies/travel"
        )
        {
            Content = JsonContent.Create(payload)
        };

        request.Headers.Add("X-Correlation-Id", rid);

        var response = await _http.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var problem = await response.Content
                .ReadFromJsonAsync<ProblemDetails>(ct);

            throw new ApiException(problem, response.StatusCode, rid);
        }

        return await response.Content
            .ReadFromJsonAsync<PLACEHOLDER_RETURN_VALUE>(ct)
            ?? throw new InvalidOperationException(
                "Failed to deserialize PLACEHOLDER_RETURN_VALUE.");
    }
}
