using Cinturon360.Web.Exceptions;
using Microsoft.AspNetCore.Mvc;
using static Cinturon360.Shared.Contracts.Common.AirlineDataUnifiedDto;

namespace Cinturon360.Web.Infrastructure.Http.Policy.Travel;

public sealed class AirlineDataProviderApiClient
{
    private readonly HttpClient _http;

    public AirlineDataProviderApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<AirlineWebUILiteAggregate>
    GetAirlinesAsync(
        string rid,
        CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/v1/airlines/lite"
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
            .ReadFromJsonAsync<AirlineWebUILiteAggregate>(ct)
            ?? throw new InvalidOperationException(
                "Failed to deserialize AirlineWebUILiteAggregate.");
    }
}
