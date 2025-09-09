using BlazorLoginDemo.Shared.Models.Policies;
using BlazorLoginDemo.Shared.Services.Interfaces.Policies;
using Microsoft.EntityFrameworkCore;

namespace BlazorLoginDemo.Shared.Services.Policies;

public sealed class ContinentService : IContinentService
{
    private readonly ApplicationDbContext _db;

    public ContinentService(ApplicationDbContext db) => _db = db;

    // -----------------------------
    // CREATE
    // -----------------------------
    public async Task<Continent> CreateAsync(Continent continent, CancellationToken ct = default)
    {
        if (continent is null) throw new ArgumentNullException(nameof(continent));

        // Required by model annotations; normalize input to match unique indices on Name/IsoCode
        // (Continent has unique indices for Name and IsoCode). :contentReference[oaicite:3]{index=3}
        continent.Name = (continent.Name ?? string.Empty).Trim();
        continent.IsoCode = (continent.IsoCode ?? string.Empty).Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(continent.Name))
            throw new ArgumentException("Name must be provided.", nameof(continent));
        if (string.IsNullOrWhiteSpace(continent.IsoCode))
            throw new ArgumentException("IsoCode must be provided.", nameof(continent));

        // Optional early uniqueness check (DB indices will enforce at save time as well)
        bool clash = await _db.Set<Continent>().AsNoTracking()
            .AnyAsync(x => x.Name == continent.Name || x.IsoCode == continent.IsoCode, ct);
        if (clash) throw new InvalidOperationException("A continent with the same Name or IsoCode already exists.");

        await _db.Set<Continent>().AddAsync(continent, ct);
        await _db.SaveChangesAsync(ct);
        return continent;
    }

    // -----------------------------
    // READ
    // -----------------------------
    public async Task<Continent?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Set<Continent>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Continent?> GetByIsoAsync(string isoCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(isoCode)) return null;
        isoCode = isoCode.Trim().ToUpperInvariant();
        return await _db.Set<Continent>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsoCode == isoCode, ct);
    }

    public async Task<IReadOnlyList<Continent>> GetAllAsync(CancellationToken ct = default)
        => await _db.Set<Continent>()
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Continent>> GetAllWithCountriesAsync(CancellationToken ct = default)
        => await _db.Set<Continent>()
            .AsNoTracking()
            .Include(x => x.Countries) // uses Continent.Countries nav property. :contentReference[oaicite:4]{index=4}
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    // -----------------------------
    // UPDATE
    // -----------------------------
    public async Task<Continent> UpdateAsync(Continent continent, CancellationToken ct = default)
    {
        if (continent is null) throw new ArgumentNullException(nameof(continent));
        if (continent.Id <= 0) throw new ArgumentException("Id must be provided for update.", nameof(continent));

        continent.Name = (continent.Name ?? string.Empty).Trim();
        continent.IsoCode = (continent.IsoCode ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(continent.Name))
            throw new ArgumentException("Name must be provided.", nameof(continent));
        if (string.IsNullOrWhiteSpace(continent.IsoCode))
            throw new ArgumentException("IsoCode must be provided.", nameof(continent));

        _db.Attach(continent);
        _db.Entry(continent).State = EntityState.Modified;

        await _db.SaveChangesAsync(ct);
        return continent;
    }

    // -----------------------------
    // DELETE
    // -----------------------------
    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = await _db.Set<Continent>().FindAsync([id], ct);
        if (existing is null) return false;

        _db.Set<Continent>().Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // -----------------------------
    // UTIL
    // -----------------------------
    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => await _db.Set<Continent>().AsNoTracking().AnyAsync(x => x.Id == id, ct);
}
