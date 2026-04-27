using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using QuestPDF.Infrastructure;
using Cinturon360.Application.Abstractions.Security;
using Cinturon360.Infrastructure.Security;
using Amazon.S3;
using Amazon;
using Amazon.Runtime;
using Cinturon360.Application.Abstractions.Services;
using Cinturon360.Infrastructure.Email;
using Cinturon360.Infrastructure.Storage;
using Cinturon360.Infrastructure.Payments;

namespace Cinturon360.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // QuestPDF license — Community license for open-source / evaluation
        QuestPDF.Settings.License = LicenseType.Community;

        // JWT token service
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddSingleton<ITokenService, TokenService>();

        // Password hashing
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        // Current user (resolves from HTTP context)
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        // Email service (MailerSend)
        services.Configure<MailerSendSettings>(configuration.GetSection("MailerSend"));
        services.AddHttpClient<MailerSendEmailService>(client =>
        {
            var baseUrl = configuration["MailerSend:BaseUrl"];
            if (!string.IsNullOrWhiteSpace(baseUrl)
                && Uri.TryCreate(baseUrl, UriKind.Absolute, out var parsed))
            {
                client.BaseAddress = parsed;
            }

            client.Timeout = TimeSpan.FromSeconds(15);
        });
        services.AddScoped<IEmailService>(sp => sp.GetRequiredService<MailerSendEmailService>());

        // Payment gateway (Stripe)
        services.Configure<StripeSettings>(configuration.GetSection("Stripe"));
        services.AddScoped<IPaymentGateway, StripePaymentGateway>();

        // Object storage (Cloudflare R2 via S3 SDK)
        var storageSection = configuration.GetSection("Storage");
        if (storageSection.Exists())
        {
            services.Configure<S3StorageSettings>(storageSection);
            var accessKey = storageSection["AccessKeyId"] ?? string.Empty;
            var secretKey = storageSection["SecretAccessKey"] ?? string.Empty;
            var endpoint = storageSection["Endpoint"] ?? string.Empty;
            services.AddSingleton<IAmazonS3>(_ =>
            {
                var credentials = new BasicAWSCredentials(accessKey, secretKey);
                var config = new AmazonS3Config
                {
                    ServiceURL = endpoint,
                    ForcePathStyle = true
                };
                return new AmazonS3Client(credentials, config);
            });
            services.AddScoped<IStorageService, S3StorageService>();
        }

        return services;
    }
}
