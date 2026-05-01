namespace Cinturon360.Contracts.Feeds;

public sealed record ExchangeRateLatestResponse(
    string CurrencyCode,
    string CurrencyName,
    decimal Rate,
    DateOnly RateDate,
    DateTimeOffset FetchedAt
);

public sealed record ConvertCurrencyResponse(
    decimal Amount,
    string FromCurrencyCode,
    string ToCurrencyCode,
    decimal ConvertedAmount
);
