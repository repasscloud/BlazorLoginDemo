using Cinturon360.Domain.Common.Base;

namespace Cinturon360.Domain.Entities.Identity;

/// <summary>
/// Hashed recovery codes for account recovery. One-time use, never stored in plaintext.
/// </summary>
public sealed class UserRecoveryCode : Entity
{
    public string UserId { get; private set; } = string.Empty;

    /// <summary>BCrypt or similar hash of the recovery code. Plaintext is shown once at generation.</summary>
    public string CodeHash { get; private set; } = string.Empty;

    public bool IsUsed { get; private set; }
    public DateTimeOffset? UsedAt { get; private set; }

    private UserRecoveryCode() { }

    public static UserRecoveryCode Create(string id, string userId, string codeHash)
    {
        return new UserRecoveryCode
        {
            Id = id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            UserId = userId,
            CodeHash = codeHash
        };
    }

    public void MarkUsed()
    {
        IsUsed = true;
        UsedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
