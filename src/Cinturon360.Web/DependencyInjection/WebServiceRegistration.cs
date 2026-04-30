using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Cinturon360.Web.Security;
using Cinturon360.Web.Services.ApiClients;
using Cinturon360.Web.Services.Navigation;
using Cinturon360.Web.Services.Session;
using Cinturon360.Web.Services.State;

namespace Cinturon360.Web.DependencyInjection;

public static class WebServiceRegistration
{
    public static IServiceCollection AddWebServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── BFF Auth ──────────────────────────────────────────────
        services.AddHttpContextAccessor();

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name        = "c360.bff";
                options.Cookie.HttpOnly    = true;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite    = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                options.ExpireTimeSpan     = TimeSpan.FromDays(1);
                options.SlidingExpiration  = true;
                options.LoginPath          = "/auth/login";
                options.LogoutPath         = "/auth/logout-handler";
                options.AccessDeniedPath   = "/access-denied";
            });

        services.AddAuthorization();

        services.AddCascadingAuthenticationState();
        services.AddScoped<AuthenticationStateProvider, BffAuthStateProvider>();

        // ── Session & State ───────────────────────────────────────
        services.AddScoped<UserSessionService>();
        services.AddScoped<AppState>();
        services.AddScoped<ToastService>();
        services.AddScoped<NavigationPermissionService>();

        // ── BFF Token Handler ─────────────────────────────────────
        services.AddTransient<BffTokenHandler>();

        // ── Typed API Clients ─────────────────────────────────────
        var apiBase = configuration["Api:BaseUrl"]
            ?? throw new InvalidOperationException("Api:BaseUrl is not configured.");

        services
            .AddHttpClient<IdentityApiClient>(c => c.BaseAddress = new Uri(apiBase))
            .AddHttpMessageHandler<BffTokenHandler>()
            .AddStandardResilienceHandler();

        services
            .AddHttpClient<TravellerApiClient>(c => c.BaseAddress = new Uri(apiBase))
            .AddHttpMessageHandler<BffTokenHandler>()
            .AddStandardResilienceHandler();

        services
            .AddHttpClient<BookingApiClient>(c => c.BaseAddress = new Uri(apiBase))
            .AddHttpMessageHandler<BffTokenHandler>()
            .AddStandardResilienceHandler();

        services
            .AddHttpClient<PolicyApiClient>(c => c.BaseAddress = new Uri(apiBase))
            .AddHttpMessageHandler<BffTokenHandler>()
            .AddStandardResilienceHandler();

        services
            .AddHttpClient<ApprovalApiClient>(c => c.BaseAddress = new Uri(apiBase))
            .AddHttpMessageHandler<BffTokenHandler>()
            .AddStandardResilienceHandler();

        services
            .AddHttpClient<BillingApiClient>(c => c.BaseAddress = new Uri(apiBase))
            .AddHttpMessageHandler<BffTokenHandler>()
            .AddStandardResilienceHandler();

        services
            .AddHttpClient<OrgApiClient>(c => c.BaseAddress = new Uri(apiBase))
            .AddHttpMessageHandler<BffTokenHandler>()
            .AddStandardResilienceHandler();

        services
            .AddHttpClient<SystemApiClient>(c => c.BaseAddress = new Uri(apiBase))
            .AddHttpMessageHandler<BffTokenHandler>()
            .AddStandardResilienceHandler();

        services
            .AddHttpClient<FlightApiClient>(c => c.BaseAddress = new Uri(apiBase))
            .AddHttpMessageHandler<BffTokenHandler>()
            .AddStandardResilienceHandler();

        services
            .AddHttpClient<TicketApiClient>(c => c.BaseAddress = new Uri(apiBase))
            .AddHttpMessageHandler<BffTokenHandler>()
            .AddStandardResilienceHandler();

        // ── Localisation ──────────────────────────────────────────
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        return services;
    }
}
