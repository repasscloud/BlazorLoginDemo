using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Cinturon360.Web.Services.Session;

/// <summary>
/// Manages creating and destroying the BFF session cookie.
/// Called by auth pages (Login, Logout).
/// </summary>
public sealed class UserSessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserSessionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task SignInAsync(
        string userId,
        string orgId,
        string email,
        string displayName,
        string userCategory,
        string platformRole,
        string accessToken,
        string refreshToken,
        DateTimeOffset tokenExpiry,
        string theme = "system",
        string language = "en-AU",
        string timeZone = "Australia/Sydney")
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No active HttpContext.");

        var claims = new List<Claim>
        {
            new(Security.ClaimTypes.UserId,       userId),
            new(Security.ClaimTypes.OrgId,        orgId),
            new(Security.ClaimTypes.Email,        email),
            new(Security.ClaimTypes.DisplayName,  displayName),
            new(Security.ClaimTypes.UserCategory, userCategory),
            new(Security.ClaimTypes.PlatformRole, platformRole),
            new(Security.ClaimTypes.AccessToken,  accessToken),
            new(Security.ClaimTypes.RefreshToken, refreshToken),
            new(Security.ClaimTypes.TokenExpiry,  tokenExpiry.ToString("O")),
            new(Security.ClaimTypes.Theme,        theme),
            new(Security.ClaimTypes.Language,     language),
            new(Security.ClaimTypes.TimeZone,     timeZone),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = tokenExpiry,
                AllowRefresh = true,
            });
    }

    public async Task SignOutAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No active HttpContext.");

        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
