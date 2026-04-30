using System.Text.Json;
using Microsoft.Extensions.Logging;
using Cinturon360.Contracts.Feeds;
using Cinturon360.Integrations.ExchangeRates.Models;

namespace Cinturon360.Integrations.ExchangeRates;

/// <summary>
/// Fetches the latest ECB SDMX JSON exchange rates.
/// All rates are EUR-based: 1 EUR = {Rate} {CurrencyCode}.
/// Uses a named HttpClient "EcbRates".
/// </summary>
public sealed class EcbExchangeRateProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<EcbExchangeRateProvider> logger)
    : IExchangeRateProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<FxRateDto>> GetLatestRatesAsync(CancellationToken ct = default)
    {
        using var client = httpClientFactory.CreateClient("EcbRates");

        HttpResponseMessage response;
        try
        {
            response = await client.GetAsync(
                "service/data/EXR/D..EUR.SP00.A?lastNObservations=1&format=jsondata",
                ct);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "EVT=INT_ERR CAT=INT ACT=EXEC OUT=ERR PROV=ECB NOTE=fetch_failed");
            return [];
        }

        EcbSdmxResponse? root;
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            root = await JsonSerializer.DeserializeAsync<EcbSdmxResponse>(stream, JsonOptions, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "EVT=INT_ERR CAT=INT ACT=EXEC OUT=ERR PROV=ECB NOTE=parse_failed");
            return [];
        }

        if (root is null || root.DataSets.Count == 0)
        {
            logger.LogWarning("EVT=INT_CALL_END CAT=INT ACT=EXEC OUT=WARN PROV=ECB NOTE=empty_response");
            return [];
        }

        return Parse(root);
    }

    private static IReadOnlyList<FxRateDto> Parse(EcbSdmxResponse root)
    {
        var dataSet = root.DataSets[0];
        var structure = root.Structure;

        // Series dimension index 1 = CURRENCY
        var currencyDimension = structure.Dimensions.Series.Count > 1
            ? structure.Dimensions.Series[1]
            : null;
        if (currencyDimension is null) return [];

        // Observation dimension index 0 = TIME_PERIOD
        var timeDimension = structure.Dimensions.Observation.Count > 0
            ? structure.Dimensions.Observation[0]
            : null;

        var results = new List<FxRateDto>(dataSet.Series.Count);

        foreach (var (seriesKey, series) in dataSet.Series)
        {
            // Key format: "0:currencyIdx:0:0:0"
            var parts = seriesKey.Split(':');
            if (parts.Length < 2 || !int.TryParse(parts[1], out var currencyIdx))
                continue;

            if (currencyIdx >= currencyDimension.Values.Count)
                continue;

            var currencyValue = currencyDimension.Values[currencyIdx];

            // Take the first (and only, due to lastNObservations=1) observation
            foreach (var (obsKey, obsValues) in series.Observations)
            {
                if (obsValues.Count == 0 || obsValues[0] is null)
                    continue;

                var rate = obsValues[0]!.Value;

                // Resolve the date from TIME_PERIOD dimension
                var rateDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date);
                if (timeDimension is not null &&
                    int.TryParse(obsKey, out var dateIdx) &&
                    dateIdx < timeDimension.Values.Count &&
                    DateOnly.TryParse(timeDimension.Values[dateIdx].Id, out var parsedDate))
                {
                    rateDate = parsedDate;
                }

                results.Add(new FxRateDto(
                    CurrencyCode: currencyValue.Id,
                    CurrencyName: currencyValue.Name,
                    Rate: rate,
                    RateDate: rateDate
                ));
                break; // only one observation per series
            }
        }

        return results;
    }
}
