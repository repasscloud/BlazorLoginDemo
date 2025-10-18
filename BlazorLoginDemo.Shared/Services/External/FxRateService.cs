using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using BlazorLoginDemo.Shared.Models.Kernel.FX;
using BlazorLoginDemo.Shared.Services.Interfaces.External;

namespace BlazorLoginDemo.Shared.Services.External;

public sealed class FxRateService : IFxRateService
{
    private readonly HttpClient _http;
    private readonly ExchangeRateApiOptions _opts;
    private readonly IMemoryCache _cache;

    private static string CacheKey(string baseCode) => $"fx:rates:{baseCode.ToUpperInvariant()}";

    public FxRateService(HttpClient http, IOptions<ExchangeRateApiOptions> opts, IMemoryCache cache)
    {
        _http = http;
        _opts = opts.Value;
        _cache = cache;
    }

    public async Task<ExchangeRateResponse> GetRatesAsync(string baseCode, CancellationToken ct = default)
    {
        baseCode = Normalize(baseCode) ?? _opts.DefaultBaseCode;

        if (_cache.TryGetValue(CacheKey(baseCode), out ExchangeRateResponse? cached) && cached != null)
            return cached;

        // Endpoint: {BaseUrl}/{ApiKey}/latest/{baseCode}
        var path = $"latest/{baseCode}";
        using var req = new HttpRequestMessage(HttpMethod.Get, path);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);

        if (resp.StatusCode == HttpStatusCode.NotFound)
            throw new InvalidOperationException($"Base currency '{baseCode}' not supported by provider.");

        resp.EnsureSuccessStatusCode();

        var payload = await resp.Content.ReadFromJsonAsync<ExchangeRateResponse>(cancellationToken: ct)
                        ?? throw new InvalidOperationException("Empty FX response.");

        if (!string.Equals(payload.Result, "success", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"FX provider returned non-success result: '{payload.Result}'.");

        // Cache until provider’s next update time minus a small skew
        var ttl = payload.TimeNextUpdateUtc - DateTimeOffset.UtcNow - TimeSpan.FromSeconds(10);
        if (ttl < TimeSpan.FromSeconds(1)) ttl = TimeSpan.FromMinutes(5); // fallback

        _cache.Set(CacheKey(baseCode), payload, ttl);
        return payload;
    }

    public async Task<decimal> GetRateAsync(string from, string to, CancellationToken ct = default)
    {
        if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase)) return 1m;

        var baseCode = Normalize(from)!;
        var target = Normalize(to)!;

        var rates = await GetRatesAsync(baseCode, ct);
        if (!rates.ConversionRates.TryGetValue(target, out var rate))
            throw new InvalidOperationException($"Target currency '{target}' not found.");

        // Ensure decimal precision suitable for money math
        return decimal.Round(rate, 10, MidpointRounding.AwayFromZero);
    }

    public async Task<ConvertResult> ConvertAsync(string from, string to, decimal amount, CancellationToken ct = default)
    {
        var baseCode = Normalize(from)!;
        var target = Normalize(to)!;

        var rates = await GetRatesAsync(baseCode, ct);
        if (!rates.ConversionRates.TryGetValue(target, out var rate))
            throw new InvalidOperationException($"Target currency '{target}' not found.");

        var converted = amount * rate;

        return new ConvertResult
        {
            From = baseCode,
            To = target,
            Amount = amount,
            Rate = decimal.Round(rate, 10, MidpointRounding.AwayFromZero),
            Converted = decimal.Round(converted, 6, MidpointRounding.AwayFromZero),
            BaseCode = rates.BaseCode,
            RatesLastUpdateUtc = rates.TimeLastUpdateUtc,
            RatesNextUpdateUtc = rates.TimeNextUpdateUtc
        };
    }

    private static string? Normalize(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        var s = code.Trim().ToUpperInvariant();
        if (s.Length != 3) throw new ArgumentException("Currency codes must be 3 letters.", nameof(code));
        return s;
    }
}
