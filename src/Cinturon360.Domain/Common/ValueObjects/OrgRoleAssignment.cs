namespace Cinturon360.Domain.Common.ValueObjects;

/// <summary>
/// Represents an org-scoped role assignment with optional expiry.
/// </summary>
public sealed record OrgRoleAssignment(
    string OrgId,
    string RoleName,
    DateTimeOffset? ExpiresAt = null)
{
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTimeOffset.UtcNow;
}
