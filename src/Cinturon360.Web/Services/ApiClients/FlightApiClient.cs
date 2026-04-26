using Cinturon360.Contracts.Travel;

namespace Cinturon360.Web.Services.ApiClients;

public sealed class FlightApiClient : ApiClientBase
{
    public FlightApiClient(HttpClient http, ILogger<FlightApiClient> logger)
        : base(http, logger) { }

    public Task<ApiResult<FlightSearchResponse>> SearchAsync(
        FlightSearchRequest request,
        CancellationToken ct = default)
        => PostAsync<FlightSearchResponse>("api/v1/travel/flights/search", request, ct);
}
