using Microsoft.Extensions.DependencyInjection;

namespace Cinturon360.Integrations.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIntegrationServices(this IServiceCollection services)
    {
        // TODO: Register typed HttpClients with Polly resilience for:
        //   - Flight search providers (Amadeus, Duffel)
        //   - Hotel search providers
        //   - Stripe payment client
        //   - Exchange rate provider
        //   - GitHub Issues (ticketing)
        //   - AWS S3 client
        // TODO: Register OIDC/SAML provider integrations

        return services;
    }
}
