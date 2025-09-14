using BlazorLoginDemo.Shared.Models.Policies;
using BlazorLoginDemo.Shared.Services.Interfaces.Policies;
using Microsoft.EntityFrameworkCore;

namespace BlazorLoginDemo.Shared.Services.Policies;

public sealed class TravelPolicyDisabledCountryService : ITravelPolicyDisabledCountryService
{
    private readonly ApplicationDbContext _db;
    public TravelPolicyDisabledCountryService(ApplicationDbContext db) => _db = db;

    public async Task<TravelPolicyDisabledCountry?> FindAsync(string policyId, int countryId, CancellationToken ct = default)
    {
        policyId = NormalizePolicyId(policyId);
        return await _db.Set<TravelPolicyDisabledCountry>()
            .FindAsync([policyId, countryId], ct);
    }

    /// <summary>
    /// Idempotent add: returns existing row if already present.
    /// </summary>
    public async Task<TravelPolicyDisabledCountry> AddAsync(string policyId, int countryId, CancellationToken ct = default)
    {
        policyId = NormalizePolicyId(policyId);

        var existing = await _db.Set<TravelPolicyDisabledCountry>()
            .FindAsync([policyId, countryId], ct);
        if (existing is not null) return existing;

        var row = new TravelPolicyDisabledCountry { TravelPolicyId = policyId, CountryId = countryId };
        await _db.Set<TravelPolicyDisabledCountry>().AddAsync(row, ct);
        await _db.SaveChangesAsync(ct);
        return row;
    }

    public async Task<bool> RemoveAsync(string policyId, int countryId, CancellationToken ct = default)
    {
        policyId = NormalizePolicyId(policyId);

        var existing = await _db.Set<TravelPolicyDisabledCountry>()
            .FindAsync([policyId, countryId], ct);
        if (existing is null) return false;

        _db.Set<TravelPolicyDisabledCountry>().Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> IsDisabledAsync(string policyId, int countryId, CancellationToken ct = default)
    {
        policyId = NormalizePolicyId(policyId);

        return await _db.Set<TravelPolicyDisabledCountry>()
            .AsNoTracking()
            .AnyAsync(x => x.TravelPolicyId == policyId && x.CountryId == countryId, ct);
    }

    public async Task<IReadOnlyList<int>> GetDisabledCountryIdsAsync(string policyId, CancellationToken ct = default)
    {
        policyId = NormalizePolicyId(policyId);

        return await _db.Set<TravelPolicyDisabledCountry>()
            .AsNoTracking()
            .Where(x => x.TravelPolicyId == policyId)
            .Select(x => x.CountryId)
            .OrderBy(id => id)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Country>> GetDisabledCountriesAsync(string policyId, CancellationToken ct = default)
    {
        policyId = NormalizePolicyId(policyId);

        return await _db.Set<TravelPolicyDisabledCountry>()
            .AsNoTracking()
            .Where(x => x.TravelPolicyId == policyId)
            .Include(x => x.Country)
            .Select(x => x.Country!)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Replace the disabled-country set for a policy with exactly the provided IDs.
    /// Returns the total number of rows added + removed.
    /// </summary>
    public async Task<int> SetDisabledCountriesAsync(string policyId, IEnumerable<int> countryIds, CancellationToken ct = default)
    {
        policyId = NormalizePolicyId(policyId);
        var desired = countryIds?.Distinct().ToHashSet() ?? new HashSet<int>();

        var existing = await _db.Set<TravelPolicyDisabledCountry>()
            .Where(x => x.TravelPolicyId == policyId)
            .ToListAsync(ct);

        var existingIds = existing.Select(x => x.CountryId).ToHashSet();

        // Compute diffs
        var toAdd = desired.Except(existingIds).Select(id =>
            new TravelPolicyDisabledCountry { TravelPolicyId = policyId, CountryId = id }).ToList();

        var toRemove = existing.Where(x => !desired.Contains(x.CountryId)).ToList();

        if (toAdd.Count > 0)
            await _db.Set<TravelPolicyDisabledCountry>().AddRangeAsync(toAdd, ct);
        if (toRemove.Count > 0)
            _db.Set<TravelPolicyDisabledCountry>().RemoveRange(toRemove);

        if (toAdd.Count == 0 && toRemove.Count == 0) return 0;

        await _db.SaveChangesAsync(ct);
        return toAdd.Count + toRemove.Count;
    }

    public async Task<int> ClearAsync(string policyId, CancellationToken ct = default)
    {
        policyId = NormalizePolicyId(policyId);

        var rows = await _db.Set<TravelPolicyDisabledCountry>()
            .Where(x => x.TravelPolicyId == policyId)
            .ToListAsync(ct);

        if (rows.Count == 0) return 0;

        _db.Set<TravelPolicyDisabledCountry>().RemoveRange(rows);
        await _db.SaveChangesAsync(ct);
        return rows.Count;
    }

    private static string NormalizePolicyId(string policyId)
    {
        if (string.IsNullOrWhiteSpace(policyId))
            throw new ArgumentException("Policy id is required.", nameof(policyId));
        return policyId.Trim();
    }
}
