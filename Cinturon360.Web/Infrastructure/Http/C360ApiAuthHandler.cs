using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Cinturon360.Web.Infrastructure.Http;

public sealed class C360ApiAuthHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptions<C360ApiAuthOptions> _options;

    public C360ApiAuthHandler(
        IHttpContextAccessor httpContextAccessor,
        IOptions<C360ApiAuthOptions> options)
    {
        _httpContextAccessor = httpContextAccessor;
        _options = options;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var ctx = _httpContextAccessor.HttpContext;

        if (ctx?.User?.Identity?.IsAuthenticated == true)
        {
            var userId =
                ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                request.Headers.TryAddWithoutValidation(
                    "X-User-Id",
                    userId);
            }
        }

        // Keep your existing API key authentication
        request.Headers.TryAddWithoutValidation(
            _options.Value.HeaderName,
            _options.Value.Key);

        return base.SendAsync(request, cancellationToken);
    }
}