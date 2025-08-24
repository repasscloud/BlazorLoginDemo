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

namespace BlazorLoginDemo.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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
            // ❌ Remove global fallback policy — it was forcing auth on static assets
            // options.FallbackPolicy = new AuthorizationPolicyBuilder()
            //     .RequireAuthenticatedUser()
            //     .Build();

            options.AddPolicy("RequireMemberOrAbove",
                p => p.RequireRole("Member", "Manager", "Admin"));

            options.AddPolicy("ManagersOnly",
                p => p.RequireRole("Manager", "Admin"));

            options.AddPolicy("AdminsOnly",
                p => p.RequireRole("Admin"));

            // Platform/org policies
            options.AddPolicy("CanManageUsers",
                p => p.RequireRole("SuperAdmin", "OrgAdmin", "UserAdmin"));

            options.AddPolicy("CanEditPolicies",
                p => p.RequireRole("SuperAdmin", "OrgAdmin", "PolicyAdmin"));

            options.AddPolicy("CanEditFinancials",
                p => p.RequireRole("SuperAdmin", "OrgAdmin", "FinanceAdmin", "FinanceEditor"));

            options.AddPolicy("FinanceRead",
                p => p.RequireRole("SuperAdmin", "OrgAdmin", "FinanceAdmin", "FinanceEditor", "FinanceViewer", "SupportFinance"));

            options.AddPolicy("CanEnableDisableUser",
                p => p.RequireRole("SuperAdmin", "OrgAdmin", "UserAdmin"));

            options.AddPolicy("SupportArea",
                p => p.RequireRole("SupportViewer", "SupportAgent", "SupportFinance", "SupportAdmin"));

            options.AddPolicy("ApproverL1OrAbove",
                p => p.RequireRole("ApproverL1", "ApproverL2", "ApproverL3", "OrgAdmin", "SuperAdmin"));

            options.AddPolicy("ApproverL2OrAbove",
                p => p.RequireRole("ApproverL2", "ApproverL3", "OrgAdmin", "SuperAdmin"));

            options.AddPolicy("ApproverL3OrAbove",
                p => p.RequireRole("ApproverL3", "OrgAdmin", "SuperAdmin"));

            options.AddPolicy("CanManageGroups",
                p => p.RequireRole("SuperAdmin", "SupportAdmin"));
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
