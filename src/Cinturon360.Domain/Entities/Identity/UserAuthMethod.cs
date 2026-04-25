using Cinturon360.Domain.Common.Base;
using Cinturon360.Domain.Enums.Security;

namespace Cinturon360.Domain.Entities.Identity;

/// <summary>
/// A single authentication method registered for a user.
/// A user can have multiple methods (local password + Google + OIDC, etc.).
/// </summary>
public sealed class UserAuthMethod : Entity
{
    public string UserId { get; private set; } = string.Empty;
    public LoginMethod LoginMethod { get; private set; }
    public AuthMethodStatus Status { get; private set; } = AuthMethodStatus.Active;

    /// <summary>External subject identifier (e.g. Google sub, Microsoft oid, SAML nameId).</summary>
    public string? ExternalSubject { get; private set; }

    /// <summary>Tenant config ID for OIDC/SAML providers.</summary>
    public string? ProviderConfigId { get; private set; }

    public DateTimeOffset? LastUsedAt { get; private set; }

    private UserAuthMethod() { }

    public static UserAuthMethod Create(
        string id,
        string userId,
        LoginMethod method,
        string? externalSubject = null,
        string? providerConfigId = null)
    {
        return new UserAuthMethod
        {
            Id = id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            UserId = userId,
            LoginMethod = method,
            Status = AuthMethodStatus.Active,
            ExternalSubject = externalSubject,
            ProviderConfigId = providerConfigId
        };
    }

    public void RecordUsage() => LastUsedAt = DateTimeOffset.UtcNow;

    public void Disable() { Status = AuthMethodStatus.Disabled; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Enable()  { Status = AuthMethodStatus.Active;   UpdatedAt = DateTimeOffset.UtcNow; }
}
