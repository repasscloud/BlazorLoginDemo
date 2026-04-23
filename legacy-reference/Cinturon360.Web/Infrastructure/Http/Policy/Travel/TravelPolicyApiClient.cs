using Cinturon360.Web.Exceptions;
using Microsoft.AspNetCore.Mvc;
using static Cinturon360.Shared.Contracts.Policies.TravelPolicyUnifiedDto;

namespace Cinturon360.Web.Infrastructure.Http.Policy.Travel;

public sealed class TravelPolicyApiClient
{
    private readonly HttpClient _http;

    public TravelPolicyApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<TravelPolicyNoResponseAggregate> CreateAsync<TRequest>(
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
            .ReadFromJsonAsync<TravelPolicyNoResponseAggregate>(ct)
            ?? throw new InvalidOperationException(
                "Failed to deserialize TravelPolicyNoResponseAggregate.");
    }

    public async Task<TravelPolicyNoResponseAggregate> UpdateAsync<TRequest>(
        string rid,
        string policyId,
        TRequest payload,
        CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"/v1/policies/travel/{policyId}"
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
            .ReadFromJsonAsync<TravelPolicyNoResponseAggregate>(ct)
            ?? throw new InvalidOperationException(
                "Failed to deserialize TravelPolicyNoResponseAggregate.");
    }

    public async Task<ListTravelPolicyItemsAggregate> GetPoliciesAsync(
        string rid,
        string orgId,
        CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v1/orgs/{orgId}/policies/travel");

        request.Headers.Add("X-Correlation-Id", rid);

        var response = await _http.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var problem = await response.Content
                .ReadFromJsonAsync<ProblemDetails>(ct);

            throw new ApiException(problem, response.StatusCode, rid);
        }

        return await response.Content
            .ReadFromJsonAsync<ListTravelPolicyItemsAggregate>(ct)
            ?? throw new InvalidOperationException(
                "Failed to deserialize ListTravelPolicyItemsAggregate.");
    }
}
