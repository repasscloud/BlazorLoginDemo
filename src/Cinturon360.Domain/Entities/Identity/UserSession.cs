using Cinturon360.Domain.Common.Base;
using Cinturon360.Domain.Enums.Security;

namespace Cinturon360.Domain.Entities.Identity;

/// <summary>
/// An authenticated session for a user. Tracks JTI for token revocation.
/// </summary>
public sealed class UserSession : Entity
{
    public string UserId { get; private set; } = string.Empty;
    public TokenClass TokenClass { get; private set; }

    /// <summary>JWT ID — used to validate and revoke tokens.</summary>
    public string Jti { get; private set; } = string.Empty;

    /// <summary>Device identifier for mobile sessions.</summary>
    public string? DeviceId { get; private set; }

    /// <summary>User agent / client description.</summary>
    public string? UserAgent { get; private set; }

    /// <summary>IP at session creation time.</summary>
    public string? IpAddress { get; private set; }

    public bool IsRevoked { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? RevokedReason { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? LastActiveAt { get; private set; }

    private UserSession() { }

    public static UserSession Create(
        string id,
        string userId,
        TokenClass tokenClass,
        string jti,
        DateTimeOffset expiresAt,
        string? deviceId = null,
        string? userAgent = null,
        string? ipAddress = null)
    {
        return new UserSession
        {
            Id = id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            UserId = userId,
            TokenClass = tokenClass,
            Jti = jti,
            ExpiresAt = expiresAt,
            DeviceId = deviceId,
            UserAgent = userAgent,
            IpAddress = ipAddress
        };
    }

    public void Revoke(string reason)
    {
        IsRevoked = true;
        RevokedAt = DateTimeOffset.UtcNow;
        RevokedReason = reason;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RecordActivity() => LastActiveAt = DateTimeOffset.UtcNow;

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsValid   => !IsRevoked && !IsExpired;
}
