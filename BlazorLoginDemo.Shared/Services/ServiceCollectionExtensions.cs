using System.Text.Json;
using BlazorLoginDemo.Shared.Data;
using BlazorLoginDemo.Shared.Models.ExternalLib.Amadeus;
using BlazorLoginDemo.Shared.Models.Kernel.FX;
using BlazorLoginDemo.Shared.Models.Kernel.Travel;
using BlazorLoginDemo.Shared.Security;
using BlazorLoginDemo.Shared.Services.External;
using BlazorLoginDemo.Shared.Services.Interfaces.External;
using BlazorLoginDemo.Shared.Services.Interfaces.Kernel;
using BlazorLoginDemo.Shared.Services.Interfaces.Persistence;
using BlazorLoginDemo.Shared.Services.Interfaces.Platform;
using BlazorLoginDemo.Shared.Services.Interfaces.Policies;
using BlazorLoginDemo.Shared.Services.Interfaces.Policy;
using BlazorLoginDemo.Shared.Services.Interfaces.Travel;
using BlazorLoginDemo.Shared.Services.Kernel;
using BlazorLoginDemo.Shared.Services.Persistence;
using BlazorLoginDemo.Shared.Services.Platform;
using BlazorLoginDemo.Shared.Services.Policies;
using BlazorLoginDemo.Shared.Services.Policy;
using BlazorLoginDemo.Shared.Services.Travel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BlazorLoginDemo.Shared.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAvaApiHttpClient(
        this IServiceCollection services, IConfiguration config)
    {
        // Options for outbound header
        services.AddOptions<OutboundApiKeyOptions>()
            .Bind(config.GetSection("OutboundApiKeyAuth"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.HeaderName) &&
                           !string.IsNullOrWhiteSpace(o.Key),
                      "OutboundApiKeyAuth: HeaderName and Key must be set.")
            .ValidateOnStart();

        services.AddTransient<ApiKeyDelegatingHandler>();

        services.AddHttpClient("AvaApi", c =>
        {
            var baseAddress = config["Api:BaseAddress"];
            if (!string.IsNullOrWhiteSpace(baseAddress))
                c.BaseAddress = new Uri(baseAddress);
        })
        .AddHttpMessageHandler<ApiKeyDelegatingHandler>();

        return services;
    }

    public static IServiceCollection PlatformServices(this IServiceCollection services)
    {
        services.AddScoped<IAdminOrgServiceUnified, AdminOrgServiceUnified>();
        services.AddScoped<IAdminUserServiceUnified, AdminUserServiceUnified>();
        services.AddScoped<IAdminLicenseAgreementServiceUnified, AdminLicenseAgreementServiceUnified>();
        services.AddScoped<IErrorCodeService, ErrorCodeService>();
        services.AddScoped<IBillingService, BillingService>();
        return services;
    }
    
    public static IServiceCollection AddAvaClientServices(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddAvaFinanceServices(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddAvaPolicyServices(this IServiceCollection services)
    {
        services.AddScoped<ILoggerService, LoggerService>();
        services.AddScoped<ITravelPolicyService, TravelPolicyService>();
        services.AddScoped<IRegionService, RegionService>();
        services.AddScoped<IContinentService, ContinentService>();
        services.AddScoped<ICountryService, CountryService>();
        services.AddScoped<IAirportInfoService, AirportInfoService>();
        return services;
    }

    // exclusive use by the .api project
    public static IServiceCollection AddApiLibServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        // --- options ---
        services.AddOptions<AmadeusOAuthClientSettings>()
            .Bind(config.GetSection("Amadeus"))
            .ValidateDataAnnotations()
            .Validate(s =>
                !string.IsNullOrWhiteSpace(s.ClientId) &&
                !string.IsNullOrWhiteSpace(s.ClientSecret),
                "Amadeus:ClientId and Amadeus:ClientSecret must be configured.")
            .ValidateOnStart();

        services.AddOptions<InboundApiKeyOptions>()
            .Bind(config.GetSection("InboundApiKeyAuth"))
            .Validate(o =>
                !string.IsNullOrWhiteSpace(o.HeaderName) &&
                o.AllowedKeys is { Count: > 0 } &&
                o.AllowedKeys.All(k => !string.IsNullOrWhiteSpace(k)),
                "InboundAPiKeyAuth must specify HeaderName and at least one non-empty key.")
            .ValidateOnStart();

            // ExchangeRate API options
            services.AddOptions<ExchangeRateApiOptions>()
                .Bind(config.GetSection("ExchangeRateApi"))
                .Validate(o =>
                    !string.IsNullOrWhiteSpace(o.BaseUrl) &&
                    !string.IsNullOrWhiteSpace(o.ApiKey) &&
                    !string.IsNullOrWhiteSpace(o.DefaultBaseCode) &&
                    o.DefaultBaseCode.Length == 3,
                    "ExchangeRateApi: BaseUrl, ApiKey, and 3-letter DefaultBaseCode are required.")
                .ValidateOnStart();

        // --- infra ---
        services.AddHttpClient();
        services.AddMemoryCache(); // <-- needed for IMemoryCache
        services.AddSingleton(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // --- fx http client ---
        services.AddHttpClient("fx", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        // --- Identity (required by AdminUserServiceUnified) ---
        services
            .AddIdentityCore<ApplicationUser>(o =>
            {
                o.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        // --- airline service ---
        services.AddOptions<AirlineIngestionOptions>()
            .Bind(config.GetSection(AirlineIngestionOptions.SectionName))   // "AirlineIngestion"
            .Validate(o =>
                Uri.TryCreate(o.SourceUrl, UriKind.Absolute, out var u) &&
                (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps),
                "SourceUrl must be absolute http/https")
            .Validate(o => o.HttpTimeoutSeconds is > 0 and <= 300, "HttpTimeoutSeconds must be 1-300")
            .ValidateOnStart();

        services.AddHttpClient("airlines")
            .ConfigureHttpClient((sp, c) =>
            {
                var o = sp.GetRequiredService<IOptions<AirlineIngestionOptions>>().Value;
                c.Timeout = TimeSpan.FromSeconds(o.HttpTimeoutSeconds);
            });

        services.AddScoped<IAirlineService, AirlineService>();

        // --- shared services ---
        services.AddScoped<ILoggerService, LoggerService>();
        services.AddScoped<IAmadeusAuthService, AmadeusAuthService>();
        services.AddScoped<IAmadeusFlightSearchService, AmadeusFlightSearchService>();
        services.AddScoped<IAirportInfoService, AirportInfoService>();
        services.AddScoped<RequireApiKeyFilter>();

        // --- kernel services ---
        services.AddScoped<IAdminOrgServiceUnified, AdminOrgServiceUnified>();
        services.AddScoped<IAdminUserServiceUnified, AdminUserServiceUnified>();
        services.AddScoped<ITravelPolicyService, TravelPolicyService>();
        services.AddScoped<ITravelQuoteService, TravelQuoteService>();
        services.AddScoped<IErrorCodeService, ErrorCodeService>();
        services.AddScoped<IAdminLicenseAgreementServiceUnified, AdminLicenseAgreementServiceUnified>();

        // --- fx services ---
        services.AddScoped<IFxRateStore, EfFxRateStore>();
        services.AddScoped<IFxRateService, FxRateService>();

        // --- geographic services ---
        services.AddScoped<IRegionService, RegionService>();
        services.AddScoped<IContinentService, ContinentService>();
        services.AddScoped<ICountryService, CountryService>();

        return services;
    }
}