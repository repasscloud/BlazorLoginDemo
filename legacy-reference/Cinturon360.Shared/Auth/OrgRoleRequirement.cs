using Microsoft.AspNetCore.Authorization;

namespace Cinturon360.Shared.Auth;

public sealed class OrgRoleRequirement : IAuthorizationRequirement
{
    public string[] AllowedOrgRoles { get; }
    public OrgRoleRequirement(params string[] allowedOrgRoles) => AllowedOrgRoles = allowedOrgRoles;
}