using Microsoft.Extensions.DependencyInjection;
using Serilog;
using QuestPDF.Infrastructure;

namespace Cinturon360.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // QuestPDF license — Community license for open-source / evaluation
        QuestPDF.Settings.License = LicenseType.Community;

        // TODO: Register storage services (S3), email, SMS, caching, job queue
        // TODO: Register Serilog enrichers (see Logging/ folder)

        return services;
    }
}
