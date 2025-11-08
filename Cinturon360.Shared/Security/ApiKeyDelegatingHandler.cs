using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Cinturon360.Shared.Security;

public sealed class ApiKeyDelegatingHandler : DelegatingHandler
{
    private readonly OutboundApiKeyOptions _opts;

    public ApiKeyDelegatingHandler(IOptions<OutboundApiKeyOptions> opts)
        => _opts = opts.Value;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        // Don’t duplicate if caller already set it for some reason
        if (!request.Headers.Contains(_opts.HeaderName) && !string.IsNullOrEmpty(_opts.Key))
        {
            request.Headers.TryAddWithoutValidation(_opts.HeaderName, _opts.Key);
        }
        return base.SendAsync(request, ct);
    }
}
