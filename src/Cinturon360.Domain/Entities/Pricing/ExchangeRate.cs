using Cinturon360.Domain.Common.Base;

namespace Cinturon360.Domain.Entities.Pricing;

/// <summary>
/// Stores the latest ECB spot exchange rate for a currency against EUR.
/// Rate is: 1 EUR = {Rate} {CurrencyCode}
/// Base currency is always EUR. Cross-currency conversion goes via EUR.
/// </summary>
public sealed class ExchangeRate : Entity
{
    /// <summary>ISO 4217 currency code, e.g. "AUD", "USD", "GBP".</summary>
    public string CurrencyCode { get; private set; } = string.Empty;

    /// <summary>Human-readable currency name, e.g. "Australian dollar".</summary>
    public string CurrencyName { get; private set; } = string.Empty;

    /// <summary>
    /// Number of currency units per 1 EUR.
    /// e.g. if Rate = 1.6344, then 1 EUR = 1.6344 AUD.
    /// </summary>
    public decimal Rate { get; private set; }

    /// <summary>The ECB publication date for this rate.</summary>
    public DateOnly RateDate { get; private set; }

    /// <summary>When this row was last fetched from the ECB API.</summary>
    public DateTimeOffset FetchedAt { get; private set; }

    private ExchangeRate() { }

    public static ExchangeRate Create(
        string id,
        string currencyCode,
        string currencyName,
        decimal rate,
        DateOnly rateDate)
        => new()
        {
            Id = id,
            CurrencyCode = currencyCode.ToUpperInvariant(),
            CurrencyName = currencyName,
            Rate = rate,
            RateDate = rateDate,
            FetchedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void UpdateRate(decimal rate, DateOnly rateDate)
    {
        Rate = rate;
        RateDate = rateDate;
        FetchedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
