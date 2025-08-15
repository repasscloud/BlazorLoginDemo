using BlazorLoginDemo.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorLoginDemo.Web.Startup;

public static class SeedData
{
    private static readonly string[] Roles = new[] { "Member", "Manager", "Admin" };

    public static async Task InitializeAsync(IServiceProvider services, IConfiguration config)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

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

        // 2) Seed initial admin user from config
        var adminEmail = config["AdminEmail"];
        var adminPassword = config["AdminPassword"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            return; // silently skip if not configured

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName    = adminEmail,
                Email       = adminEmail,
                EmailConfirmed = true,
                DisplayName = "Administrator",
                Group       = UserGroup.Admin,
                IsActive    = true,
                LastSeenUtc = null
            };

            var create = await userManager.CreateAsync(adminUser, adminPassword);
            if (!create.Succeeded)
                throw new Exception($"Failed creating admin user: {string.Join("; ", create.Errors.Select(e => e.Description))}");
        }

        // Ensure Admin role
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            var addRole = await userManager.AddToRoleAsync(adminUser, "Admin");
            if (!addRole.Succeeded)
                throw new Exception($"Failed assigning Admin role: {string.Join("; ", addRole.Errors.Select(e => e.Description))}");
        }

        // (Optional) Also grant Member/Manager if you want cascading privileges via roles
        // await userManager.AddToRolesAsync(adminUser, new[] { "Member", "Manager" });
    }
}
