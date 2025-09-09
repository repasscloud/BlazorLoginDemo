using BlazorLoginDemo.Shared.Models.Kernel.Travel;
using BlazorLoginDemo.Shared.Models.Static;
using Microsoft.EntityFrameworkCore;

namespace BlazorLoginDemo.Shared.Services.Kernel;

public sealed class AirportInfoService : IAirportInfoService
{
    private readonly ApplicationDbContext _db;
    public AirportInfoService(ApplicationDbContext db) => _db = db;

    // -----------------------------
    // CREATE
    // -----------------------------
    public async Task<AirportInfo> CreateAsync(AirportInfo airport, CancellationToken ct = default)
    {
        if (airport is null) throw new ArgumentNullException(nameof(airport));
        Normalize(airport);

        if (string.IsNullOrWhiteSpace(airport.Ident))
            throw new ArgumentException("Ident is required.", nameof(airport));
        if (string.IsNullOrWhiteSpace(airport.Name))
            throw new ArgumentException("Name is required.", nameof(airport));

        await _db.Set<AirportInfo>().AddAsync(airport, ct);
        await _db.SaveChangesAsync(ct);
        return airport;
    }

    // -----------------------------
    // READ (single)
    // -----------------------------
    public async Task<AirportInfo?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Set<AirportInfo>().AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<AirportInfo?> GetByIdentAsync(string ident, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(ident)) return null;
        var v = ident.Trim().ToUpperInvariant();
        return await _db.Set<AirportInfo>().AsNoTracking().FirstOrDefaultAsync(a => a.Ident == v, ct);
    }

    public async Task<AirportInfo?> GetByIataAsync(string iata, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(iata)) return null;
        var v = iata.Trim().ToUpperInvariant();
        return await _db.Set<AirportInfo>().AsNoTracking().FirstOrDefaultAsync(a => a.IataCode == v, ct);
    }

    public async Task<AirportInfo?> GetByGpsAsync(string gpsCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(gpsCode)) return null;
        var v = gpsCode.Trim().ToUpperInvariant();
        return await _db.Set<AirportInfo>().AsNoTracking().FirstOrDefaultAsync(a => a.GpsCode == v, ct);
    }

    // -----------------------------
    // READ (collections)
    // -----------------------------
    public async Task<IReadOnlyList<AirportInfo>> GetAllAsync(CancellationToken ct = default)
        => await _db.Set<AirportInfo>().AsNoTracking()
            .OrderBy(a => a.Name)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AirportInfo>> SearchAsync(
        string? query = null,
        AirportType? type = null,
        AirportContinent? continent = null,
        Iso3166_Alpha2? country = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default)
    {
        var q = _db.Set<AirportInfo>().AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            var termUpper = term.ToUpperInvariant();

            q = q.Where(a =>
                a.Name.Contains(term) ||
                a.Municipality.Contains(term) ||
                (a.Ident != null && a.Ident.ToUpper().StartsWith(termUpper)) ||
                (a.IataCode != null && a.IataCode.ToUpper().StartsWith(termUpper)));
        }

        if (type.HasValue)      q = q.Where(a => a.Type == type.Value);
        if (continent.HasValue) q = q.Where(a => a.Continent == continent.Value);
        if (country.HasValue)   q = q.Where(a => a.IsoCountry == country.Value);

        return await q
            .OrderBy(a => a.Name)
            .Skip(Math.Max(0, skip))
            .Take(Math.Clamp(take, 1, 500))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AirportInfo>> GetByCountry(Iso3166_Alpha2 isoCountry, CancellationToken ct = default)
        => await _db.Set<AirportInfo>().AsNoTracking()
            .Where(a => a.IsoCountry == isoCountry)
            .OrderBy(a => a.Name)
            .ToListAsync(ct);

    // -----------------------------
    // UPDATE
    // -----------------------------
    public async Task<AirportInfo> UpdateAsync(AirportInfo airport, CancellationToken ct = default)
    {
        if (airport is null) throw new ArgumentNullException(nameof(airport));
        if (airport.Id <= 0) throw new ArgumentException("Id must be provided for update.", nameof(airport));

        Normalize(airport);
        if (string.IsNullOrWhiteSpace(airport.Ident))
            throw new ArgumentException("Ident is required.", nameof(airport));
        if (string.IsNullOrWhiteSpace(airport.Name))
            throw new ArgumentException("Name is required.", nameof(airport));

        _db.Attach(airport);
        _db.Entry(airport).State = EntityState.Modified;
        await _db.SaveChangesAsync(ct);
        return airport;
    }

    // -----------------------------
    // DELETE
    // -----------------------------
    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = await _db.Set<AirportInfo>().FindAsync([id], ct);
        if (existing is null) return false;

        _db.Set<AirportInfo>().Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // -----------------------------
    // UTIL
    // -----------------------------
    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => await _db.Set<AirportInfo>().AsNoTracking().AnyAsync(a => a.Id == id, ct);

    public async Task<int> BulkUpsertAsync(IEnumerable<AirportInfo> batch, CancellationToken ct = default)
    {
        if (batch is null) return 0;

        // Normalize inputs and collect idents
        var list = batch.ToList();
        foreach (var a in list) Normalize(a);
        var identSet = list
            .Where(a => !string.IsNullOrWhiteSpace(a.Ident))
            .Select(a => a.Ident)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (identSet.Count == 0) return 0;

        // Load existing by ident (case-insensitive)
        var existing = await _db.Set<AirportInfo>()
            .Where(a => identSet.Contains(a.Ident))
            .ToListAsync(ct);

        var map = existing.ToDictionary(a => a.Ident, StringComparer.OrdinalIgnoreCase);

        int adds = 0, updates = 0;

        foreach (var incoming in list)
        {
            if (string.IsNullOrWhiteSpace(incoming.Ident)) continue;

            if (map.TryGetValue(incoming.Ident, out var found))
            {
                // Update tracked entity fields
                found.Type            = incoming.Type;
                found.Name            = incoming.Name;
                found.LatitudeDeg     = incoming.LatitudeDeg;
                found.LongitudeDeg    = incoming.LongitudeDeg;
                found.ElevationFt     = incoming.ElevationFt;
                found.Continent       = incoming.Continent;
                found.IsoCountry      = incoming.IsoCountry;
                found.IsoRegion       = incoming.IsoRegion;
                found.Municipality    = incoming.Municipality;
                found.ScheduledService= incoming.ScheduledService;
                found.GpsCode         = incoming.GpsCode;
                found.IataCode        = incoming.IataCode;
                found.LocalCode       = incoming.LocalCode;
                updates++;
            }
            else
            {
                await _db.Set<AirportInfo>().AddAsync(incoming, ct);
                adds++;
            }
        }

        if (adds == 0 && updates == 0) return 0;
        await _db.SaveChangesAsync(ct);
        return adds + updates;
    }

    // -----------------------------
    // Helpers
    // -----------------------------
    private static void Normalize(AirportInfo a)
    {
        a.Ident        = (a.Ident ?? string.Empty).Trim().ToUpperInvariant();
        a.Name         = (a.Name ?? string.Empty).Trim();
        a.IsoRegion    = (a.IsoRegion ?? string.Empty).Trim().ToUpperInvariant();
        a.Municipality = (a.Municipality ?? string.Empty).Trim();

        a.GpsCode   = a.GpsCode?.Trim().ToUpperInvariant();
        a.IataCode  = a.IataCode?.Trim().ToUpperInvariant();
        a.LocalCode = a.LocalCode?.Trim().ToUpperInvariant();

        // Enums are value types — defaults already set in the model; nothing else to normalize.
    }
}
