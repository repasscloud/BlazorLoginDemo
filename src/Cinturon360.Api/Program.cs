using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Cinturon360.Application.DependencyInjection;
using Cinturon360.Data.DependencyInjection;
using Cinturon360.Infrastructure.DependencyInjection;
using Cinturon360.Api.Endpoints.Auth;
using Cinturon360.Api.Endpoints;
using Cinturon360.Api.Endpoints.Admin;
using Cinturon360.Api.Endpoints.Travellers;
using Cinturon360.Api.Endpoints.Bookings;
using Cinturon360.Api.Endpoints.Policies;
using Cinturon360.Api.Endpoints.Approvals;
using Cinturon360.Api.Endpoints.Billing;
using Cinturon360.Api.Endpoints.System;
using Cinturon360.Api.Endpoints.Ticketing;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Logging ──────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, cfg) =>
    {
        cfg.ReadFrom.Configuration(ctx.Configuration)
           .ReadFrom.Services(services)
           .Enrich.FromLogContext()
           .WriteTo.Console(outputTemplate:
               "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}");
    });

    // ── OpenAPI ──────────────────────────────────────────────────────────
    builder.Services.AddOpenApi();

    // ── JWT Authentication ────────────────────────────────────────────────
    var jwtKey = builder.Configuration["Jwt:SecretKey"]
        ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer              = builder.Configuration["Jwt:Issuer"],
                ValidAudience            = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew                = TimeSpan.FromSeconds(30)
            };
        });

    builder.Services.AddAuthorization();

    // ── Application Layers ────────────────────────────────────────────────
    builder.Services.AddApplicationServices();
    builder.Services.AddDataServices(builder.Configuration);
    builder.Services.AddInfrastructureServices(builder.Configuration);

    var app = builder.Build();

    // ── Middleware pipeline ───────────────────────────────────────────────
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
        app.MapOpenApi();

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    // ── Endpoints ─────────────────────────────────────────────────────────
    app.MapAuthEndpoints();
    app.MapUserEndpoints();
    app.MapOrganisationEndpoints();
    app.MapTravellerEndpoints();
    app.MapBookingEndpoints();
    app.MapPolicyEndpoints();
    app.MapApprovalEndpoints();
    app.MapBillingEndpoints();
    app.MapJobEndpoints();
    app.MapExchangeRateEndpoints();
    app.MapDuffelConfigurationEndpoints();
    app.MapTicketEmailTemplateEndpoints();
    app.MapTicketEndpoints();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application startup failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
