using Cinturon360.Shared.Contracts.Geography;
using Cinturon360.Web.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Cinturon360.Web.Infrastructure.Http.Geography;

public sealed class GeographyApiClient
{
    private readonly HttpClient _http;

    public GeographyApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<GeographyUnifiedDto.GeographyAggregate>
        GetAggregateAsync(
            string rid,
            CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/v1/geography"
        );

        request.Headers.Add("X-Correlation-Id", rid);

        var response = await _http.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var problem = await response.Content
                .ReadFromJsonAsync<ProblemDetails>(ct);

            throw new ApiException(problem, response.StatusCode, rid);
        }

        return await response.Content
            .ReadFromJsonAsync<GeographyUnifiedDto.GeographyAggregate>(ct)
            ?? throw new InvalidOperationException(
                "Failed to deserialize GeographyUnifiedDto.GeographyAggregate.");
    }
}
