using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;

using BlazorLoginDemo.Web.Components;
using BlazorLoginDemo.Web.Components.Account;
using BlazorLoginDemo.Web.Data;      // ApplicationUser, UserGroup
using BlazorLoginDemo.Web.Services;  // MailerSendEmailSender + MailerSendOptions
using BlazorLoginDemo.Web.Startup;   // SeedData

namespace BlazorLoginDemo.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<IdentityUserAccessor>();
        builder.Services.AddScoped<IdentityRedirectManager>();
        builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

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
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        // Authorization policies
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireMemberOrAbove",
                p => p.RequireRole("Member", "Manager", "Admin"));

            options.AddPolicy("ManagersOnly",
                p => p.RequireRole("Manager", "Admin"));

            options.AddPolicy("AdminsOnly",
                p => p.RequireRole("Admin"));

            // --- New platform/org policies ---
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

        // Cookie options (so unauthorized goes to /Account/Login)
        builder.Services.ConfigureApplicationCookie(opts =>
        {
            opts.LoginPath = "/Account/Login";
        });

        // existing DbContext + interceptor
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
        builder.Services.AddScoped<IGroupResolver, GroupResolver>();
        builder.Services.AddScoped<UserGroupAssignmentInterceptor>();


        // Email Sender (MailerSend)
        builder.Services.Configure<MailerSendOptions>(builder.Configuration.GetSection("MailerSend"));
        builder.Services.AddHttpClient();
        builder.Services.AddTransient<IEmailSender, MailerSendEmailSender>();
        builder.Services.AddTransient<IEmailSender<ApplicationUser>, MailerSendEmailSender>();

        // Keep confirmed account requirement (already set above)
        builder.Services.Configure<IdentityOptions>(o =>
        {
            o.SignIn.RequireConfirmedAccount = true;
        });

        // Detect container
        var inContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            if (!inContainer) app.UseHsts();               // ⬅ gate HSTS in container
        }

        if (!inContainer)
        {
            app.UseHttpsRedirection();                     // ⬅ gate HTTPS redirect in container
        }

        // Static files for wwwroot + static web assets for RCLs
        app.UseStaticFiles();                              // ⬅ ensure blazor.web.js/_framework/* are served

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        // Optional: track LastSeenUtc for signed-in users
        app.Use(async (ctx, next) =>
        {
            if (ctx.User?.Identity?.IsAuthenticated == true)
            {
                var um = ctx.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await um.GetUserAsync(ctx.User);
                if (user is not null)
                {
                    user.LastSeenUtc = DateTimeOffset.UtcNow;
                    _ = await um.UpdateAsync(user); // ignore errors
                }
            }
            await next();
        });

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        // Identity endpoints
        app.MapAdditionalIdentityEndpoints();

        // Simple logout
        app.MapPost("/auth/logout", async (SignInManager<ApplicationUser> signIn) =>
        {
            await signIn.SignOutAsync();
            return Results.Redirect("/");
        })
        .RequireAuthorization();

        // Seed roles/admin
        // using (var scope = app.Services.CreateScope())
        // {
        //     var services = scope.ServiceProvider;
        //     await SeedData.InitializeAsync(services, builder.Configuration);
        // }
        // --- Seed roles/admin ---
        // using (var scope = app.Services.CreateScope())
        // {
        //     var services = scope.ServiceProvider;
        //     var db = services.GetRequiredService<ApplicationDbContext>();

        //     if (!await db.Database.CanConnectAsync())
        //     {
        //         app.Logger.LogError("❌ Cannot connect to Postgres. Check connection string and that the server/DB/user exist.");
        //     }
        //     else
        //     {
        //         await SeedData.InitializeAsync(services, builder.Configuration);
        //     }
        // }


        await app.RunAsync();
    }
}
