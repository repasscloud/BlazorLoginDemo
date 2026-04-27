using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Cinturon360.Contracts.Auth;
using Cinturon360.Web.Services.ApiClients;
using Cinturon360.Web.Security;

namespace Cinturon360.Web.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login-handler", HandleLoginPost)
            .AllowAnonymous()
                        .DisableAntiforgery()
            .WithName("LoginHandler");

        app.MapPost("/auth/register-handler", HandleRegisterPost)
            .AllowAnonymous()
            .DisableAntiforgery()
            .WithName("RegisterHandler");

        app.MapGet("/auth/logout-handler", HandleLogout)
            .WithName("LogoutHandler");

        return app;
    }

    private static async Task<IResult> HandleLoginPost(
        HttpContext httpContext,
        IdentityApiClient identityApi,
        [FromForm] LoginFormRequest request)
    {
        var apiRequest = new Cinturon360.Contracts.Auth.LoginRequest(
            request.Email,
            request.Password);

        // Call the backend API to authenticate
        var apiResult = await identityApi.LoginAsync(apiRequest);

        if (!apiResult.IsSuccess)
        {
            return Results.Redirect("/auth/login?error=invalid_credentials");
        }

        var authResponse = apiResult.Value;
        if (authResponse is null)
        {
            return Results.Redirect("/auth/login?error=invalid_credentials");
        }

        var user = authResponse.User;
        var tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(authResponse.ExpiresIn);

        // Set up claims for the authenticated user
        var claims = new List<Claim>
        {
            new(Cinturon360.Web.Security.ClaimTypes.UserId, user?.UserId ?? "unknown"),
            new(Cinturon360.Web.Security.ClaimTypes.OrgId, user?.OrgId ?? ""),
            new(Cinturon360.Web.Security.ClaimTypes.Email, user?.Email ?? "unknown@example.com"),
            new(Cinturon360.Web.Security.ClaimTypes.DisplayName, user?.FullName ?? "User"),
            new(Cinturon360.Web.Security.ClaimTypes.UserCategory, user?.UserCategory ?? "Client"),
            new(Cinturon360.Web.Security.ClaimTypes.PlatformRole, user?.PlatformRole ?? "User"),
            new(Cinturon360.Web.Security.ClaimTypes.AccessToken, authResponse.AccessToken ?? ""),
            new(Cinturon360.Web.Security.ClaimTypes.RefreshToken, ""),
            new(Cinturon360.Web.Security.ClaimTypes.TokenExpiry, tokenExpiry.ToString("O")),
            new(Cinturon360.Web.Security.ClaimTypes.SessionId, authResponse.SessionId ?? ""),
            new(Cinturon360.Web.Security.ClaimTypes.Theme, "system"),
            new(Cinturon360.Web.Security.ClaimTypes.Language, "en-AU"),
            new(Cinturon360.Web.Security.ClaimTypes.TimeZone, "Australia/Sydney"),
        };

        var identity = new System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);

        // Sign in the user
        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = tokenExpiry,
                AllowRefresh = true,
            });

        return Results.Redirect("/");
    }

    private static async Task<IResult> HandleLogout(
        HttpContext httpContext,
        IdentityApiClient identityApi)
    {
        // Revoke the session on the API (best-effort — do not block sign-out on failure)
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            var sessionId = httpContext.User.FindFirstValue(Cinturon360.Web.Security.ClaimTypes.SessionId);
            if (!string.IsNullOrEmpty(sessionId))
            {
                try { await identityApi.LogoutAsync(sessionId); }
                catch { /* ignore — cookie is cleared regardless */ }
            }
        }

        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Redirect("/auth/login");
    }

    private static async Task<IResult> HandleRegisterPost(
        HttpContext httpContext,
        IdentityApiClient identityApi,
        [FromForm] RegisterFormRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            return Results.Redirect("/auth/register?error=password_mismatch");

        var apiRequest = new Cinturon360.Contracts.Auth.RegisterRequest(
            FirstName: request.FirstName,
            LastName: request.LastName,
            Email: request.Email,
            Password: request.Password);

        var apiResult = await identityApi.RegisterAsync(apiRequest);

        if (!apiResult.IsSuccess)
        {
            var error = apiResult.Error?.Contains("already") == true
                ? "email_taken"
                : "registration_failed";
            return Results.Redirect($"/auth/register?error={error}");
        }

        var authResponse = apiResult.Value;
        if (authResponse is null)
            return Results.Redirect("/auth/register?error=registration_failed");

        var user = authResponse.User;
        var tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(authResponse.ExpiresIn);

        var claims = new List<Claim>
        {
            new(Cinturon360.Web.Security.ClaimTypes.UserId, user?.UserId ?? "unknown"),
            new(Cinturon360.Web.Security.ClaimTypes.OrgId, user?.OrgId ?? ""),
            new(Cinturon360.Web.Security.ClaimTypes.Email, user?.Email ?? ""),
            new(Cinturon360.Web.Security.ClaimTypes.DisplayName, user?.FullName ?? "User"),
            new(Cinturon360.Web.Security.ClaimTypes.UserCategory, user?.UserCategory ?? "Client"),
            new(Cinturon360.Web.Security.ClaimTypes.PlatformRole, user?.PlatformRole ?? ""),
            new(Cinturon360.Web.Security.ClaimTypes.AccessToken, authResponse.AccessToken ?? ""),
            new(Cinturon360.Web.Security.ClaimTypes.RefreshToken, ""),
            new(Cinturon360.Web.Security.ClaimTypes.TokenExpiry, tokenExpiry.ToString("O")),
            new(Cinturon360.Web.Security.ClaimTypes.SessionId, authResponse.SessionId ?? ""),
            new(Cinturon360.Web.Security.ClaimTypes.Theme, "system"),
            new(Cinturon360.Web.Security.ClaimTypes.Language, "en-AU"),
            new(Cinturon360.Web.Security.ClaimTypes.TimeZone, "Australia/Sydney"),
        };

        var identity = new System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = tokenExpiry,
                AllowRefresh = true,
            });

        return Results.Redirect("/");
    }

    private sealed record LoginFormRequest(string Email, string Password);

    private sealed record RegisterFormRequest(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string ConfirmPassword);
}

