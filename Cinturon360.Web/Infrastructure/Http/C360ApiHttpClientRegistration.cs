namespace Cinturon360.Web.Infrastructure.Http;

public static class C360ApiHttpClientRegistration
{
    public static IServiceCollection AddC360ApiHttpClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<C360ApiClient>();

        services.AddOptions<C360ApiAuthOptions>()
            .Bind(configuration.GetSection("OutboundApiKeyAuth"))
            .Validate(o =>
                !string.IsNullOrWhiteSpace(o.HeaderName) &&
                !string.IsNullOrWhiteSpace(o.Key),
                "OutboundApiKeyAuth configuration is invalid.")
            .ValidateOnStart();

        services.AddTransient<C360ApiAuthHandler>();

        services.AddHttpClient("C360Api", client =>
        {
            var baseAddress = configuration["Api:BaseAddress"];
            if (string.IsNullOrWhiteSpace(baseAddress))
                throw new InvalidOperationException(
                    "Api:BaseAddress must be configured.");

            client.BaseAddress = new Uri(baseAddress);
        })
        .AddHttpMessageHandler<C360ApiAuthHandler>();

        return services;
    }
}
