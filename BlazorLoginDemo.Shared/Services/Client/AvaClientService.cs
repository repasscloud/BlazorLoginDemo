using BlazorLoginDemo.Shared.Services.Interfaces.Client;
using Microsoft.EntityFrameworkCore;

namespace BlazorLoginDemo.Shared.Services.Client;

public sealed class AvaClientService : IAvaClientService
{
    private readonly ApplicationDbContext _db;

    public AvaClientService(ApplicationDbContext db) => _db = db;

    // CREATE
    public async Task<AvaClient> CreateAsync(AvaClient client, CancellationToken ct = default)
    {
        await _db.AvaClients.AddAsync(client, ct);
        await _db.SaveChangesAsync(ct);
        return client;
    }

    // READ
    public async Task<AvaClient?> GetByIdAsync(string id, CancellationToken ct = default)
        => await _db.AvaClients.FindAsync([id], ct);

    public async Task<IReadOnlyList<AvaClient>> GetAllAsync(CancellationToken ct = default)
        => await _db.AvaClients.AsNoTracking().OrderBy(c => c.CompanyName).ToListAsync(ct);

    public async Task<IReadOnlyList<AvaClient>> SearchClientAsync(string query, int take = 50, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query)) return Array.Empty<AvaClient>();

        var q = query.Trim();
        var pattern = $"%{q}%";

        // For phone matching like "492257868" vs "0492257868"
        var qDigits = new string(q.Where(char.IsDigit).ToArray());
        var digitsPattern = $"%{qDigits}%";

        var filtered = _db.AvaClients
            .AsNoTracking()
            .Where(c =>
                // Company name contains (case-insensitive)
                EF.Functions.ILike(c.CompanyName ?? string.Empty, pattern)
                // Tax ID contains
                || EF.Functions.ILike(c.TaxId ?? string.Empty, pattern)
                // Email contains
                || EF.Functions.ILike(c.ContactPersonEmail ?? string.Empty, pattern)
                // Phone contains (digits-only compare; ignore spaces, +, (), -)
                || (qDigits != string.Empty && EF.Functions.ILike(
                    ((c.ContactPersonPhone ?? string.Empty)
                        .Replace(" ", string.Empty)
                        .Replace("-", string.Empty)
                        .Replace("(", string.Empty)
                        .Replace(")", string.Empty)
                        .Replace("+", string.Empty)),
                    digitsPattern))
            );

        return await filtered
            .OrderBy(c => c.CompanyName)
            .Take(Math.Max(1, take))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AvaClient>> SearchClientByPageAsync(
        string query, 
        int page = 0, 
        int take = 50, 
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query)) 
            return Array.Empty<AvaClient>();

        var q = query.Trim();
        var pattern = $"%{q}%";

        // For phone matching like "492257868" vs "0492257868"
        var qDigits = new string(q.Where(char.IsDigit).ToArray());
        var digitsPattern = $"%{qDigits}%";

        var filtered = _db.AvaClients
            .AsNoTracking()
            .Where(c =>
                // Company name contains (case-insensitive)
                EF.Functions.ILike(c.CompanyName ?? string.Empty, pattern)
                // Tax ID contains
                || EF.Functions.ILike(c.TaxId ?? string.Empty, pattern)
                // Email contains
                || EF.Functions.ILike(c.ContactPersonEmail ?? string.Empty, pattern)
                // Phone contains (digits-only compare; ignore spaces, +, (), -)
                || (qDigits != string.Empty && EF.Functions.ILike(
                    ((c.ContactPersonPhone ?? string.Empty)
                        .Replace(" ", string.Empty)
                        .Replace("-", string.Empty)
                        .Replace("(", string.Empty)
                        .Replace(")", string.Empty)
                        .Replace("+", string.Empty)),
                    digitsPattern))
            );

        return await filtered
            .OrderBy(c => c.CompanyName)
            .Skip(page * take)        // 👈 offset
            .Take(Math.Max(1, take))  // 👈 limit
            .ToListAsync(ct);
    }


    // UPDATE (replace whole object)
    public async Task<AvaClient> UpdateAsync(AvaClient client, CancellationToken ct = default)
    {
        // If the context isn't already tracking, attach + mark modified to replace the entity
        _db.Attach(client);
        _db.Entry(client).State = EntityState.Modified;

        await _db.SaveChangesAsync(ct);
        return client;
    }

    // DELETE
    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var existing = await _db.AvaClients.FindAsync([id], ct);
        if (existing is null) return false;

        _db.AvaClients.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // UTIL
    public async Task<bool> ExistsAsync(string id, CancellationToken ct = default)
        => await _db.AvaClients.AsNoTracking().AnyAsync(c => c.Id == id, ct);

    public async Task<string?> GetClientNameOnlyAsync(string id, CancellationToken ct = default)
        => await _db.AvaClients
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => c.CompanyName)
            .FirstOrDefaultAsync(ct);

    public async Task<string?> GetClientDefaultCurrencyAsync(string id, CancellationToken ct = default)
        => await _db.AvaClients
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => c.DefaultCurrency)
            .FirstOrDefaultAsync(ct);

}