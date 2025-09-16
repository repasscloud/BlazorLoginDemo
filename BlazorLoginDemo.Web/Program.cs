using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using BlazorLoginDemo.Web.Components;
using BlazorLoginDemo.Web.Components.Account;
using BlazorLoginDemo.Web.Services;  // MailerSendEmailSender + MailerSendOptions
using BlazorLoginDemo.Web.Security;  // SeedData
using BlazorLoginDemo.Shared.Auth;
using BlazorLoginDemo.Shared.Services;

using BlazorLoginDemo.Shared.Logging;
using Serilog;

namespace BlazorLoginDemo.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Serilog first
        SerilogBootstrap.UseSerilogWithPostgres(builder.Configuration, appName: "Ava.Web");
        builder.Host.UseSerilog();

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents(); // .NET 9 Interactive Server

        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<IdentityUserAccessor>();
        builder.Services.AddScoped<IdentityRedirectManager>();
        builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        // Identity + Roles
        builder.Services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.MaxFailedAccessAttempts = 5;

                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AppClaimsFactory>();

        // Authorization policies
        builder.Services.AddAuthorization(options =>
        {
            // Sudo always allowed via [Authorize(Roles=AppRoles.Sudo)] if you want.

            // Platform
            options.AddPolicy(AppPolicies.PlatformPolicy.AdminArea, p =>
                p.RequireRole(AppRoles.Sudo, AppRoles.Platform.SuperAdmin, AppRoles.Platform.Admin, AppRoles.Platform.SuperUser));
            options.AddPolicy(AppPolicies.PlatformPolicy.SupportArea, p =>
                p.RequireRole(AppRoles.Sudo, AppRoles.Platform.Support.Admin, AppRoles.Platform.Support.Agent, AppRoles.Platform.Support.Viewer, AppRoles.Platform.Support.Finance));
            options.AddPolicy(AppPolicies.PlatformPolicy.FinanceWrite, p =>
                p.RequireRole(AppRoles.Sudo, AppRoles.Platform.Finance.Admin, AppRoles.Platform.Finance.Editor));
            options.AddPolicy(AppPolicies.PlatformPolicy.FinanceRead, p =>
                p.RequireRole(AppRoles.Sudo, AppRoles.Platform.Finance.Admin, AppRoles.Platform.Finance.Editor, AppRoles.Platform.Finance.Viewer));
            options.AddPolicy(AppPolicies.PlatformPolicy.SalesArea, p =>
                p.RequireRole(AppRoles.Sudo, AppRoles.Platform.Sales.Admin, AppRoles.Platform.Sales.Manager, AppRoles.Platform.Sales.Rep));
            options.AddPolicy(AppPolicies.PlatformPolicy.ReportsRead, p =>
                p.RequireRole(AppRoles.Sudo, AppRoles.Platform.ReportsViewer, AppRoles.Platform.DataExporter, AppRoles.Platform.Auditor, AppRoles.Platform.ReadOnly));
            options.AddPolicy(AppPolicies.PlatformPolicy.DataExport, p =>
                p.RequireRole(AppRoles.Sudo, AppRoles.Platform.DataExporter));

            options.AddPolicy("Platform:ManageUsers", p =>
                p.RequireRole(AppRoles.Sudo, AppRoles.Platform.UserAdmin, AppRoles.Platform.SuperAdmin, AppRoles.Platform.Admin));

            // TMC
            options.AddPolicy(AppPolicies.TmcPolicy.AdminArea, p =>
                p.RequireRole(AppRoles.Sudo, AppRoles.Tmc.Admin, AppRoles.Tmc.UserAdmin, AppRoles.Tmc.PolicyAdmin, AppRoles.Tmc.SecurityAdmin, AppRoles.Tmc.IntegrationAdmin));
            options.AddPolicy(AppPolicies.TmcPolicy.FinanceWrite, p =>
                p.RequireRole(AppRoles.Sudo, AppRoles.Tmc.Finance.Admin, AppRoles.Tmc.Finance.Editor));
            options.AddPolicy(AppPolicies.TmcPolicy.FinanceRead, p =>
                p.RequireRole(AppRoles.Sudo, AppRoles.Tmc.Finance.Admin, AppRoles.Tmc.Finance.Editor, AppRoles.Tmc.Finance.Viewer, AppRoles.Tmc.ReadOnly));
            options.AddPolicy(AppPolicies.TmcPolicy.BookingsOps, p =>
                p.RequireRole(AppRoles.Sudo, AppRoles.Tmc.BookingsManager, AppRoles.Tmc.TravelAgent));
            options.AddPolicy(AppPolicies.TmcPolicy.ReportsRead, p =>
                p.RequireRole(AppRoles.Sudo, AppRoles.Tmc.ReportsViewer, AppRoles.Tmc.DataExporter, AppRoles.Tmc.Auditor, AppRoles.Tmc.ReadOnly));
            options.AddPolicy(AppPolicies.TmcPolicy.DataExport, p =>
                p.RequireRole(AppRoles.Sudo, AppRoles.Tmc.DataExporter));

            // Client
            options.AddPolicy(AppPolicies.ClientPolicy.AdminArea, p =>
                p.RequireRole(AppRoles.Sudo, AppRoles.Client.Admin, AppRoles.Client.UserAdmin, AppRoles.Client.PolicyAdmin, AppRoles.Client.SecurityAdmin, AppRoles.Client.IntegrationAdmin));
            options.AddPolicy(AppPolicies.ClientPolicy.FinanceWrite, p =>
                p.RequireRole(AppRoles.Sudo, AppRoles.Client.Finance.Admin, AppRoles.Client.Finance.Editor));
            options.AddPolicy(AppPolicies.ClientPolicy.FinanceRead, p =>
                p.RequireRole(AppRoles.Sudo, AppRoles.Client.Finance.Admin, AppRoles.Client.Finance.Editor, AppRoles.Client.Finance.Viewer, AppRoles.Client.ReadOnly));
            options.AddPolicy(AppPolicies.ClientPolicy.ApproverL1Plus, p =>
                p.RequireRole(AppRoles.Sudo, AppRoles.Client.Approver.L1, AppRoles.Client.Approver.L2, AppRoles.Client.Approver.L3));
            options.AddPolicy(AppPolicies.ClientPolicy.ApproverL2Plus, p =>
                p.RequireRole(AppRoles.Sudo, AppRoles.Client.Approver.L2, AppRoles.Client.Approver.L3));
            options.AddPolicy(AppPolicies.ClientPolicy.ApproverL3Only, p =>
                p.RequireRole(AppRoles.Sudo, AppRoles.Client.Approver.L3));
            options.AddPolicy(AppPolicies.ClientPolicy.ReportsRead, p =>
                p.RequireRole(AppRoles.Sudo, AppRoles.Client.ReportsViewer, AppRoles.Client.DataExporter, AppRoles.Client.Auditor, AppRoles.Client.ReadOnly));
            options.AddPolicy(AppPolicies.ClientPolicy.DataExport, p =>
                p.RequireRole(AppRoles.Sudo, AppRoles.Client.DataExporter));
            options.AddPolicy(AppPolicies.ClientPolicy.Requestor, p =>
                p.RequireRole(AppRoles.Sudo, AppRoles.Client.Requestor));
        });


        // Cookie options
        builder.Services.ConfigureApplicationCookie(opts =>
        {
            opts.LoginPath = "/Account/Login";
            opts.AccessDeniedPath = "/Account/AccessDenied";
        });

        // DbContext + interceptor
        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npg =>
            {
                npg.EnableRetryOnFailure(5, TimeSpan.FromSeconds(2), null);
                npg.CommandTimeout(30);
            });
        });

        // Email Sender (MailerSend)
        builder.Services.Configure<MailerSendOptions>(builder.Configuration.GetSection("MailerSend"));

        // Http
        builder.Services.AddHttpClient();
        builder.Services.AddAvaApiHttpClient(builder.Configuration);

        // Services
        builder.Services.AddAvaPlatformServices();
        builder.Services.AddAvaClientServices();
        builder.Services.AddAvaFinanceServices();
        builder.Services.AddAvaPolicyServices();
        builder.Services.AddTransient<IEmailSender, MailerSendEmailSender>();
        builder.Services.AddTransient<IEmailSender<ApplicationUser>, MailerSendEmailSender>();

        builder.Services.Configure<IdentityOptions>(o =>
        {
            o.SignIn.RequireConfirmedAccount = true;
        });

        // Localization: RESX under /Resources
        builder.Services.AddLocalization(opts => opts.ResourcesPath = "Resources");

        var supportedCultures = new[] { "en-AU", "en-GB", "es-ES", "it-IT", "fr-FR" };
        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            options.SetDefaultCulture("en-AU")
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);

            options.RequestCultureProviders = new IRequestCultureProvider[]
            {
                new ClaimRequestCultureProvider(),
                new QueryStringRequestCultureProvider { QueryStringKey = "culture" },
                new CookieRequestCultureProvider(),
                new AcceptLanguageHeaderRequestCultureProvider()
            };
        });

        // Detect container
        var inContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

        var app = builder.Build();

        // Serilog
        app.UseSerilogRequestLogging(opts =>
        {
            // add request-scoped properties
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

        // Culture switch endpoint
        app.MapGet("/set-culture", (string culture, string? redirectUri, HttpContext ctx) =>
        {
            ctx.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });

            return Results.Redirect(string.IsNullOrWhiteSpace(redirectUri) ? "/" : redirectUri);
        });

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            if (!inContainer) app.UseHsts();
        }

        if (!inContainer)
        {
            app.UseHttpsRedirection();
        }

        // 1) wwwroot files (images, app.css, etc.)
        app.UseStaticFiles();

        // 2) Localization
        app.UseRequestLocalization(app.Services
            .GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

        // 3) Auth middlewares
        app.UseAuthentication();
        app.UseAuthorization();

        // 4) Antiforgery
        app.UseAntiforgery();

        // 5) SPA fallback to render NotFound via <Router>, preserving HTTP 404
        //    - Only triggers for non-file paths (no extension) and non-API/framework/content paths
        app.Use(async (ctx, next) =>
        {
            await next();

            if (ctx.Response.StatusCode == 404
                && !Path.HasExtension(ctx.Request.Path.Value ?? string.Empty)
                && !ctx.Request.Path.StartsWithSegments("/api")
                && !ctx.Request.Path.StartsWithSegments("/_framework")
                && !ctx.Request.Path.StartsWithSegments("/_content")
                && !ctx.Request.Path.StartsWithSegments("/css")
                && !ctx.Request.Path.StartsWithSegments("/js")
                && !ctx.Request.Path.StartsWithSegments("/images"))
            {
                // Preserve 404 status for crawlers/SEO, but re-run pipeline at '/'
                ctx.Response.Clear();
                ctx.Response.StatusCode = 404;
                ctx.Request.Path = "/";
                await next();
            }
        });

        // 6) Map Razor Components (your app)
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        // 7) Map static web assets for components/framework (/_framework, /_content, *.styles.css)
        //    Allow anonymous so fallback auth policy never blocks scripts/styles.
        app.MapStaticAssets().AllowAnonymous();

        // Identity endpoints
        app.MapAdditionalIdentityEndpoints();

        // Simple logout
        app.MapPost("/auth/logout", async (SignInManager<ApplicationUser> signIn) =>
        {
            await signIn.SignOutAsync();
            return Results.Redirect("/");
        })
        .RequireAuthorization();

        await app.RunAsync();
    }
}
