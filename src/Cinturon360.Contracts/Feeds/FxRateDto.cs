namespace Cinturon360.Contracts.Feeds;

/// <summary>
/// A parsed exchange rate from a provider. EUR is always the base currency.
/// Rate = number of CurrencyCode units per 1 EUR.
/// </summary>
public sealed record FxRateDto(
    string CurrencyCode,
    string CurrencyName,
    decimal Rate,
    DateOnly RateDate
);
