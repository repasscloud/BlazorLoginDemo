using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Cinturon360.Integrations.ExchangeRates;
using Cinturon360.Integrations.GitHub.Services;

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

        // ── GitHub Ticketing ──────────────────────────────────────────────
        var ghPat   = configuration["GitHub:TicketingPat"];
        var ghOwner = configuration["GitHub:TicketingOwner"];
        var ghRepo  = configuration["GitHub:TicketingRepo"];

        var ghConfigured = !string.IsNullOrWhiteSpace(ghPat)
                        && !string.IsNullOrWhiteSpace(ghOwner)
                        && !string.IsNullOrWhiteSpace(ghRepo);

        services.AddHttpClient("GitHubTicketing", client =>
        {
            if (ghConfigured)
            {
                client.BaseAddress = new Uri($"https://api.github.com/repos/{ghOwner}/{ghRepo}/");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ghPat}");
                client.DefaultRequestHeaders.Add("User-Agent",    "Cinturon360-TicketBot/1.0");
                client.DefaultRequestHeaders.Add("Accept",        "application/vnd.github+json");
                client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            }
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Register with the configured flag so dry-run mode works without secrets
        services.AddScoped<IGitHubTicketingService>(sp =>
            new GitHubIssuesClient(
                sp.GetRequiredService<IHttpClientFactory>(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<GitHubIssuesClient>>(),
                ghConfigured));

        return services;
    }
}
