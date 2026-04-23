using Cinturon360.Web.Exceptions;
using Microsoft.AspNetCore.Mvc;
using static Cinturon360.Shared.Contracts.Organizations.OrganizationUnifiedDto;

namespace Cinturon360.Web.Infrastructure.Http.Organization;

public sealed class OrganizationApiClient
{
    private readonly HttpClient _http;

    public OrganizationApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<OrganizationNoResponseAggregate> CreateAsync<TRequest>(
        string rid,
        TRequest payload,
        CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/v1/organizations"
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
            .ReadFromJsonAsync<OrganizationNoResponseAggregate>(ct)
            ?? throw new InvalidOperationException(
                "Failed to deserialize OrganizationNoResponseAggregate.");
    }

    // public async Task<OrganizationNoResponseAggregate> UpdateAsync<TRequest>(
    //     string rid,
    //     string policyId,
    //     TRequest payload,
    //     CancellationToken ct = default)
    // {
    //     using var request = new HttpRequestMessage(
    //         HttpMethod.Put,
    //         $"/v1/policies/travel/{policyId}"
    //     )
    //     {
    //         Content = JsonContent.Create(payload)
    //     };

    //     request.Headers.Add("X-Correlation-Id", rid);

    //     var response = await _http.SendAsync(request, ct);

    //     if (!response.IsSuccessStatusCode)
    //     {
    //         var problem = await response.Content
    //             .ReadFromJsonAsync<ProblemDetails>(ct);

    //         throw new ApiException(problem, response.StatusCode, rid);
    //     }

    //     return await response.Content
    //         .ReadFromJsonAsync<TravelPolicyNoResponseAggregate>(ct)
    //         ?? throw new InvalidOperationException(
    //             "Failed to deserialize TravelPolicyNoResponseAggregate.");
    // }

    public async Task<ListOrganizationItemsAggregate> GetOrganizationsAsync(
        string rid,
        CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v1/organizations");

        request.Headers.Add("X-Correlation-Id", rid);

        var response = await _http.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var problem = await response.Content
                .ReadFromJsonAsync<ProblemDetails>(ct);

            throw new ApiException(problem, response.StatusCode, rid);
        }

        return await response.Content
            .ReadFromJsonAsync<ListOrganizationItemsAggregate>(ct)
            ?? throw new InvalidOperationException(
                "Failed to deserialize ListOrganizationItemsAggregate.");
    }
}
