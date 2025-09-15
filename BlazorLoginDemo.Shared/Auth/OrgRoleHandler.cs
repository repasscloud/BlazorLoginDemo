using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BlazorLoginDemo.Shared.Auth
{
    /// <summary>
    /// Authorizes a user against tenant-scoped actions using:
    /// - Logical org roles (e.g., "OrgPolicyAdmin") supplied via OrgRoleRequirement
    /// - Concrete role membership (e.g., "Client.PolicyAdmin" / "Tmc.PolicyAdmin")
    /// - Scope derived from route values (clientId / tmcId / orgId) and user claims (client_id / tmc_id)
    /// </summary>
    public sealed class OrgRoleHandler : AuthorizationHandler<OrgRoleRequirement>
    {
        private readonly IHttpContextAccessor _http;

        public OrgRoleHandler(IHttpContextAccessor http) => _http = http;

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OrgRoleRequirement requirement)
        {
            var user = context.User;
            if (user is null)
                return Task.CompletedTask;

            // 0) Break-glass / global super (grant immediately)
            if (user.IsInRole("Sudo") || user.IsInRole("Platform.SuperAdmin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // 1) Pull route scope
            var rd = _http.HttpContext?.GetRouteData();
            var targetClientId = rd?.Values.TryGetValue("clientId", out var cObj) == true ? cObj?.ToString() : null;
            var targetTmcId    = rd?.Values.TryGetValue("tmcId",    out var tObj) == true ? tObj?.ToString() : null;
            var targetOrgId    = rd?.Values.TryGetValue("orgId",    out var oObj) == true ? oObj?.ToString() : null;

            // 2) Pull user’s org scope from claims
            var userClientId = user.FindFirst(AppClaimTypes.ClientId)?.Value;
            var userTmcId    = user.FindFirst(AppClaimTypes.TmcId)?.Value;

            // 3) Determine the target scope and id to compare against
            string? scope;   // "Client" | "Tmc"
            string? targetId;

            if (!string.IsNullOrWhiteSpace(targetClientId))
            {
                scope = "Client"; targetId = targetClientId;
            }
            else if (!string.IsNullOrWhiteSpace(targetTmcId))
            {
                scope = "Tmc"; targetId = targetTmcId;
            }
            else if (!string.IsNullOrWhiteSpace(targetOrgId))
            {
                // try to infer by matching user’s own ids
                if (!string.IsNullOrWhiteSpace(userClientId) && targetOrgId == userClientId)
                    (scope, targetId) = ("Client", userClientId);
                else if (!string.IsNullOrWhiteSpace(userTmcId) && targetOrgId == userTmcId)
                    (scope, targetId) = ("Tmc", userTmcId);
                else
                    return Task.CompletedTask; // unknown scope → deny
            }
            else
            {
                return Task.CompletedTask; // no target info → deny
            }

            // 4) Translate logical "Org..." roles into concrete suffixes (e.g., "OrgPolicyAdmin" -> "PolicyAdmin")
            static string ToSuffix(string orgRole)
                => orgRole.StartsWith("Org", StringComparison.Ordinal) ? orgRole.Substring(3) : orgRole;

            var roleSuffixes = requirement.AllowedOrgRoles.Select(ToSuffix).ToArray();

            // 5) Build acceptable concrete role names for this scope
            // - Client scope: allow Client.<suffix> OR Tmc.<suffix> (TMC acting on child client), scope-checked below
            // - TMC scope:    allow Tmc.<suffix> only
            static IEnumerable<string> AcceptableConcreteRoles(string scope, string sfx)
            {
                if (scope == "Client") return new[] { $"Client.{sfx}", $"Tmc.{sfx}" };
                if (scope == "Tmc")    return new[] { $"Tmc.{sfx}" };
                return Array.Empty<string>();
            }

            // 6) Role membership + scope match
            foreach (var sfx in roleSuffixes)
            {
                foreach (var concrete in AcceptableConcreteRoles(scope, sfx))
                {
                    if (!user.IsInRole(concrete)) continue;

                    if (scope == "Client")
                    {
                        // (a) Client user: their client_id must match the route client
                        if (!string.IsNullOrWhiteSpace(userClientId) && userClientId == targetId)
                        {
                            context.Succeed(requirement);
                            return Task.CompletedTask;
                        }

                        // (b) TMC user acting on a client: user.tmc_id must equal the route's tmcId (please include tmcId in routes)
                        if (!string.IsNullOrWhiteSpace(userTmcId) &&
                            !string.IsNullOrWhiteSpace(targetTmcId) &&
                            userTmcId == targetTmcId)
                        {
                            context.Succeed(requirement);
                            return Task.CompletedTask;
                        }
                    }
                    else if (scope == "Tmc")
                    {
                        if (!string.IsNullOrWhiteSpace(userTmcId) && userTmcId == targetId)
                        {
                            context.Succeed(requirement);
                            return Task.CompletedTask;
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
