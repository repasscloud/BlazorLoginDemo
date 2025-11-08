using Cinturon360.Shared.Data;
using Cinturon360.Shared.Models.Policies;
using Cinturon360.Shared.Services.Interfaces.Policies;
using Microsoft.EntityFrameworkCore;

namespace Cinturon360.Shared.Services.Policies;

public sealed class CountryService : ICountryService
{
    private readonly ApplicationDbContext _db;
    public CountryService(ApplicationDbContext db) => _db = db;

    public async Task<Country> CreateAsync(Country country, CancellationToken ct = default)
    {
        if (country is null) throw new ArgumentNullException(nameof(country));

        country.Name    = NormalizeName(country.Name);
        country.IsoCode = NormalizeIso(country.IsoCode);
        country.Flag    = NormalizeFlag(country.Flag);

        if (string.IsNullOrWhiteSpace(country.Name))
            throw new ArgumentException("Name is required.", nameof(country));
        if (string.IsNullOrWhiteSpace(country.IsoCode))
            throw new ArgumentException("IsoCode is required.", nameof(country));
        if (string.IsNullOrWhiteSpace(country.Flag))
            throw new ArgumentException("Flag is required.", nameof(country));

        var nameExists = await _db.Set<Country>().AsNoTracking()
            .AnyAsync(x => x.Name == country.Name, ct);
        if (nameExists) throw new InvalidOperationException($"Country '{country.Name}' already exists.");

        var codeExists = await _db.Set<Country>().AsNoTracking()
            .AnyAsync(x => x.IsoCode == country.IsoCode, ct);
        if (codeExists) throw new InvalidOperationException($"Country with IsoCode '{country.IsoCode}' already exists.");

        await _db.Set<Country>().AddAsync(country, ct);
        await _db.SaveChangesAsync(ct);
        return country;
    }

    public async Task<Country?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Set<Country>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Country?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var norm = NormalizeName(name);
        return await _db.Set<Country>().AsNoTracking().FirstOrDefaultAsync(x => x.Name == norm, ct);
    }

    public async Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken ct = default)
        => await _db.Set<Country>().AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);

    public async Task<Country> UpdateAsync(Country country, CancellationToken ct = default)
    {
        if (country is null) throw new ArgumentNullException(nameof(country));
        if (country.Id <= 0) throw new ArgumentException("Valid Id required.", nameof(country));

        var existing = await _db.Set<Country>().FirstOrDefaultAsync(x => x.Id == country.Id, ct)
            ?? throw new InvalidOperationException($"Country {country.Id} not found.");

        var newName = NormalizeName(country.Name);
        var newIso  = NormalizeIso(country.IsoCode);
        var newFlag = NormalizeFlag(country.Flag);

        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name is required.", nameof(country));
        if (string.IsNullOrWhiteSpace(newIso))
            throw new ArgumentException("IsoCode is required.", nameof(country));
        if (string.IsNullOrWhiteSpace(newFlag))
            throw new ArgumentException("Flag is required.", nameof(country));

        var nameClash = await _db.Set<Country>().AsNoTracking()
            .AnyAsync(x => x.Id != country.Id && x.Name == newName, ct);
        if (nameClash) throw new InvalidOperationException($"Country '{newName}' already exists.");

        var codeClash = await _db.Set<Country>().AsNoTracking()
            .AnyAsync(x => x.Id != country.Id && x.IsoCode == newIso, ct);
        if (codeClash) throw new InvalidOperationException($"Country with IsoCode '{newIso}' already exists.");

        existing.Name       = newName;
        existing.IsoCode    = newIso;
        existing.Flag       = newFlag;
        existing.ContinentId = country.ContinentId; // nullable

        await _db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Set<Country>().FindAsync([id], ct);
        if (entity is null) return false;

        _db.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => await _db.Set<Country>().AsNoTracking().AnyAsync(x => x.Id == id, ct);

    private static string NormalizeName(string? v) => (v ?? string.Empty).Trim();
    private static string NormalizeIso(string? v)  => (v ?? string.Empty).Trim().ToUpperInvariant();
    private static string NormalizeFlag(string? v) => (v ?? string.Empty).Trim();
}
