using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;
using Mapster;
using Cinturon360.Application.Behaviors.Validation;
using Cinturon360.Application.Behaviors.Logging;
using Cinturon360.Application.Behaviors.Authorization;

namespace Cinturon360.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = typeof(ServiceCollectionExtensions).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        // Register Mapster global config (scan assembly for IRegister implementations)
        TypeAdapterConfig.GlobalSettings.Scan(assembly);

        return services;
    }
}
