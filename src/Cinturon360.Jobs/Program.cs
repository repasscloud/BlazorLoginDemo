using Cinturon360.Application.DependencyInjection;
using Cinturon360.Data.DependencyInjection;
using Cinturon360.Infrastructure.DependencyInjection;
using Cinturon360.Integrations.DependencyInjection;
using Cinturon360.Jobs.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddApplicationServices()
    .AddDataServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddIntegrationServices(builder.Configuration)
    .AddJobServices();

var host = builder.Build();
host.Run();
