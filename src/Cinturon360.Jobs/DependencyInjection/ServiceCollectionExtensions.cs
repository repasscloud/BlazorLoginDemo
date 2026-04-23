using Microsoft.Extensions.DependencyInjection;

namespace Cinturon360.Jobs.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJobServices(this IServiceCollection services)
    {
        // TODO: Register hosted services for recurring jobs
        // TODO: Register queue processors

        return services;
    }
}
