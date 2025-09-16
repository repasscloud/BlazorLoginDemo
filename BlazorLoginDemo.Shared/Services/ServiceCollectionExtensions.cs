using System.Text.Json;
using BlazorLoginDemo.Shared.Models.ExternalLib.Amadeus;
using BlazorLoginDemo.Shared.Security;
using BlazorLoginDemo.Shared.Services.Client;
using BlazorLoginDemo.Shared.Services.External;
using BlazorLoginDemo.Shared.Services.Finance;
using BlazorLoginDemo.Shared.Services.Interfaces.Client;
using BlazorLoginDemo.Shared.Services.Interfaces.External;
using BlazorLoginDemo.Shared.Services.Interfaces.Finance;
using BlazorLoginDemo.Shared.Services.Interfaces.Kernel;
using BlazorLoginDemo.Shared.Services.Interfaces.Platform;
using BlazorLoginDemo.Shared.Services.Interfaces.Policies;
using BlazorLoginDemo.Shared.Services.Interfaces.Policy;
using BlazorLoginDemo.Shared.Services.Interfaces.User;
using BlazorLoginDemo.Shared.Services.Kernel;
using BlazorLoginDemo.Shared.Services.Platform;
using BlazorLoginDemo.Shared.Services.Policies;
using BlazorLoginDemo.Shared.Services.User;
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

    public static IServiceCollection AddAvaPlatformServices(this IServiceCollection services)
    {
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IAdminOrgService, AdminOrgService>();
        return services;
    }
    
    public static IServiceCollection AddAvaClientServices(this IServiceCollection services)
    {
        services.AddScoped<IAvaUserService, AvaUserService>();
        services.AddScoped<IAvaClientService, AvaClientService>();
        services.AddScoped<IAvaClientLicenseService, AvaClientLicenseService>();
        services.AddScoped<ILicenseAgreementService, LicenseAgreementService>();
        services.AddScoped<IAvaUserSysPreferenceService, AvaUserSysPreferenceService>();
        return services;
    }

    public static IServiceCollection AddAvaFinanceServices(this IServiceCollection services)
    {
        services.AddScoped<ILateFeeConfigService, LateFeeConfigService>();
        return services;
    }

    public static IServiceCollection AddAvaPolicyServices(this IServiceCollection services)
    {
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
        // get settings from host's configuration
        services.AddOptions<AmadeusOAuthClientSettings>()
            .Bind(config.GetSection("Amadeus"))
            .ValidateDataAnnotations()
            .Validate(s =>
                !string.IsNullOrWhiteSpace(s.ClientId) &&
                !string.IsNullOrWhiteSpace(s.ClientSecret),
                "Amadeus:ClientId and Amadeus:ClientSecret must be configured.")
            .ValidateOnStart();  // throws at startup if invalid (nice fail-fast)

        // --- inbound API key options for the WebAPI ---
        services.AddOptions<InboundApiKeyOptions>()
        .Bind(config.GetSection("InboundApiKeyAuth"))
        .Validate(o =>
            !string.IsNullOrWhiteSpace(o.HeaderName) &&
            o.AllowedKeys is { Count: > 0 } &&
            o.AllowedKeys.All(k => !string.IsNullOrWhiteSpace(k)),
            "InboundAPiKeyAuth must specify HeaderName and at least one non-empty key.")
        .ValidateOnStart();

        // infra used by shared services
        services.AddHttpClient();  // for IHttpClientFactory
        services.AddSingleton(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // shared services
        services.AddScoped<ILoggerService, LoggerService>();
        services.AddScoped<IAmadeusAuthService, AmadeusAuthService>();
        services.AddScoped<IAmadeusFlightSearchService, AmadeusFlightSearchService>();
        services.AddScoped<IAirportInfoService, AirportInfoService>();
        services.AddScoped<RequireApiKeyFilter>();
        return services;
    }
}