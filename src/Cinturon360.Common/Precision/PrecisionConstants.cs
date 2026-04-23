namespace Cinturon360.Common.Precision;

/// <summary>
/// Centrally controlled decimal precision conventions for the platform.
/// All EF configurations and monetary calculations must reference these constants.
/// </summary>
public static class PrecisionConstants
{
    // Money / currency amounts
    public const int MoneyPrecision = 18;
    public const int MoneyScale = 4;

    // Exchange rates
    public const int ExchangeRatePrecision = 18;
    public const int ExchangeRateScale = 8;

    // Tax and fee percentages
    public const int TaxPrecision = 10;
    public const int TaxScale = 6;

    // General percentage (e.g. markup %)
    public const int PercentagePrecision = 10;
    public const int PercentageScale = 4;

    // Quantities (e.g. nights, segments)
    public const int QuantityPrecision = 10;
    public const int QuantityScale = 2;
}
