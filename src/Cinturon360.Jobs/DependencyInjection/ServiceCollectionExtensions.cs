using Microsoft.Extensions.DependencyInjection;
using Cinturon360.Jobs.Recurring.DataFeeds;

namespace Cinturon360.Jobs.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJobServices(this IServiceCollection services)
    {
        // ── Recurring data-feed jobs ───────────────────────────────────────
        services.AddHostedService<ExchangeRateSyncJob>();

        return services;
    }
}
