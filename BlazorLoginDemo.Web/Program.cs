using System.IO;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using BlazorLoginDemo.Web.Components;
using BlazorLoginDemo.Web.Components.Account;
using BlazorLoginDemo.Web.Data;      // ApplicationUser, UserGroup
using BlazorLoginDemo.Web.Services;  // MailerSendEmailSender + MailerSendOptions
using BlazorLoginDemo.Shared.Startup;
using BlazorLoginDemo.Web.Security;   // SeedData
using BlazorLoginDemo.Shared.Auth;
using BlazorLoginDemo.Shared.Services;
using System.Collections.Specialized;

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
            .AddInteractiveServerComponents(); // .NET 8 Interactive Server

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
            // ── GLOBAL POLICIES ──────────────────────────────────────────────────────
            options.AddPolicy(AppPolicies.GlobalPolicy.AdminsOnly,
                p => p.RequireRole(AppRoles.GlobalRole.SuperAdmin, AppRoles.GlobalRole.UserAdmin, AppRoles.GlobalRole.PolicyAdmin, AppRoles.GlobalRole.FinanceAdmin));

            // Keep your legacy “member/manager/admin” if still used:
            options.AddPolicy(AppPolicies.GlobalPolicy.RequireMemberOrAbove,
                p => p.RequireRole("Member", "Manager", "Admin"));

            options.AddPolicy(AppPolicies.GlobalPolicy.ManagersOnly,
                p => p.RequireRole("Manager", "Admin"));

            options.AddPolicy(AppPolicies.GlobalPolicy.CanManageUsers,
                p => p.RequireRole(AppRoles.GlobalRole.SuperAdmin, AppRoles.GlobalRole.UserAdmin));

            options.AddPolicy(AppPolicies.GlobalPolicy.CanEditPolicies,
                p => p.RequireRole(AppRoles.GlobalRole.SuperAdmin, AppRoles.GlobalRole.PolicyAdmin));

            options.AddPolicy(AppPolicies.GlobalPolicy.CanEditFinancials,
                p => p.RequireRole(AppRoles.GlobalRole.SuperAdmin, AppRoles.GlobalRole.FinanceAdmin, AppRoles.GlobalRole.FinanceEditor));

            options.AddPolicy(AppPolicies.GlobalPolicy.FinanceRead,
                p => p.RequireRole(AppRoles.GlobalRole.SuperAdmin, AppRoles.GlobalRole.FinanceAdmin, AppRoles.GlobalRole.FinanceEditor, AppRoles.GlobalRole.FinanceViewer, AppRoles.GlobalRole.SupportFinance));

            options.AddPolicy(AppPolicies.GlobalPolicy.CanEnableDisableUser,
                p => p.RequireRole(AppRoles.GlobalRole.SuperAdmin, AppRoles.GlobalRole.UserAdmin));

            options.AddPolicy(AppPolicies.GlobalPolicy.SupportArea,
                p => p.RequireRole(AppRoles.GlobalRole.SupportViewer, AppRoles.GlobalRole.SupportAgent, AppRoles.GlobalRole.SupportFinance, AppRoles.GlobalRole.SupportAdmin, AppRoles.GlobalRole.SuperAdmin));

            options.AddPolicy(AppPolicies.GlobalPolicy.CanManageGroups,
                p => p.RequireRole(AppRoles.GlobalRole.SuperAdmin, AppRoles.GlobalRole.SupportAdmin));

            // Sales / Licensing
            options.AddPolicy(AppPolicies.GlobalPolicy.SalesArea,
                p => p.RequireRole(AppRoles.GlobalRole.SalesRep, AppRoles.GlobalRole.SalesManager, AppRoles.GlobalRole.SalesAdmin, AppRoles.GlobalRole.SuperAdmin));

            options.AddPolicy(AppPolicies.GlobalPolicy.CanCreateCustomers,
                p => p.RequireRole(AppRoles.GlobalRole.SalesRep, AppRoles.GlobalRole.SalesManager, AppRoles.GlobalRole.SalesAdmin, AppRoles.GlobalRole.SuperAdmin));

            options.AddPolicy(AppPolicies.GlobalPolicy.LicenseRead,
                p => p.RequireRole(AppRoles.GlobalRole.SalesRep, AppRoles.GlobalRole.SalesManager, AppRoles.GlobalRole.SalesAdmin, AppRoles.GlobalRole.FinanceViewer, AppRoles.GlobalRole.SupportFinance, AppRoles.GlobalRole.SuperAdmin));

            options.AddPolicy(AppPolicies.GlobalPolicy.CanManageLicenses,
                p => p.RequireRole(AppRoles.GlobalRole.SalesManager, AppRoles.GlobalRole.SalesAdmin, AppRoles.GlobalRole.SuperAdmin));

            options.AddPolicy(AppPolicies.GlobalPolicy.CanAmendLicenses,
                p => p.RequireRole(AppRoles.GlobalRole.SalesManager, AppRoles.GlobalRole.SalesAdmin, AppRoles.GlobalRole.SuperAdmin));

            options.AddPolicy(AppPolicies.GlobalPolicy.CanApproveDiscounts,
                p => p.RequireRole(AppRoles.GlobalRole.SalesManager, AppRoles.GlobalRole.SalesAdmin, AppRoles.GlobalRole.SuperAdmin));

            // ── ORG-SCOPED POLICIES (tenant matching via OrgRoleRequirement) ────────
            options.AddPolicy(AppPolicies.OrgPolicy.Admin,
                p => p.Requirements.Add(new OrgRoleRequirement(AppRoles.OrgRole.Admin)));

            options.AddPolicy(AppPolicies.OrgPolicy.UserAdmin,
                p => p.Requirements.Add(new OrgRoleRequirement(AppRoles.OrgRole.UserAdmin, AppRoles.OrgRole.Admin)));

            options.AddPolicy(AppPolicies.OrgPolicy.PolicyAdmin,
                p => p.Requirements.Add(new OrgRoleRequirement(AppRoles.OrgRole.PolicyAdmin, AppRoles.OrgRole.Admin)));

            options.AddPolicy(AppPolicies.OrgPolicy.FinanceAdmin,
                p => p.Requirements.Add(new OrgRoleRequirement(AppRoles.OrgRole.FinanceAdmin, AppRoles.OrgRole.Admin)));

            options.AddPolicy(AppPolicies.OrgPolicy.BookingsManager,
                p => p.Requirements.Add(new OrgRoleRequirement(AppRoles.OrgRole.BookingsManager, AppRoles.OrgRole.Admin)));

            options.AddPolicy(AppPolicies.OrgPolicy.ReportsViewer,
                p => p.Requirements.Add(new OrgRoleRequirement(AppRoles.OrgRole.ReportsViewer, AppRoles.OrgRole.Admin)));

            options.AddPolicy(AppPolicies.OrgPolicy.DataExporter,
                p => p.Requirements.Add(new OrgRoleRequirement(AppRoles.OrgRole.DataExporter, AppRoles.OrgRole.Admin)));

            // Approvals ladder per org
            options.AddPolicy(AppPolicies.OrgPolicy.ApproverL1OrAbove,
                p => p.Requirements.Add(new OrgRoleRequirement(AppRoles.OrgRole.ApproverL1, AppRoles.OrgRole.ApproverL2, AppRoles.OrgRole.ApproverL3, AppRoles.OrgRole.Admin)));

            options.AddPolicy(AppPolicies.OrgPolicy.ApproverL2OrAbove,
                p => p.Requirements.Add(new OrgRoleRequirement(AppRoles.OrgRole.ApproverL2, AppRoles.OrgRole.ApproverL3, AppRoles.OrgRole.Admin)));

            options.AddPolicy(AppPolicies.OrgPolicy.ApproverL3OrAbove,
                p => p.Requirements.Add(new OrgRoleRequirement(AppRoles.OrgRole.ApproverL3, AppRoles.OrgRole.Admin)));
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
            options.AddInterceptors(sp.GetRequiredService<UserGroupAssignmentInterceptor>());
        });

        // Group resolver + assignment
        builder.Services.AddScoped<BlazorLoginDemo.Shared.Services.Interfaces.IGroupResolver, GroupResolver>();
        builder.Services.AddScoped<BlazorLoginDemo.Shared.Data.UserGroupAssignmentInterceptor>();

        // Email Sender (MailerSend)
        builder.Services.Configure<MailerSendOptions>(builder.Configuration.GetSection("MailerSend"));
        builder.Services.AddHttpClient();
        builder.Services.AddAvaClientServices();
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
