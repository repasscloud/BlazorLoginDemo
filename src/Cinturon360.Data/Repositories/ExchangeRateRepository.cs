using Microsoft.EntityFrameworkCore;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Data.Context;
using Cinturon360.Domain.Entities.Pricing;

namespace Cinturon360.Data.Repositories;

public sealed class ExchangeRateRepository(AppDbContext db) : IExchangeRateRepository
{
    public async Task<IReadOnlyList<ExchangeRate>> GetAllAsync(CancellationToken ct = default)
        => await db.ExchangeRates.ToListAsync(ct);

    public Task<ExchangeRate?> GetByCurrencyCodeAsync(string currencyCode, CancellationToken ct = default)
        => db.ExchangeRates
            .Where(r => r.CurrencyCode == currencyCode.ToUpperInvariant())
            .OrderByDescending(r => r.FetchedAt)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<ExchangeRate>> ListLatestAsync(CancellationToken ct = default)
    {
        return await db.ExchangeRates
            .GroupBy(r => r.CurrencyCode)
            .Select(g => g.OrderByDescending(x => x.FetchedAt).First())
            .OrderBy(r => r.CurrencyCode)
            .ToListAsync(ct);
    }

    public async Task UpsertAsync(IReadOnlyList<ExchangeRate> rates, CancellationToken ct = default)
    {
        await db.ExchangeRates.AddRangeAsync(rates, ct);
    }

    public Task<int> DeleteOlderThanAsync(DateTimeOffset cutoffUtc, CancellationToken ct = default)
        => db.ExchangeRates
            .Where(r => r.FetchedAt < cutoffUtc)
            .ExecuteDeleteAsync(ct);
}
