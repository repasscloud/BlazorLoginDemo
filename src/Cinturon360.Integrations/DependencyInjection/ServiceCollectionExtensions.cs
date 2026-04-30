using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Cinturon360.Integrations.ExchangeRates;

namespace Cinturon360.Integrations.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIntegrationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── ECB Exchange Rate provider ────────────────────────────────────
        services.AddHttpClient("EcbRates", client =>
        {
            client.BaseAddress = new Uri("https://data-api.ecb.europa.eu/");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddScoped<IExchangeRateProvider, EcbExchangeRateProvider>();

        // TODO: Register typed HttpClients with Polly resilience for:
        //   - Flight search providers (Amadeus, Duffel)
        //   - Hotel search providers
        //   - Stripe payment client (uses Stripe.net SDK directly)
        //   - GitHub Issues (ticketing)
        //   - AWS S3 client (uses AWSSDK directly)
        // TODO: Register OIDC/SAML provider integrations

        return services;
    }
}
