using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using BlazorLoginDemo.Shared.Data;
using BlazorLoginDemo.Shared.Auth; // AppClaimTypes

namespace BlazorLoginDemo.Web.Security
{
    // emits org + category + convenience scope claims at sign-in
    public sealed class AppClaimsFactory
        : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        private readonly ApplicationDbContext _db;

        public AppClaimsFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            ApplicationDbContext db)
            : base(userManager, roleManager, optionsAccessor)
        {
            _db = db;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            // start with base: name, sub, roles, etc.
            var id = await base.GenerateClaimsAsync(user);

            // make sure Organization nav is populated (depends on how load users)
            if (user.Organization == null && !string.IsNullOrEmpty(user.OrganizationId))
            {
                // minimal re-query with the nav you need
                user = await _db.Users
                    .Include(u => u.Organization)
                    .AsNoTracking()
                    .FirstAsync(u => u.Id == user.Id);
            }

            // user_category
            id.AddClaim(new Claim(AppClaimTypes.UserCategory, user.UserCategory.ToString()));

            // org_id + org_type (if any)
            if (!string.IsNullOrEmpty(user.OrganizationId))
            {
                id.AddClaim(new Claim(AppClaimTypes.OrgId, user.OrganizationId));
                if (user.Organization is not null)
                {
                    id.AddClaim(new Claim(AppClaimTypes.OrgType, user.Organization.Type.ToString()));
                }
            }

            // tmc_id / client_id convenience scope (strings in the model)
            if (!string.IsNullOrEmpty(user.TmcId))
                id.AddClaim(new Claim(AppClaimTypes.TmcId, user.TmcId));

            if (!string.IsNullOrEmpty(user.ClientId))
                id.AddClaim(new Claim(AppClaimTypes.ClientId, user.ClientId));

            return id;
        }
    }
}
