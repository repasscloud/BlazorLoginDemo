using BlazorLoginDemo.Shared.Models.User;
using BlazorLoginDemo.Shared.Services.Interfaces.User;
using Microsoft.EntityFrameworkCore;
using BlazorLoginDemo.Shared.Data;
using BlazorLoginDemo.Shared.Models.Kernel.User;

namespace BlazorLoginDemo.Shared.Services.User;

public sealed class AvaUserService : IAvaUserService
{
    private readonly ApplicationDbContext _db;

    public AvaUserService(ApplicationDbContext db) => _db = db;

    // -----------------------------
    // CREATE
    // -----------------------------
    public async Task<AvaUser> CreateAsync(AvaUser user, CancellationToken ct = default)
    {
        await _db.AvaUsers.AddAsync(user, ct);
        await _db.SaveChangesAsync(ct);
        return user;
    }

    // -----------------------------
    // READ
    // -----------------------------
    public async Task<AvaUser?> GetByIdAsync(string id, CancellationToken ct = default)
        => await _db.AvaUsers.FindAsync([id], ct);

    public async Task<AvaUser?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;
        var e = email.Trim();
        // case-insensitive exact match using ILIKE
        return await _db.AvaUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => EF.Functions.ILike(u.Email, e), ct);
    }

    public async Task<AvaUser?> GetByAspNetUserIdAsync(string aspNetUsersId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(aspNetUsersId)) return null;
        var id = aspNetUsersId.Trim();
        return await _db.AvaUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.AspNetUsersId == id, ct);
    }

    public async Task<IReadOnlyList<AvaUser>> GetAllAsync(CancellationToken ct = default)
        => await _db.AvaUsers
            .AsNoTracking()
            .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AvaUser>> SearchUsersAsync(string query, int page = 0, int take = 50, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<AvaUser>();

        var q = query.Trim();
        var pattern = $"%{q}%";

        var filtered = _db.AvaUsers
            .AsNoTracking()
            .Where(u =>
                EF.Functions.ILike(u.Email ?? string.Empty, pattern) ||
                EF.Functions.ILike(u.FirstName ?? string.Empty, pattern) ||
                EF.Functions.ILike(u.LastName ?? string.Empty, pattern));

        return await filtered
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Skip(page * take)        // 👈 shift starting point
            .Take(Math.Max(1, take))  // 👈 how many to grab
            .ToListAsync(ct);
    }

    // -----------------------------
    // UPDATE (replace whole object)
    // -----------------------------
    public async Task<AvaUser> UpdateAsync(AvaUser user, CancellationToken ct = default)
    {
        _db.Attach(user);
        _db.Entry(user).State = EntityState.Modified;

        await _db.SaveChangesAsync(ct);
        return user;
    }

    // -----------------------------
    // DELETE
    // -----------------------------
    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var existing = await _db.AvaUsers.FindAsync([id], ct);
        if (existing is null) return false;

        _db.AvaUsers.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // -----------------------------
    // UTIL
    // -----------------------------
    public async Task<bool> ExistsAsync(string id, CancellationToken ct = default)
        => await _db.AvaUsers.AsNoTracking().AnyAsync(u => u.Id == id, ct);

    public async Task<bool> AssignTravelPolicyToUserAsync(
        string id,
        string travelPolicyId,
        CancellationToken ct = default)
    {
        // read policy name only
        var policy = await _db.TravelPolicies
            .AsNoTracking()
            .Where(p => p.Id == travelPolicyId)
            .Select(p => new { p.Id, p.PolicyName })
            .SingleOrDefaultAsync(ct);
        if (policy is null) return false;

        // user (tracked)
        var usr = await _db.AvaUsers.FindAsync([id], ct);
        if (usr is null) return false;

        // 1) update AvaUser (force write)
        usr.TravelPolicyId = travelPolicyId;
        _db.Entry(usr).Property(x => x.TravelPolicyId).IsModified = true;
        await _db.SaveChangesAsync(ct);

        // 2) update SysPref only if it exists (force write)
        if (!string.IsNullOrWhiteSpace(usr.AvaUserSysPreferenceId))
        {
            var usp = await _db.AvaUserSysPreferences.FindAsync([usr.AvaUserSysPreferenceId!], ct);
            if (usp is not null)
            {
                usp.TravelPolicyId   = policy.Id;
                usp.TravelPolicyName = policy.PolicyName;

                _db.Entry(usp).Property(x => x.TravelPolicyId).IsModified   = true;
                _db.Entry(usp).Property(x => x.TravelPolicyName).IsModified = true;

                await _db.SaveChangesAsync(ct);
            }
        }

        return true;
    }





    public async Task<int> IngestUsersAsync(CancellationToken ct = default)
    {
        // 1) Find Identity users that don't have an AvaUser profile yet
        var missing = await (from u in _db.Users.AsNoTracking() // AspNetUsers via Identity
                                join au in _db.AvaUsers.AsNoTracking()
                                    on u.Id equals au.AspNetUsersId into aug
                                from au in aug.DefaultIfEmpty()
                                where au == null
                                select new
                                {
                                    u.Id,
                                    u.Email,
                                    u.UserName,
                                    // If you store human name in Identity:
                                    u.DisplayName,
                                    u.FirstName,
                                    u.LastName,
                                })
                                .ToListAsync(ct);

        if (missing.Count == 0) return 0;

        // 2) Bulk-add AvaUser rows
        var prevDetect = _db.ChangeTracker.AutoDetectChangesEnabled;
        _db.ChangeTracker.AutoDetectChangesEnabled = false;
        try
        {
            foreach (var m in missing)
            {
                
                var (first, last) = SplitDisplayName(m.DisplayName);
                _db.AvaUsers.Add(new AvaUser
                {
                    // NOTE: set Id if your AvaUser key isn't DB-generated
                    AspNetUsersId = m.Id,
                    Email = m.Email ?? m.UserName ?? string.Empty,
                    FirstName = m.FirstName ?? first,
                    LastName = m.LastName ?? last,
                    OriginLocationCode = "SYD"
                });
            }

            return await _db.SaveChangesAsync(ct);
        }
        finally
        {
            _db.ChangeTracker.AutoDetectChangesEnabled = prevDetect;
        }

    }

    public async Task<bool> AssignAvaClientToUserAsync(string id, string clientId, CancellationToken ct = default)
    {
        var usr = await _db.AvaUsers.FindAsync([id], ct);
        if (usr == null) return false;

        usr.AvaClientId = clientId;
        await _db.SaveChangesAsync(ct);
        return true;
    }
    private static (string First, string Last) SplitDisplayName(string? display)
    {
        if (string.IsNullOrWhiteSpace(display)) return (string.Empty, string.Empty);
        var parts = display.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => (string.Empty, string.Empty),
            1 => (parts[0], string.Empty),
            _ => (parts[0], string.Join(' ', parts.Skip(1)))
        };
    }
}
