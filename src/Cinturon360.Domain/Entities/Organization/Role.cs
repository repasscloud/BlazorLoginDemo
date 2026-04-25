using Cinturon360.Domain.Common.Base;
using Cinturon360.Domain.Enums.System;

namespace Cinturon360.Domain.Entities.Organization;

/// <summary>
/// An org-scoped role definition. System roles are seeded and cannot be deleted.
/// Custom roles can be created per org.
/// </summary>
public sealed class Role : SoftDeletableEntity
{
    public OrgType OrgType { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsSystemRole { get; private set; }

    /// <summary>Null for platform-wide system roles; set for org-specific custom roles.</summary>
    public string? OrgId { get; private set; }

    private Role() { }

    public static Role CreateSystem(string id, OrgType orgType, string name, string? description = null)
    {
        return new Role
        {
            Id = id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            OrgType = orgType,
            Name = name,
            Description = description,
            IsSystemRole = true
        };
    }

    public static Role CreateCustom(string id, string orgId, OrgType orgType, string name, string? description = null)
    {
        return new Role
        {
            Id = id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            OrgId = orgId,
            OrgType = orgType,
            Name = name,
            Description = description,
            IsSystemRole = false
        };
    }
}
