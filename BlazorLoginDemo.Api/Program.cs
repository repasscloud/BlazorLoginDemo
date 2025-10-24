using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

using BlazorLoginDemo.Shared.Data;
using BlazorLoginDemo.Api.Auth;
using BlazorLoginDemo.Shared.Models.Auth;
using BlazorLoginDemo.Shared.Logging;
using BlazorLoginDemo.Shared.Services;

using Serilog;
using BlazorLoginDemo.Shared.Models.ExternalLib.Amadeus;
using System.Text.Json.Serialization;

public class Program
{
    public static void Main(string[] args)
    {
        // --------------------------
        // Host + builder
        // --------------------------
        var builder = WebApplication.CreateBuilder(args);

        // --------------------------
        // Logging (Serilog first)
        // --------------------------
        SerilogBootstrap.UseSerilogWithPostgres(builder.Configuration, appName: "Ava.API");
        builder.Host.UseSerilog();

        // --------------------------
        // Options (JWT)
        // --------------------------
        builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
        var jwt = builder.Configuration.GetSection("Jwt");
        builder.Services.Configure<AmadeusOAuthClientSettings>(
            builder.Configuration.GetSection("Amadeus"));

        // --------------------------
        // Data (DbContext)
        // --------------------------
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        // --------------------------
        // Identity (core-only)
        // --------------------------
        builder.Services
            .AddIdentityCore<ApplicationUser>(o =>
            {
                // Configure password/lockout/etc here if needed
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

        // --------------------------
        // AuthN / JWT
        // --------------------------
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwt["Issuer"],
                    ValidAudience = jwt["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt["SigningKey"]!)),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        builder.Services.AddAuthorization();

        // --------------------------
        // HTTP + JSON for shared services
        // --------------------------
        // moved to 'infra used by shared services' in ServiceCollectionExtensions of AddExternalLibService

        // --------------------------
        // Shared external library services
        // --------------------------
        builder.Services.AddApiLibServices(builder.Configuration); // wires IAmadeusAuthService->AmadeusAuthService

        // --------------------------
        // CORS
        // --------------------------
        builder.Services.AddCors(o =>
        {
            o.AddPolicy("AllowClients", p => p
                .WithOrigins("https://localhost:5003", "http://localhost:5003") // Blazor dev origins
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
        });

        // --------------------------
        // Swagger
        // --------------------------
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement{
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }});
        });

        // --------------------------
        // MVC / Controllers
        // --------------------------
        builder.Services.AddControllers();
            // .AddJsonOptions(o =>
            // {
            //     o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            // });

        // --------------------------
        // App-specific services
        // --------------------------
        builder.Services.AddScoped<TokenService>();

        // --------------------------
        // Build app
        // --------------------------
        var app = builder.Build();

        // --------------------------
        // Middleware pipeline
        // --------------------------
        app.UseSerilogRequestLogging(opts =>
        {
            // Enrich requests with useful properties
            opts.EnrichDiagnosticContext = (ctx, http) =>
            {
                ctx.Set("RequestPath", http.Request.Path);
                ctx.Set("RequestId", http.TraceIdentifier);

                var userId = http.User?.Identity?.IsAuthenticated == true
                    ? (http.User.Identity?.Name ?? http.User.FindFirst("sub")?.Value)
                    : null;

                if (!string.IsNullOrWhiteSpace(userId))
                    ctx.Set("UserId", userId);

                ctx.Set("Environment", app.Environment.EnvironmentName);
                ctx.Set("Application", "Ava.API");
            };
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        // CORS before auth
        app.UseCors("AllowClients");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
