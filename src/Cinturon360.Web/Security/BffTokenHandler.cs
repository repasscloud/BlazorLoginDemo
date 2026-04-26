using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Cinturon360.Web.Security;

/// <summary>
/// DelegatingHandler that attaches the current user's access token
/// to outbound API requests. Token is read server-side from the BFF
/// auth ticket — the browser never sees it.
/// </summary>
public sealed class BffTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<BffTokenHandler> _logger;

    public BffTokenHandler(IHttpContextAccessor httpContextAccessor, ILogger<BffTokenHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is not null)
        {
            var authResult = await httpContext.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            if (authResult.Succeeded && authResult.Principal is not null)
            {
                var token = authResult.Principal.FindFirstValue(ClaimTypes.AccessToken);

                if (!string.IsNullOrWhiteSpace(token))
                {
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }
            }
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning(
                "API returned 401 Unauthorized for {Method} {Uri}. Session may have expired.",
                request.Method, request.RequestUri);
        }

        return response;
    }
}
