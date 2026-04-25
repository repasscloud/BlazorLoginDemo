using Cinturon360.Domain.Common.Base;
using Cinturon360.Domain.Enums.Security;

namespace Cinturon360.Domain.Entities.Identity;

/// <summary>
/// Personal Access Token or Service Account token.
/// Hash only stored after creation — plaintext is shown once.
/// </summary>
public sealed class UserApiToken : Entity
{
    public string UserId { get; private set; } = string.Empty;
    public TokenClass TokenClass { get; private set; }

    public string Name { get; private set; } = string.Empty;

    /// <summary>SHA-256 hash of the token. Plaintext is never stored.</summary>
    public string TokenHash { get; private set; } = string.Empty;

    /// <summary>Token prefix shown in the UI to help user identify the token (e.g. "c360pat_abc123...").</summary>
    public string TokenPrefix { get; private set; } = string.Empty;

    public DateTimeOffset? ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public DateTimeOffset? LastUsedAt { get; private set; }

    /// <summary>Comma-separated permission scopes granted to this token.</summary>
    public string? Scopes { get; private set; }

    private UserApiToken() { }

    public static UserApiToken Create(
        string id,
        string userId,
        TokenClass tokenClass,
        string name,
        string tokenHash,
        string tokenPrefix,
        DateTimeOffset? expiresAt = null,
        string? scopes = null)
    {
        return new UserApiToken
        {
            Id = id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            UserId = userId,
            TokenClass = tokenClass,
            Name = name,
            TokenHash = tokenHash,
            TokenPrefix = tokenPrefix,
            ExpiresAt = expiresAt,
            Scopes = scopes
        };
    }

    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RecordUsage() => LastUsedAt = DateTimeOffset.UtcNow;

    public bool IsExpired => ExpiresAt.HasValue && DateTimeOffset.UtcNow >= ExpiresAt.Value;
    public bool IsValid   => !IsRevoked && !IsExpired;
}
