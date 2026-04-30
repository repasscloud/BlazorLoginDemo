using Cinturon360.Contracts.Feeds;

namespace Cinturon360.Integrations.ExchangeRates;

/// <summary>
/// Abstraction over an external FX rate data source.
/// Returns EUR-based spot rates (1 EUR = N units of each currency).
/// </summary>
public interface IExchangeRateProvider
{
    Task<IReadOnlyList<FxRateDto>> GetLatestRatesAsync(CancellationToken ct = default);
}
