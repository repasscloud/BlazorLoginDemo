using Microsoft.EntityFrameworkCore;
using Cinturon360.Shared.Data;
using Cinturon360.Shared.Models.ExternalLib.Amadeus;
using Cinturon360.Shared.Services.Interfaces.External;

namespace Cinturon360.Shared.Services.External;

public sealed class AmadeusAccountService : IAmadeusAccountService
{
    private readonly ApplicationDbContext _db;

    public AmadeusAccountService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<AmadeusAccount> CreateAsync(
        AmadeusAccount account,
        CancellationToken ct = default)
    {
        if (await ExistsAsync(account.TmcId, ct))
            throw new InvalidOperationException(
                $"Amadeus account already exists for TMC '{account.TmcId}'.");

        _db.AmadeusAccounts.Add(account);
        await _db.SaveChangesAsync(ct);

        return account;
    }

    public async Task<AmadeusAccount> UpdateAsync(
        AmadeusAccount account,
        CancellationToken ct = default)
    {
        var existing = await _db.AmadeusAccounts
            .FirstOrDefaultAsync(a => a.TmcId == account.TmcId, ct);

        if (existing is null)
            throw new InvalidOperationException(
                $"Amadeus account not found for TMC '{account.TmcId}'.");

        _db.Entry(existing).CurrentValues.SetValues(account);
        await _db.SaveChangesAsync(ct);

        return account;
    }

    public async Task<AmadeusAccount?> GetAsync(
        string tmcId,
        CancellationToken ct = default)
    {
        return await _db.AmadeusAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.TmcId == tmcId, ct);
    }

    public async Task<bool> ExistsAsync(
        string tmcId,
        CancellationToken ct = default)
    {
        return await _db.AmadeusAccounts
            .AnyAsync(a => a.TmcId == tmcId, ct);
    }
}
