using Cinturon360.Web.Infrastructure.Http.Geography;
using Cinturon360.Web.Infrastructure.Http.Organization;
using Cinturon360.Web.Infrastructure.Http.Policy.Travel;

namespace Cinturon360.Web.Infrastructure.Http;

public sealed class C360ApiClient
{
    private readonly HttpClient _http;

    // ------------------------
    // API groupings
    // ------------------------

    public GeographyApiClient Geography { get; }
    public TravelPolicyApiClient TravelPolicy { get; }
    public AirlineDataProviderApiClient AirlineDataProvider { get; }
    public RailDataProviderApiClient RailDataProvider { get; }

    public OrganizationApiClient Organization { get; }

    // ------------------------
    // Construction
    // ------------------------

    public C360ApiClient(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("C360Api");

        Geography = new GeographyApiClient(_http);
        TravelPolicy = new TravelPolicyApiClient(_http);
        RailDataProvider = new RailDataProviderApiClient(_http);
        AirlineDataProvider = new AirlineDataProviderApiClient(_http);
        Organization = new OrganizationApiClient(_http);
    }

    // ------------------------
    // Low-level access (escape hatch)
    // ------------------------

    public Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken ct = default)
        => _http.SendAsync(request, ct);
}
