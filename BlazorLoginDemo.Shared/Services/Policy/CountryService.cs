using BlazorLoginDemo.Shared.Models.Policies;
using BlazorLoginDemo.Shared.Services.Interfaces.Policies;
using Microsoft.EntityFrameworkCore;

namespace BlazorLoginDemo.Shared.Services.Policies;

public sealed class CountryService : ICountryService
{
    private readonly ApplicationDbContext _db;
    public CountryService(ApplicationDbContext db) => _db = db;

    public async Task<Country> CreateAsync(Country country, CancellationToken ct = default)
    {
        if (country is null) throw new ArgumentNullException(nameof(country));

        country.Name    = (country.Name ?? string.Empty).Trim();
        country.IsoCode = (country.IsoCode ?? string.Empty).Trim().ToUpperInvariant();
        country.Flag    = (country.Flag ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(country.Name))    throw new ArgumentException("Name is required.", nameof(country));
        if (string.IsNullOrWhiteSpace(country.IsoCode)) throw new ArgumentException("IsoCode is required.", nameof(country));
        if (string.IsNullOrWhiteSpace(country.Flag))    throw new ArgumentException("Flag is required.", nameof(country));

        bool clash = await _db.Set<Country>().AsNoTracking()
            .AnyAsync(x => x.Name == country.Name || x.IsoCode == country.IsoCode, ct);
        if (clash) throw new InvalidOperationException("A country with the same Name or IsoCode already exists.");

        await _db.Set<Country>().AddAsync(country, ct);
        await _db.SaveChangesAsync(ct);
        return country;
    }

    public async Task<Country?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Set<Country>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Country?> GetByIsoAsync(string isoCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(isoCode)) return null;
        isoCode = isoCode.Trim().ToUpperInvariant();
        return await _db.Set<Country>().AsNoTracking().FirstOrDefaultAsync(x => x.IsoCode == isoCode, ct);
    }

    public async Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken ct = default)
        => await _db.Set<Country>().AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);

    public async Task<IReadOnlyList<Country>> GetAllWithContinentAsync(CancellationToken ct = default)
        => await _db.Set<Country>().AsNoTracking()
            .Include(x => x.Continent)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Country>> GetByContinentAsync(int continentId, CancellationToken ct = default)
        => await _db.Set<Country>().AsNoTracking()
            .Where(x => x.ContinentId == continentId)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    public async Task<Country> UpdateAsync(Country country, CancellationToken ct = default)
    {
        if (country is null) throw new ArgumentNullException(nameof(country));
        if (country.Id <= 0) throw new ArgumentException("Id must be provided for update.", nameof(country));

        country.Name    = (country.Name ?? string.Empty).Trim();
        country.IsoCode = (country.IsoCode ?? string.Empty).Trim().ToUpperInvariant();
        country.Flag    = (country.Flag ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(country.Name))    throw new ArgumentException("Name is required.", nameof(country));
        if (string.IsNullOrWhiteSpace(country.IsoCode)) throw new ArgumentException("IsoCode is required.", nameof(country));
        if (string.IsNullOrWhiteSpace(country.Flag))    throw new ArgumentException("Flag is required.", nameof(country));

        _db.Attach(country);
        _db.Entry(country).State = EntityState.Modified;

        await _db.SaveChangesAsync(ct);
        return country;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = await _db.Set<Country>().FindAsync([id], ct);
        if (existing is null) return false;
        _db.Set<Country>().Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => await _db.Set<Country>().AsNoTracking().AnyAsync(x => x.Id == id, ct);
}
