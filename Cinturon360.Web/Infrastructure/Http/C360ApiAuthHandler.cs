using Microsoft.Extensions.Options;

namespace Cinturon360.Web.Infrastructure.Http;

public sealed class C360ApiAuthHandler : DelegatingHandler
{
    private readonly IOptions<C360ApiAuthOptions> _options;

    public C360ApiAuthHandler(IOptions<C360ApiAuthOptions> options)
    {
        _options = options;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.TryAddWithoutValidation(
            _options.Value.HeaderName,
            _options.Value.Key);

        return base.SendAsync(request, cancellationToken);
    }
}
