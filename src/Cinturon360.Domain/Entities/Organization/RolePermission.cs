using Cinturon360.Domain.Common.Base;

namespace Cinturon360.Domain.Entities.Organization;

/// <summary>
/// Maps a permission code to a role. Permission codes are string constants (e.g. "bookings.read").
/// </summary>
public sealed class RolePermission : Entity
{
    public string RoleId { get; private set; } = string.Empty;

    /// <summary>e.g. "bookings.read", "bookings.manage", "reports.finance.read"</summary>
    public string PermissionCode { get; private set; } = string.Empty;

    private RolePermission() { }

    public static RolePermission Create(string id, string roleId, string permissionCode)
    {
        return new RolePermission
        {
            Id = id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            RoleId = roleId,
            PermissionCode = permissionCode
        };
    }
}
