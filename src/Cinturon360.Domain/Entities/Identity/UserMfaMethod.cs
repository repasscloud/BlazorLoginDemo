using Cinturon360.Domain.Common.Base;
using Cinturon360.Domain.Enums.Security;

namespace Cinturon360.Domain.Entities.Identity;

/// <summary>
/// A registered MFA method for a user. A user can have multiple active MFA methods.
/// </summary>
public sealed class UserMfaMethod : Entity
{
    public string UserId { get; private set; } = string.Empty;
    public MfaMethod MfaMethod { get; private set; }
    public bool IsEnabled { get; private set; } = true;

    /// <summary>Encrypted TOTP secret, phone number, or device token depending on MfaMethod.</summary>
    public string? SecretOrTarget { get; private set; }

    public DateTimeOffset? LastUsedAt { get; private set; }
    public DateTimeOffset? VerifiedAt { get; private set; }

    private UserMfaMethod() { }

    public static UserMfaMethod Create(
        string id,
        string userId,
        MfaMethod method,
        string? secretOrTarget = null)
    {
        return new UserMfaMethod
        {
            Id = id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            UserId = userId,
            MfaMethod = method,
            IsEnabled = true,
            SecretOrTarget = secretOrTarget
        };
    }

    public void Verify()
    {
        VerifiedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RecordUsage() => LastUsedAt = DateTimeOffset.UtcNow;

    public void Disable() { IsEnabled = false; UpdatedAt = DateTimeOffset.UtcNow; }
}
