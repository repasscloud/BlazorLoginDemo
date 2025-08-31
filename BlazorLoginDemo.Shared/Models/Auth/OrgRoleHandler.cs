using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BlazorLoginDemo.Shared.Auth;

public sealed class OrgRoleHandler : AuthorizationHandler<OrgRoleRequirement>
{
    private readonly IHttpContextAccessor _http;

    public OrgRoleHandler(IHttpContextAccessor http) => _http = http;

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OrgRoleRequirement requirement)
    {
        var user = context.User;
        if (user is null)
            return Task.CompletedTask;

        // Global override
        if (user.IsInRole(AppRoles.GlobalRole.SuperAdmin))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Extract orgId from route (e.g., /orgs/{orgId}/...)
        var httpCtx = _http.HttpContext;
        var routeData = httpCtx?.GetRouteData();
        var orgIdStr = routeData?.Values.TryGetValue("orgId", out var v) == true ? v?.ToString() : null;
        if (string.IsNullOrWhiteSpace(orgIdStr))
            return Task.CompletedTask; // no orgId => deny (you can choose to fallback to header/query if you prefer)

        // org_role claims: "<orgId>:<RoleName>"
        var orgRoleClaims = user.FindAll("org_role");
        foreach (var claim in orgRoleClaims)
        {
            var value = claim.Value; // e.g. "42:OrgFinanceAdmin"
            var sepIndex = value.IndexOf(':');
            if (sepIndex < 0) continue;

            var claimOrgId = value[..sepIndex];
            var roleName   = value[(sepIndex + 1)..];

            if (!string.Equals(claimOrgId, orgIdStr, StringComparison.Ordinal))
                continue;

            if (requirement.AllowedOrgRoles.Contains(roleName, StringComparer.Ordinal))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}
