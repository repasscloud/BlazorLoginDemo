using System.Text.Json.Serialization;

namespace Cinturon360.Integrations.ExchangeRates.Models;

/// <summary>
/// Root response from the ECB SDMX JSON API.
/// URL: https://data-api.ecb.europa.eu/service/data/EXR/D..EUR.SP00.A?lastNObservations=1&amp;format=jsondata
/// </summary>
public sealed class EcbSdmxResponse
{
    [JsonPropertyName("dataSets")]
    public List<EcbDataSet> DataSets { get; set; } = [];

    [JsonPropertyName("structure")]
    public EcbStructure Structure { get; set; } = new();
}

public sealed class EcbDataSet
{
    /// <summary>
    /// Compressed series map. Key format: "{FREQ_idx}:{CURRENCY_idx}:{DENOM_idx}:{TYPE_idx}:{SUFFIX_idx}"
    /// </summary>
    [JsonPropertyName("series")]
    public Dictionary<string, EcbSeries> Series { get; set; } = new();
}

public sealed class EcbSeries
{
    /// <summary>
    /// Observation map. Key = index into structure.dimensions.observation[0].values.
    /// Value = [ rate ] (array with one element).
    /// </summary>
    [JsonPropertyName("observations")]
    public Dictionary<string, List<decimal?>> Observations { get; set; } = new();
}

public sealed class EcbStructure
{
    [JsonPropertyName("dimensions")]
    public EcbDimensions Dimensions { get; set; } = new();
}

public sealed class EcbDimensions
{
    /// <summary>5-element list: FREQ, CURRENCY, CURRENCY_DENOM, EXR_TYPE, EXR_SUFFIX.</summary>
    [JsonPropertyName("series")]
    public List<EcbDimension> Series { get; set; } = [];

    /// <summary>1-element list: TIME_PERIOD.</summary>
    [JsonPropertyName("observation")]
    public List<EcbDimension> Observation { get; set; } = [];
}

public sealed class EcbDimension
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("values")]
    public List<EcbDimensionValue> Values { get; set; } = [];
}

public sealed class EcbDimensionValue
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
