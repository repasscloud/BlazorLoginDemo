using BlazorLoginDemo.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;              // <-- needed for AnyAsync/FirstOrDefaultAsync
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorLoginDemo.Web.Startup;

public static class SeedData
{
    // static + capitalized to match usage below
    private static readonly string[] Roles = new[]
    {
        "SuperAdmin","SupportAdmin","SupportFinance","SupportAgent","SupportViewer",
        "OrgAdmin","UserAdmin","PolicyAdmin","FinanceAdmin","FinanceEditor","FinanceViewer",
        "SecurityAdmin","IntegrationAdmin","BookingsManager","TravelAgent",
        "ApproverL1","ApproverL2","ApproverL3",
        "ReportsViewer","DataExporter","Auditor",
        "Requestor","ReadOnly"
    };

    public static async Task InitializeAsync(IServiceProvider services, IConfiguration config)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db          = services.GetRequiredService<ApplicationDbContext>();

        // 1) Ensure roles exist
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                    throw new Exception($"Failed creating role '{role}': {string.Join("; ", result.Errors.Select(e => e.Description))}");
            }
        }

        // 2) Ensure catch-all group exists (GroupX)
        if (!await db.Groups.AnyAsync(g => g.IsCatchAll))
        {
            db.Groups.Add(new Group { Name = "GroupX", IsCatchAll = true, IsActive = true });
            await db.SaveChangesAsync();
        }

        // 3) Seed initial admin user from config
        var adminEmail    = config["AdminEmail"];
        var adminPassword = config["AdminPassword"];
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            return; // silently skip if not configured

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName      = adminEmail,
                Email         = adminEmail,
                EmailConfirmed = true,
                DisplayName   = "Administrator",
                IsActive      = true,
                LastSeenUtc   = null
            };

            // Optional: put the admin into the catch-all group
            var catchAll = await db.Groups.FirstOrDefaultAsync(g => g.IsCatchAll);
            if (catchAll != null)
                adminUser.GroupId = catchAll.Id;   // requires you've switched to DB-backed GroupId

            var create = await userManager.CreateAsync(adminUser, adminPassword);
            if (!create.Succeeded)
                throw new Exception($"Failed creating admin user: {string.Join("; ", create.Errors.Select(e => e.Description))}");
        }

        // Ensure SuperAdmin role for the seeded admin
        if (!await userManager.IsInRoleAsync(adminUser, "SuperAdmin"))
        {
            var addRole = await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
            if (!addRole.Succeeded)
                throw new Exception($"Failed assigning SuperAdmin role: {string.Join("; ", addRole.Errors.Select(e => e.Description))}");
        }
    }
}
