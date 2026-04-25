using Cinturon360.Domain.Common.Base;
using Cinturon360.Domain.Enums.Security;

namespace Cinturon360.Domain.Entities.Identity;

/// <summary>
/// Records how a user was provisioned. One row per provisioning event per user.
/// </summary>
public sealed class UserProvisioningSource : Entity
{
    public string UserId { get; private set; } = string.Empty;
    public ProvisioningSource Source { get; private set; }

    /// <summary>For JIT/SCIM: the IdP or SCIM client identifier.</summary>
    public string? SourceIdentifier { get; private set; }

    /// <summary>For CSV import: the import batch ID.</summary>
    public string? BatchId { get; private set; }

    /// <summary>Who triggered the provisioning if done manually.</summary>
    public string? ProvisionedByUserId { get; private set; }

    private UserProvisioningSource() { }

    public static UserProvisioningSource Create(
        string id,
        string userId,
        ProvisioningSource source,
        string? sourceIdentifier = null,
        string? batchId = null,
        string? provisionedByUserId = null)
    {
        return new UserProvisioningSource
        {
            Id = id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            UserId = userId,
            Source = source,
            SourceIdentifier = sourceIdentifier,
            BatchId = batchId,
            ProvisionedByUserId = provisionedByUserId
        };
    }
}
