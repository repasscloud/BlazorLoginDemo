using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using BlazorLoginDemo.Shared.Data;
using BlazorLoginDemo.Shared.Auth;

namespace BlazorLoginDemo.Web.Security
{
    // Emits org + category + convenience scope claims at sign-in
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
            // Start with base: name, sub, roles, etc.
            var id = await base.GenerateClaimsAsync(user);

            // Make sure Organization nav is populated (depending on how you load users)
            if (user.Organization == null && user.OrganizationId != null)
            {
                user = await _db.Users
                    .Include(u => u.Organization)
                    .FirstAsync(u => u.Id == user.Id);
            }

            // user_category
            id.AddClaim(new Claim(AppClaimTypes.UserCategory, user.UserCategory.ToString()));

            // org_id + org_type (if any)
            if (user.OrganizationId is string orgId)
            {
                id.AddClaim(new Claim(AppClaimTypes.OrgId, orgId.ToString()));

                if (user.Organization is not null)
                {
                    id.AddClaim(new Claim(AppClaimTypes.OrgType, user.Organization.Type.ToString()));
                }
            }

            // tmc_id / client_id convenience scope
            if (user.TmcId is string tmcId)
            {
                id.AddClaim(new Claim(AppClaimTypes.TmcId, tmcId.ToString()));
            }

            if (user.ClientId is string clientId)
            {
                id.AddClaim(new Claim(AppClaimTypes.ClientId, clientId.ToString()));
            }

            return id;
        }
    }
}
