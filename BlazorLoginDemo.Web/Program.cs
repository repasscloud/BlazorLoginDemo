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

        // builder.Services.AddDbContext<ApplicationDbContext>(options =>
        //     options.UseSqlite(connectionString));
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
            // --- Existing base policies you had ---
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

        // Group resolver + assignment
        builder.Services.AddScoped<IGroupResolver, GroupResolver>();
        builder.Services.AddScoped<UserGroupAssignmentInterceptor>();

        // Replace your DbContext registration to inject the interceptor:
        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            // var cs = builder.Configuration.GetConnectionString("DefaultConnection")
            //         ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            // options.UseSqlite(cs);
            options.UseSqlite(connectionString);
            options.AddInterceptors(sp.GetRequiredService<UserGroupAssignmentInterceptor>());
        });

        // --- SMTP Email Sender configuration ---
        // builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
        // builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
        // builder.Services.AddTransient<IEmailSender<ApplicationUser>, SmtpEmailSender>();

        // --- Email Sender (MailerSend) ---
        builder.Services.Configure<MailerSendOptions>(builder.Configuration.GetSection("MailerSend"));
        builder.Services.AddHttpClient();
        builder.Services.AddTransient<IEmailSender, MailerSendEmailSender>();
        builder.Services.AddTransient<IEmailSender<ApplicationUser>, MailerSendEmailSender>();

        // Keep confirmed account requirement (already set above)
        builder.Services.Configure<IdentityOptions>(o =>
        {
            o.SignIn.RequireConfirmedAccount = true;
        });

        // before building the app:
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
            app.UseHsts();
        }

        if (!inContainer)
        {
            app.UseHttpsRedirection();
        }

        // ✅ Auth first
        app.UseAuthentication();
        app.UseAuthorization();

        // ✅ Then antiforgery
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

        // Add additional endpoints required by the Identity /Account Razor components.
        app.MapAdditionalIdentityEndpoints();

        // Simple, reliable logout endpoint (no returnUrl binding)
        app.MapPost("/auth/logout", async (SignInManager<ApplicationUser> signIn) =>
        {
            await signIn.SignOutAsync();
            return Results.Redirect("/");
        })
        .RequireAuthorization();   // keep auth; antiforgery is handled by app.UseAntiforgery()

        // --- Seed roles and initial admin on startup ---
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            await SeedData.InitializeAsync(services, builder.Configuration);
        }

        await app.RunAsync();
    }
}
