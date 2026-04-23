using Cinturon360.Domain.Common.Base;
using Cinturon360.Domain.Enums.System;

namespace Cinturon360.Domain.Entities.Organization;

/// <summary>
/// A tenant organisation. Can be a Vendor, TMC, or Client type.
/// All data, roles, and billing is scoped by OrgId.
/// </summary>
public sealed class Organisation : SoftDeletableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public OrgType OrgType { get; private set; }
    public string? ParentOrgId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? LogoStorageKey { get; private set; }

    private Organisation() { }

    public static Organisation Create(string id, string name, string slug, OrgType orgType, string? parentOrgId = null)
    {
        return new Organisation
        {
            Id = id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Name = name.Trim(),
            Slug = slug.ToLowerInvariant().Trim(),
            OrgType = orgType,
            ParentOrgId = parentOrgId,
            IsActive = true
        };
    }
}
