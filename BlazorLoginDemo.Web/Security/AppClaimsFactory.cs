using System.Security.Claims;
using BlazorLoginDemo.Web.Data;
using Microsoft.AspNetCore.Identity;

namespace BlazorLoginDemo.Web.Security;

public class AppClaimsFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public AppClaimsFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        Microsoft.Extensions.Options.IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor) { }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var id = await base.GenerateClaimsAsync(user);
        if (!string.IsNullOrWhiteSpace(user.PreferredCulture))
            id.AddClaim(new Claim("preferred_culture", user.PreferredCulture));
        return id;
    }
}
