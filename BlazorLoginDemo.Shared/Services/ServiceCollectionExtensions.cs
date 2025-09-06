using BlazorLoginDemo.Shared.Services.Client;
using BlazorLoginDemo.Shared.Services.Finance;
using BlazorLoginDemo.Shared.Services.Interfaces.Client;
using BlazorLoginDemo.Shared.Services.Interfaces.Finance;
using BlazorLoginDemo.Shared.Services.Interfaces.Policy;
using BlazorLoginDemo.Shared.Services.Interfaces.User;
using BlazorLoginDemo.Shared.Services.Policies;
using BlazorLoginDemo.Shared.Services.User;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorLoginDemo.Shared.Services;

public static class ServiceCollectionExtensions
{
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
        return services;
    }
}