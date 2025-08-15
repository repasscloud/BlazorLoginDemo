// Data/UserGroupAssignmentInterceptor.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BlazorLoginDemo.Web.Data;

public class UserGroupAssignmentInterceptor : SaveChangesInterceptor
{
    private readonly IGroupResolver _resolver;

    public UserGroupAssignmentInterceptor(IGroupResolver resolver) => _resolver = resolver;

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var ctx = eventData.Context;
        if (ctx is null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

        // Only for new users; do NOT run on updates like LastSeenUtc
        IEnumerable<EntityEntry<ApplicationUser>> newUsers =
            ctx.ChangeTracker.Entries<ApplicationUser>()
               .Where(e => e.State == EntityState.Added);

        foreach (var entry in newUsers)
        {
            var user = entry.Entity;
            if (user.GroupId == null)
            {
                var groupId = await _resolver.ResolveGroupIdForEmailAsync(user.Email ?? user.UserName, cancellationToken);
                if (groupId.HasValue) user.GroupId = groupId.Value;
            }
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
