using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

// -----------------------------
// Configuration
// -----------------------------
const string DuffelBaseUrl = "https://api.duffel.com";
const string DuffelVersion = "v2";
const string CsvFileName = "duffel_airports.csv";

// -----------------------------
// Resolve bearer token
// -----------------------------
string? bearerToken = null;

foreach (var arg in args)
{
    if (arg.StartsWith("--bearer-token=", StringComparison.OrdinalIgnoreCase))
    {
        bearerToken = arg.Split('=', 2)[1];
        break;
    }
}

bearerToken ??= Environment.GetEnvironmentVariable("DUFFEL_API_TOKEN");

if (string.IsNullOrWhiteSpace(bearerToken))
{
    Console.Error.WriteLine("Missing Duffel bearer token. Use --bearer-token or DUFFEL_API_TOKEN.");
    return;
}

// -----------------------------
// HTTP client (FIXED: gzip support)
// -----------------------------
using var handler = new HttpClientHandler
{
    AutomaticDecompression =
        DecompressionMethods.GZip |
        DecompressionMethods.Deflate
};

using var http = new HttpClient(handler)
{
    BaseAddress = new Uri(DuffelBaseUrl)
};

http.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", bearerToken);

http.DefaultRequestHeaders.Add("Duffel-Version", DuffelVersion);
http.DefaultRequestHeaders.Accept.Add(
    new MediaTypeWithQualityHeaderValue("application/json"));
http.DefaultRequestHeaders.AcceptEncoding.Add(
    new StringWithQualityHeaderValue("gzip"));

// -----------------------------
// CSV setup
// -----------------------------
var csvPath = Path.Combine(AppContext.BaseDirectory, CsvFileName);
bool writeHeader = !File.Exists(csvPath);

using var csvStream = new FileStream(csvPath, FileMode.Append, FileAccess.Write, FileShare.Read);
using var csvWriter = new StreamWriter(csvStream, Encoding.UTF8);

if (writeHeader)
{
    csvWriter.WriteLine(string.Join(',',
        "id",
        "name",
        "iata_code",
        "icao_code",
        "iata_city_code",
        "iata_country_code",
        "city_name",
        "latitude",
        "longitude",
        "time_zone"
    ));
    csvWriter.Flush();
}

// -----------------------------
// Pagination loop
// -----------------------------
string? afterCursor = null;
int page = 1;
int total = 0;

do
{
    var url = "/air/airports";
    if (!string.IsNullOrEmpty(afterCursor))
        url += $"?after={Uri.EscapeDataString(afterCursor)}";

    Console.WriteLine($"Fetching page {page}…");

    using var response = await http.GetAsync(url);
    response.EnsureSuccessStatusCode();

    var json = await response.Content.ReadAsStringAsync();
    var payload = JsonSerializer.Deserialize<AirportResponse>(json);

    if (payload?.Data == null)
        break;

    foreach (var a in payload.Data)
    {
        csvWriter.WriteLine(string.Join(',',
            Csv(a.Id),
            Csv(a.Name),
            Csv(a.IataCode),
            Csv(a.IcaoCode),
            Csv(a.IataCityCode),
            Csv(a.IataCountryCode),
            Csv(a.CityName),
            Csv(a.Latitude),
            Csv(a.Longitude),
            Csv(a.TimeZone)
        ));

        total++;
    }

    csvWriter.Flush();

    afterCursor = payload.Meta?.After;
    page++;

} while (!string.IsNullOrEmpty(afterCursor));

Console.WriteLine($"Done. {total} airports written to {CsvFileName}.");

// -----------------------------
// Helpers
// -----------------------------
static string Csv(object? value)
{
    if (value == null) return "";
    var s = value.ToString() ?? "";
    if (s.Contains('"') || s.Contains(',') || s.Contains('\n'))
        return $"\"{s.Replace("\"", "\"\"")}\"";
    return s;
}

// -----------------------------
// Models
// -----------------------------
sealed class AirportResponse
{
    [JsonPropertyName("meta")]
    public Meta? Meta { get; set; }

    [JsonPropertyName("data")]
    public List<Airport>? Data { get; set; }
}

sealed class Meta
{
    [JsonPropertyName("before")]
    public string? Before { get; set; }

    [JsonPropertyName("after")]
    public string? After { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }
}

sealed class Airport
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("iata_code")]
    public string? IataCode { get; set; }

    [JsonPropertyName("icao_code")]
    public string? IcaoCode { get; set; }

    [JsonPropertyName("iata_city_code")]
    public string? IataCityCode { get; set; }

    [JsonPropertyName("iata_country_code")]
    public string? IataCountryCode { get; set; }

    [JsonPropertyName("city_name")]
    public string? CityName { get; set; }

    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }

    [JsonPropertyName("time_zone")]
    public string? TimeZone { get; set; }
}
