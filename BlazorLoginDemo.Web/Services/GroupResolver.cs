// Services/GroupResolver.cs
using Microsoft.EntityFrameworkCore;

namespace BlazorLoginDemo.Web.Data;

public interface IGroupResolver
{
    Task<Group> ResolveForEmailAsync(string? email, CancellationToken ct = default);
}

public class GroupResolver : IGroupResolver
{
    private readonly ApplicationDbContext _db;
    public GroupResolver(ApplicationDbContext db) => _db = db;

    public async Task<Group> ResolveForEmailAsync(string? email, CancellationToken ct = default)
    {
        var domain = ExtractDomain(email);
        if (!string.IsNullOrEmpty(domain))
        {
            var match = await _db.GroupDomains
                .Include(gd => gd.Group)
                .Where(gd => gd.Domain == domain && gd.Group.IsActive)
                .Select(gd => gd.Group)
                .FirstOrDefaultAsync(ct);
            if (match is not null) return match;
        }

        // Fallback to catch-all (must exist)
        var catchAll = await _db.Groups.FirstOrDefaultAsync(g => g.IsCatchAll, ct);
        if (catchAll is null)
            throw new InvalidOperationException("Catch-all group not found. Seed GroupX once at startup.");
        return catchAll;
    }

    private static string ExtractDomain(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return string.Empty;
        var at = email.IndexOf('@');
        if (at < 0 || at == email.Length - 1) return string.Empty;
        return email[(at + 1)..].Trim().ToLowerInvariant();
    }
}
