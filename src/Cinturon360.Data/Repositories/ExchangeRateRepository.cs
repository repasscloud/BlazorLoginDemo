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
            .FirstOrDefaultAsync(r => r.CurrencyCode == currencyCode.ToUpperInvariant(), ct);

    public async Task UpsertAsync(IReadOnlyList<ExchangeRate> rates, CancellationToken ct = default)
    {
        foreach (var rate in rates)
        {
            var entry = db.ChangeTracker.Entries<ExchangeRate>()
                .FirstOrDefault(e => e.Entity.Id == rate.Id);

            if (entry is not null)
            {
                // Already tracked — EF will detect the property changes from UpdateRate()
                continue;
            }

            // Check if it exists in the DB but isn't tracked yet
            var existing = await db.ExchangeRates
                .FirstOrDefaultAsync(r => r.CurrencyCode == rate.CurrencyCode, ct);

            if (existing is null)
            {
                await db.ExchangeRates.AddAsync(rate, ct);
            }
            // If existing != null, it was already loaded and updated via UpdateRate() by the handler
        }
    }
}
