using BlazorLoginDemo.Shared.Models.User;
using BlazorLoginDemo.Shared.Services.Interfaces.User;
using Microsoft.EntityFrameworkCore;

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

    public async Task<IReadOnlyList<AvaUser>> SearchUsersAsync(string query, int take = 50, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query)) return Array.Empty<AvaUser>();
        var q = query.Trim();
        var pattern = $"%{q}%";

        var filtered = _db.AvaUsers
            .AsNoTracking()
            .Where(u =>
                EF.Functions.ILike(u.Email ?? string.Empty, pattern) ||
                EF.Functions.ILike(u.FirstName ?? string.Empty, pattern) ||
                EF.Functions.ILike(u.LastName ?? string.Empty, pattern));

        return await filtered
            .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
            .Take(Math.Max(1, take))
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
}
