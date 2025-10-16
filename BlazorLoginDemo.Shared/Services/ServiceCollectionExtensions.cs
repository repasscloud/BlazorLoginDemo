using System.Text.Json;
using BlazorLoginDemo.Shared.Data;
using BlazorLoginDemo.Shared.Models.ExternalLib.Amadeus;
using BlazorLoginDemo.Shared.Security;
using BlazorLoginDemo.Shared.Services.External;
using BlazorLoginDemo.Shared.Services.Interfaces.External;
using BlazorLoginDemo.Shared.Services.Interfaces.Kernel;
using BlazorLoginDemo.Shared.Services.Interfaces.Platform;
using BlazorLoginDemo.Shared.Services.Interfaces.Policies;
using BlazorLoginDemo.Shared.Services.Interfaces.Policy;
using BlazorLoginDemo.Shared.Services.Interfaces.Travel;
using BlazorLoginDemo.Shared.Services.Kernel;
using BlazorLoginDemo.Shared.Services.Platform;
using BlazorLoginDemo.Shared.Services.Policies;
using BlazorLoginDemo.Shared.Services.Policy;
using BlazorLoginDemo.Shared.Services.Travel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        // --- infra ---
        services.AddHttpClient();
        services.AddSingleton(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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

        // --- shared services ---
        services.AddScoped<ILoggerService, LoggerService>();
        services.AddScoped<IAmadeusAuthService, AmadeusAuthService>();
        services.AddScoped<IAmadeusFlightSearchService, AmadeusFlightSearchService>();
        services.AddScoped<IAirportInfoService, AirportInfoService>();
        services.AddScoped<RequireApiKeyFilter>();

        // --- kernel services ---
        services.AddScoped<IAdminOrgServiceUnified, AdminOrgServiceUnified>();
        services.AddScoped<IAdminUserServiceUnified, AdminUserServiceUnified>();
        services.AddScoped<ITravelQuoteService, TravelQuoteService>();
        services.AddScoped<IErrorCodeService, ErrorCodeService>();
        services.AddScoped<IAdminLicenseAgreementServiceUnified, AdminLicenseAgreementServiceUnified>();

        // --- geographic services ---
        services.AddScoped<IRegionService, RegionService>();
        services.AddScoped<IContinentService, ContinentService>();
        services.AddScoped<ICountryService, CountryService>();

        return services;
    }
}