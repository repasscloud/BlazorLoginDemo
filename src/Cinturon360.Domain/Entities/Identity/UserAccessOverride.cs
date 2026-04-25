using Cinturon360.Domain.Common.Base;

namespace Cinturon360.Domain.Entities.Identity;

/// <summary>
/// Per-user policy exceptions. Avoids 50-column bloat on the User table.
/// Each row is a named override with a typed value.
/// </summary>
public sealed class UserAccessOverride : Entity
{
    public string UserId { get; private set; } = string.Empty;

    /// <summary>Named policy key (e.g. "AllowInternationalBookings", "MaxApprovalLevel").</summary>
    public string PolicyKey { get; private set; } = string.Empty;

    public string PolicyValue { get; private set; } = string.Empty;

    /// <summary>Who granted the override.</summary>
    public string? GrantedByUserId { get; private set; }

    public DateTimeOffset? ExpiresAt { get; private set; }
    public bool IsActive { get; private set; } = true;

    private UserAccessOverride() { }

    public static UserAccessOverride Create(
        string id,
        string userId,
        string policyKey,
        string policyValue,
        string? grantedByUserId = null,
        DateTimeOffset? expiresAt = null)
    {
        return new UserAccessOverride
        {
            Id = id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            UserId = userId,
            PolicyKey = policyKey,
            PolicyValue = policyValue,
            GrantedByUserId = grantedByUserId,
            ExpiresAt = expiresAt
        };
    }

    public void Revoke() { IsActive = false; UpdatedAt = DateTimeOffset.UtcNow; }
}
