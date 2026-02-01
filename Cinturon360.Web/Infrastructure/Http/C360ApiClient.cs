using Cinturon360.Web.Infrastructure.Http.Geography;

namespace Cinturon360.Web.Infrastructure.Http;

public sealed class C360ApiClient
{
    private readonly HttpClient _http;

    // ------------------------
    // API groupings
    // ------------------------

    public GeographyApiClient Geography { get; }

    // ------------------------
    // Construction
    // ------------------------

    public C360ApiClient(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("C360Api");

        Geography = new GeographyApiClient(_http);
    }

    // ------------------------
    // Low-level access (escape hatch)
    // ------------------------

    public Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken ct = default)
        => _http.SendAsync(request, ct);
}
