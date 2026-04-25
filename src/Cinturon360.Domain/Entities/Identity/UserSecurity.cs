using Cinturon360.Domain.Common.Base;

namespace Cinturon360.Domain.Entities.Identity;

/// <summary>
/// Credential and MFA state for a user. One row per user. Never returned in API responses.
/// </summary>
public sealed class UserSecurity : Entity
{
    public string UserId { get; private set; } = string.Empty;

    // ── Password ──────────────────────────────────────────────────────────
    public string? PasswordHash { get; private set; }
    public DateTimeOffset? PasswordChangedAt { get; private set; }
    public DateTimeOffset? PasswordExpiresAt { get; private set; }

    // ── MFA ───────────────────────────────────────────────────────────────
    public bool IsMfaEnabled { get; private set; }
    public bool IsMfaRequired { get; private set; }

    // ── Direct login policy ───────────────────────────────────────────────
    /// <summary>
    /// When true, direct (local) login is allowed even if SSO is configured for the org.
    /// Used to prevent platform admins being locked out via tenant SSO.
    /// </summary>
    public bool AllowDirectLogin { get; private set; } = true;

    // ── Lockout tracking ──────────────────────────────────────────────────
    public int FailedLoginAttempts { get; private set; }
    public DateTimeOffset? LockoutUntil { get; private set; }

    private UserSecurity() { }

    public static UserSecurity Create(string id, string userId, bool allowDirectLogin = true)
    {
        return new UserSecurity
        {
            Id = id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            UserId = userId,
            AllowDirectLogin = allowDirectLogin
        };
    }

    public void SetPasswordHash(string hash)
    {
        PasswordHash = hash;
        PasswordChangedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void EnableMfa()  { IsMfaEnabled = true;  UpdatedAt = DateTimeOffset.UtcNow; }
    public void DisableMfa() { IsMfaEnabled = false; UpdatedAt = DateTimeOffset.UtcNow; }

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ClearFailedLogins()
    {
        FailedLoginAttempts = 0;
        LockoutUntil = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetLockout(DateTimeOffset until)
    {
        LockoutUntil = until;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
