// Data/UserGroupAssignmentInterceptor.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BlazorLoginDemo.Web.Data;

public class UserGroupAssignmentInterceptor : SaveChangesInterceptor
{
    private readonly IGroupResolver _resolver;
    public UserGroupAssignmentInterceptor(IGroupResolver resolver) => _resolver = resolver;

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        if (eventData.Context is not ApplicationDbContext db) return await base.SavingChangesAsync(eventData, result, ct);

        var newUsers = db.ChangeTracker.Entries<ApplicationUser>()
            .Where(e => e.State == EntityState.Added && e.Entity.GroupId == null)
            .Select(e => e.Entity)
            .ToList();

        foreach (var u in newUsers)
        {
            var grp = await _resolver.ResolveForEmailAsync(u.Email ?? u.UserName, ct);
            u.GroupId = grp.Id;
        }

        return await base.SavingChangesAsync(eventData, result, ct);
    }
}
