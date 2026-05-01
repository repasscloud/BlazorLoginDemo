using Cinturon360.Domain.Entities.Pricing;

namespace Cinturon360.Application.Abstractions.Persistence;

/// <summary>
/// Repository for exchange rate data. Rates are EUR-based (1 EUR = N units of currency).
/// </summary>
public interface IExchangeRateRepository
{
    /// <summary>
    /// Returns all stored exchange rates (latest per currency).
    /// </summary>
    Task<IReadOnlyList<ExchangeRate>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns the latest rate for a specific currency code.
    /// Returns null if no rate is stored for the given code.
    /// </summary>
    Task<ExchangeRate?> GetByCurrencyCodeAsync(string currencyCode, CancellationToken ct = default);

    /// <summary>
    /// Returns the latest stored rate per currency code.
    /// </summary>
    Task<IReadOnlyList<ExchangeRate>> ListLatestAsync(CancellationToken ct = default);

    /// <summary>
    /// Upserts a batch of rates (insert if not exists, update rate/date if currency code matches).
    /// </summary>
    Task UpsertAsync(IReadOnlyList<ExchangeRate> rates, CancellationToken ct = default);

    /// <summary>
    /// Deletes historical snapshots older than the provided UTC cutoff.
    /// </summary>
    Task<int> DeleteOlderThanAsync(DateTimeOffset cutoffUtc, CancellationToken ct = default);
}
