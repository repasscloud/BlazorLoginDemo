using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Data.Context;
using Cinturon360.Data.Repositories;

namespace Cinturon360.Data.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

        services.AddDbContext<AppDbContext>(options =>
        {
            options
                .UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    npgsql.EnableRetryOnFailure(maxRetryCount: 5);
                })
                .UseSnakeCaseNamingConvention()
                .UseOpenIddict();
        });

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserSecurityRepository, UserSecurityRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IUserApiTokenRepository, UserApiTokenRepository>();
        services.AddScoped<IOrganisationRepository, OrganisationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Phase 3 — Traveller profile
        services.AddScoped<ITravellerProfileRepository, TravellerProfileRepository>();

        // Phase 4 — Bookings / Quotes / Policy
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IQuoteRepository, QuoteRepository>();
        services.AddScoped<ITravelPolicyRepository, TravelPolicyRepository>();

        // Phase 5 — Approvals
        services.AddScoped<IApprovalRepository, ApprovalRepository>();

        // Phase 6 — Billing
        services.AddScoped<IBillingRepository, BillingRepository>();

        // Phase 7 — Jobs / Documents
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IStoredDocumentRepository, StoredDocumentRepository>();

        // Phase 10 — Exchange Rates
        services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
        services.AddScoped<IDuffelOrgConfigurationRepository, DuffelOrgConfigurationRepository>();

        // Phase 10 — Support Ticketing
        services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
        services.AddScoped<ITicketEmailTemplateRepository, TicketEmailTemplateRepository>();

        return services;
    }
}
