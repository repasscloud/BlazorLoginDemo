namespace Cinturon360.Shared.Services.Persistence;

using Cinturon360.Shared.Data;
using Cinturon360.Shared.Models.Kernel.FX;
using Cinturon360.Shared.Services.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

public sealed class EfFxRateStore : IFxRateStore
{
    private readonly ApplicationDbContext _db;
    public EfFxRateStore(ApplicationDbContext db) => _db = db;

    public Task<ExchangeRateSnapshot?> GetLatestAsync(string baseCode, CancellationToken ct = default)
    {
        baseCode = (baseCode ?? "").Trim().ToUpperInvariant();
        return _db.ExchangeRateSnapshots
                  .Where(x => x.BaseCode == baseCode)
                  .OrderByDescending(x => x.CreatedAtUtc)
                  .FirstOrDefaultAsync(ct);
    }

    public async Task<Guid> SaveAsync(ExchangeRateSnapshot snapshot, CancellationToken ct = default)
    {
        _db.ExchangeRateSnapshots.Add(snapshot);
        await _db.SaveChangesAsync(ct);
        return snapshot.Id;
    }
}
