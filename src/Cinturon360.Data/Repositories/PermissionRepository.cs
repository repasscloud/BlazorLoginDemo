using Microsoft.EntityFrameworkCore;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Data.Context;

namespace Cinturon360.Data.Repositories;

public sealed class PermissionRepository(AppDbContext db) : IPermissionRepository
{
    public async Task<IReadOnlyList<string>> GetPermissionsForUserAsync(
        string userId,
        CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        var permissions = await db.UserRoleAssignments
            .AsNoTracking()
            .Where(a => a.UserId == userId
                     && a.IsActive
                     && (a.ExpiresAt == null || a.ExpiresAt > now))
            .Join(db.RolePermissions.AsNoTracking(),
                  assignment => assignment.RoleId,
                  rp        => rp.RoleId,
                  (_, rp)   => rp.PermissionCode)
            .Distinct()
            .ToListAsync(ct);

        return permissions;
    }
}
