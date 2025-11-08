using System.Text.Json.Serialization;

namespace Cinturon360.Shared.Models.Kernel.FX;

public sealed class ExchangeRateResponse
{
    [JsonPropertyName("result")] public string Result { get; set; } = "";
    [JsonPropertyName("documentation")] public string? Documentation { get; set; }
    [JsonPropertyName("terms_of_use")] public string? TermsOfUse { get; set; }

    [JsonPropertyName("time_last_update_unix")] public long TimeLastUpdateUnix { get; set; }
    [JsonPropertyName("time_last_update_utc")] public string? TimeLastUpdateUtcRaw { get; set; }
    [JsonPropertyName("time_next_update_unix")] public long TimeNextUpdateUnix { get; set; }
    [JsonPropertyName("time_next_update_utc")] public string? TimeNextUpdateUtcRaw { get; set; }

    [JsonPropertyName("base_code")] public string BaseCode { get; set; } = "";

    // Use dictionary to avoid fragile per-code properties
    [JsonPropertyName("conversion_rates")]
    public Dictionary<string, decimal> ConversionRates { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonIgnore] public DateTimeOffset TimeLastUpdateUtc => DateTimeOffset.FromUnixTimeSeconds(TimeLastUpdateUnix);
    [JsonIgnore] public DateTimeOffset TimeNextUpdateUtc => DateTimeOffset.FromUnixTimeSeconds(TimeNextUpdateUnix);
}

public sealed class ConvertQuery
{
    public string From { get; init; } = "USD";
    public string To { get; init; } = "AUD";
    public decimal Amount { get; init; } = 1m;
}

public sealed class ConvertResult
{
    public string From { get; init; } = "";
    public string To { get; init; } = "";
    public decimal Amount { get; init; }
    public decimal Rate { get; init; }
    public decimal Converted { get; init; }
    public string BaseCode { get; init; } = "";
    public DateTimeOffset RatesLastUpdateUtc { get; init; }
    public DateTimeOffset RatesNextUpdateUtc { get; init; }
}
