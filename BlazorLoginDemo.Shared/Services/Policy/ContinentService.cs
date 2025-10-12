using BlazorLoginDemo.Shared.Data;
using BlazorLoginDemo.Shared.Models.Policies;
using BlazorLoginDemo.Shared.Services.Interfaces.Policies;
using Microsoft.EntityFrameworkCore;

namespace BlazorLoginDemo.Shared.Services.Policies;

public sealed class ContinentService : IContinentService
{
    private readonly ApplicationDbContext _db;
    public ContinentService(ApplicationDbContext db) => _db = db;

    public async Task<Continent> CreateAsync(Continent continent, CancellationToken ct = default)
    {
        if (continent is null) throw new ArgumentNullException(nameof(continent));

        continent.Name    = NormalizeName(continent.Name);
        continent.IsoCode = NormalizeIso(continent.IsoCode);

        if (string.IsNullOrWhiteSpace(continent.Name))
            throw new ArgumentException("Name is required.", nameof(continent));
        if (string.IsNullOrWhiteSpace(continent.IsoCode))
            throw new ArgumentException("IsoCode is required.", nameof(continent));

        var nameExists = await _db.Set<Continent>()
            .AsNoTracking()
            .AnyAsync(x => x.Name == continent.Name, ct);
        if (nameExists) throw new InvalidOperationException($"Continent '{continent.Name}' already exists.");

        var codeExists = await _db.Set<Continent>()
            .AsNoTracking()
            .AnyAsync(x => x.IsoCode == continent.IsoCode, ct);
        if (codeExists) throw new InvalidOperationException($"Continent with IsoCode '{continent.IsoCode}' already exists.");

        await _db.Set<Continent>().AddAsync(continent, ct);
        await _db.SaveChangesAsync(ct);
        return continent;
    }

    public async Task<Continent?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Set<Continent>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Continent?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var norm = NormalizeName(name);
        return await _db.Set<Continent>().AsNoTracking().FirstOrDefaultAsync(x => x.Name == norm, ct);
    }

    public async Task<IReadOnlyList<Continent>> GetAllAsync(CancellationToken ct = default)
        => await _db.Set<Continent>().AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);

    public async Task<IReadOnlyList<Country>> GetAssignedCountriesAsync(int id, CancellationToken ct = default)
        => await _db.Set<Country>().AsNoTracking().Where(c => c.ContinentId == id).ToListAsync(ct);

    public async Task<Continent> UpdateAsync(Continent continent, CancellationToken ct = default)
    {
        if (continent is null) throw new ArgumentNullException(nameof(continent));
        if (continent.Id <= 0) throw new ArgumentException("Valid Id required.", nameof(continent));

        var existing = await _db.Set<Continent>().FirstOrDefaultAsync(x => x.Id == continent.Id, ct)
            ?? throw new InvalidOperationException($"Continent {continent.Id} not found.");

        var newName = NormalizeName(continent.Name);
        var newIso  = NormalizeIso(continent.IsoCode);

        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name is required.", nameof(continent));
        if (string.IsNullOrWhiteSpace(newIso))
            throw new ArgumentException("IsoCode is required.", nameof(continent));

        var nameClash = await _db.Set<Continent>().AsNoTracking()
            .AnyAsync(x => x.Id != continent.Id && x.Name == newName, ct);
        if (nameClash) throw new InvalidOperationException($"Continent '{newName}' already exists.");

        var codeClash = await _db.Set<Continent>().AsNoTracking()
            .AnyAsync(x => x.Id != continent.Id && x.IsoCode == newIso, ct);
        if (codeClash) throw new InvalidOperationException($"Continent with IsoCode '{newIso}' already exists.");

        existing.Name    = newName;
        existing.IsoCode = newIso;
        existing.RegionId = continent.RegionId; // nullable, no FK logic here

        await _db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Set<Continent>().FindAsync([id], ct);
        if (entity is null) return false;

        _db.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => await _db.Set<Continent>().AsNoTracking().AnyAsync(x => x.Id == id, ct);

    private static string NormalizeName(string? v) => (v ?? string.Empty).Trim();
    private static string NormalizeIso(string? v)  => (v ?? string.Empty).Trim().ToUpperInvariant();
}
