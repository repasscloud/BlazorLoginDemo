using BlazorLoginDemo.Shared.Services.Client;
using BlazorLoginDemo.Shared.Services.Interfaces.Client;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorLoginDemo.Shared.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAvaClientServices(this IServiceCollection services)
    {
        services.AddScoped<IAvaClientService, AvaClientService>();
        services.AddScoped<IAvaClientLicenseService, AvaClientLicenseService>();
        services.AddScoped<ILicenseAgreementService, LicenseAgreementService>();
        return services;
    }
}