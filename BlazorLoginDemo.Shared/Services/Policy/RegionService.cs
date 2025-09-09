using BlazorLoginDemo.Shared.Models.Policies;
using BlazorLoginDemo.Shared.Services.Interfaces.Policies;
using Microsoft.EntityFrameworkCore;

namespace BlazorLoginDemo.Shared.Services.Policies;

public sealed class RegionService : IRegionService
{
    private readonly ApplicationDbContext _db;
    public RegionService(ApplicationDbContext db) => _db = db;

    public async Task<Region> CreateAsync(Region region, CancellationToken ct = default)
    {
        if (region is null) throw new ArgumentNullException(nameof(region));
        region.Name = (region.Name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(region.Name))
            throw new ArgumentException("Name is required.", nameof(region));

        bool clash = await _db.Set<Region>().AsNoTracking()
            .AnyAsync(x => x.Name == region.Name, ct);
        if (clash) throw new InvalidOperationException("A region with the same Name already exists.");

        await _db.Set<Region>().AddAsync(region, ct);
        await _db.SaveChangesAsync(ct);
        return region;
    }

    public async Task<Region?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Set<Region>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Region?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        name = name.Trim();
        return await _db.Set<Region>().AsNoTracking().FirstOrDefaultAsync(x => x.Name == name, ct);
    }

    public async Task<IReadOnlyList<Region>> GetAllAsync(CancellationToken ct = default)
        => await _db.Set<Region>().AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);

    public async Task<IReadOnlyList<Region>> GetAllWithContinentsAsync(CancellationToken ct = default)
        => await _db.Set<Region>().AsNoTracking()
            .Include(x => x.Continents)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    public async Task<Region> UpdateAsync(Region region, CancellationToken ct = default)
    {
        if (region is null) throw new ArgumentNullException(nameof(region));
        if (region.Id <= 0) throw new ArgumentException("Id must be provided for update.", nameof(region));

        region.Name = (region.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(region.Name))
            throw new ArgumentException("Name is required.", nameof(region));

        _db.Attach(region);
        _db.Entry(region).State = EntityState.Modified;

        await _db.SaveChangesAsync(ct);
        return region;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = await _db.Set<Region>().FindAsync([id], ct);
        if (existing is null) return false;

        _db.Set<Region>().Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => await _db.Set<Region>().AsNoTracking().AnyAsync(x => x.Id == id, ct);
}
