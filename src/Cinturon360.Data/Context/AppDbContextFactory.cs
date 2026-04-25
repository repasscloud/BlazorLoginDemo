using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Cinturon360.Data.Context;

/// <summary>
/// Design-time factory for EF Core migrations.
/// Reads connection string from environment variable or user-secrets / appsettings.Development.json.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Cinturon360.Api"))
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Postgres")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? "Host=localhost;Database=cinturon360_dev;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly("Cinturon360.Data");
            })
            .UseSnakeCaseNamingConvention()
            .Options;

        return new AppDbContext(options);
    }
}
