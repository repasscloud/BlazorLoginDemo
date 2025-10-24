using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using BlazorLoginDemo.Shared.Models.Kernel.FX;
using BlazorLoginDemo.Shared.Services.Interfaces.External;
using BlazorLoginDemo.Shared.Services.Interfaces.Persistence;

namespace BlazorLoginDemo.Shared.Services.External;

public sealed class FxRateService : IFxRateService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ExchangeRateApiOptions _opts;
    private readonly IMemoryCache _cache;
    private readonly IFxRateStore _store;

    private static string CacheKey(string baseCode) => $"fx:rates:{baseCode.ToUpperInvariant()}";

    public FxRateService(
        IHttpClientFactory httpFactory,
        IOptions<ExchangeRateApiOptions> opts,
        IMemoryCache cache,
        IFxRateStore store)
    {
        _httpFactory = httpFactory;
        _opts = opts.Value;
        _cache = cache;
        _store = store;
    }

    public async Task<decimal?> GetLatestSnapshotRateAsync(string baseCode, string quoteCode, CancellationToken ct = default)
    {
        baseCode  = (baseCode ?? string.Empty).Trim().ToUpperInvariant();
        quoteCode = (quoteCode ?? string.Empty).Trim().ToUpperInvariant();

        var snap = await _store.GetLatestAsync(baseCode, ct);
        if (snap is null) return null;

        return snap.Rates.TryGetValue(quoteCode, out var r)
            ? decimal.Round(r, 6, MidpointRounding.AwayFromZero)
            : null;
    }

    public async Task<ExchangeRateResponse> GetRatesAsync(string baseCode, CancellationToken ct = default)
    {
        baseCode = Normalize(baseCode) ?? _opts.DefaultBaseCode;

        if (_cache.TryGetValue(CacheKey(baseCode), out ExchangeRateResponse? cached) && cached != null)
            return cached;

        // Build absolute URL using settings, same pattern as AmadeusAuthService.
        // e.g. https://v6.exchangerate-api.com/v6/{key}/latest/{base}
        var url = $"{_opts.BaseUrl.TrimEnd('/')}/{_opts.ApiKey.Trim()}/latest/{baseCode}";

        var http = _httpFactory.CreateClient("fx"); // named client optional, see Program.cs snippet
        using var resp = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);

        if (resp.StatusCode == HttpStatusCode.NotFound)
            throw new InvalidOperationException($"Base currency '{baseCode}' not supported by provider.");

        resp.EnsureSuccessStatusCode();

        var payload = await resp.Content.ReadFromJsonAsync<ExchangeRateResponse>(cancellationToken: ct)
                        ?? throw new InvalidOperationException("Empty FX response.");

        if (!string.Equals(payload.Result, "success", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"FX provider returned non-success result: '{payload.Result}'.");

        var ttl = payload.TimeNextUpdateUtc - DateTimeOffset.UtcNow - TimeSpan.FromSeconds(10);
        if (ttl < TimeSpan.FromSeconds(1)) ttl = TimeSpan.FromMinutes(5);

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
