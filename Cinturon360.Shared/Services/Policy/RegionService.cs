using Cinturon360.Shared.Data;
using Cinturon360.Shared.Models.Policies;
using Cinturon360.Shared.Services.Interfaces.Policies;
using Microsoft.EntityFrameworkCore;

namespace Cinturon360.Shared.Services.Policies;

public sealed class RegionService : IRegionService
{
    private readonly ApplicationDbContext _db;
    public RegionService(ApplicationDbContext db) => _db = db;

    public async Task<Region> CreateAsync(Region region, CancellationToken ct = default)
    {
        if (region is null) throw new ArgumentNullException(nameof(region));
        region.Name = Normalize(region.Name);

        if (string.IsNullOrWhiteSpace(region.Name))
            throw new ArgumentException("Name is required.", nameof(region));

        var exists = await _db.Set<Region>()
            .AsNoTracking()
            .AnyAsync(r => r.Name == region.Name, ct);
        if (exists) throw new InvalidOperationException($"Region '{region.Name}' already exists.");

        await _db.Set<Region>().AddAsync(region, ct);
        await _db.SaveChangesAsync(ct);
        return region;
    }

    public async Task<Region?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Set<Region>().AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<Region?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        name = Normalize(name);
        return await _db.Set<Region>().AsNoTracking().FirstOrDefaultAsync(r => r.Name == name, ct);
    }

    public async Task<IReadOnlyList<Region>> GetAllAsync(CancellationToken ct = default)
        => await _db.Set<Region>().AsNoTracking().OrderBy(r => r.Name).ToListAsync(ct);

    public async Task<IReadOnlyList<Continent>> GetAssignedContinentsAsync(int id, CancellationToken ct = default)
        => await _db.Set<Continent>().AsNoTracking().Where(c => c.RegionId == id).ToListAsync(ct);

    public async Task<Region> UpdateAsync(Region region, CancellationToken ct = default)
    {
        if (region is null) throw new ArgumentNullException(nameof(region));
        if (region.Id <= 0) throw new ArgumentException("Valid Id required.", nameof(region));

        var existing = await _db.Set<Region>().FirstOrDefaultAsync(r => r.Id == region.Id, ct)
            ?? throw new InvalidOperationException($"Region {region.Id} not found.");

        var newName = Normalize(region.Name);
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name is required.", nameof(region));

        var nameClash = await _db.Set<Region>().AsNoTracking()
            .AnyAsync(r => r.Id != region.Id && r.Name == newName, ct);
        if (nameClash) throw new InvalidOperationException($"Region '{newName}' already exists.");

        existing.Name = newName;

        await _db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Set<Region>().FindAsync([id], ct);
        if (entity is null) return false;

        _db.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => await _db.Set<Region>().AsNoTracking().AnyAsync(r => r.Id == id, ct);

    private static string Normalize(string? value) => (value ?? string.Empty).Trim();
}
