using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Cinturon360.Application.Abstractions.Security;
using AppClaims = Cinturon360.Common.Constants.ClaimTypes;

namespace Cinturon360.Infrastructure.Security;

/// <summary>
/// Resolves the current user from the ASP.NET Core HTTP context.
/// </summary>
public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public string? UserId => Principal?.FindFirstValue(AppClaims.UserId);
    public string? OrgId  => Principal?.FindFirstValue(AppClaims.OrgId);

    public bool IsAuthenticated
        => Principal?.Identity?.IsAuthenticated ?? false;

    public bool IsPlatformUser
        => Principal?.HasClaim(AppClaims.AppRole, "Platform") ?? false;

    public IReadOnlyList<string> Permissions
        => Principal?.FindAll("perm").Select(c => c.Value).ToList() ?? [];

    public bool HasPermission(string permissionCode)
        => Permissions.Contains(permissionCode);
}
