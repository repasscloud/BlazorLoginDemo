using Cinturon360.Domain.Common.Base;
using Cinturon360.Domain.Enums.Security;

namespace Cinturon360.Domain.Entities.Identity;

/// <summary>
/// Assigns an org-scoped role to a user.
/// A user may have multiple assignments across orgs they have access to within their hierarchy.
/// </summary>
public sealed class UserRoleAssignment : Entity
{
    public string UserId { get; private set; } = string.Empty;
    public string OrgId { get; private set; } = string.Empty;
    public string RoleId { get; private set; } = string.Empty;
    public ScopeMode ScopeMode { get; private set; }
    public bool IsActive { get; private set; } = true;

    /// <summary>Who granted this assignment.</summary>
    public string? GrantedByUserId { get; private set; }

    public DateTimeOffset? ExpiresAt { get; private set; }

    private UserRoleAssignment() { }

    public static UserRoleAssignment Create(
        string id,
        string userId,
        string orgId,
        string roleId,
        ScopeMode scopeMode,
        string? grantedByUserId = null,
        DateTimeOffset? expiresAt = null)
    {
        return new UserRoleAssignment
        {
            Id = id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            UserId = userId,
            OrgId = orgId,
            RoleId = roleId,
            ScopeMode = scopeMode,
            GrantedByUserId = grantedByUserId,
            ExpiresAt = expiresAt
        };
    }

    public void Revoke() { IsActive = false; UpdatedAt = DateTimeOffset.UtcNow; }
}
