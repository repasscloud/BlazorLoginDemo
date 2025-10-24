using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using BlazorLoginDemo.Shared.Models.Kernel.Travel;
using BlazorLoginDemo.Shared.Models.Static;
using BlazorLoginDemo.Shared.Services.Interfaces.Kernel;
using Microsoft.AspNetCore.Mvc;

namespace BlazorLoginDemo.Api.Controllers.Admin;

[Route("api/v1/admin/kerneldata")]
[ApiController]
// [ServiceFilter(typeof(RequireApiKeyFilter))] // <- uncomment to add header -based key for security (call Vicky Pollard)
public sealed class KernelDataController : ControllerBase
{
    private readonly IAirportInfoService _airportInfoService;

    public KernelDataController(IAirportInfoService aisvc) => _airportInfoService = aisvc;

    // ---------- READ ----------
    [HttpGet("airport-info/{id:int}")]
    public async Task<ActionResult<AirportInfo>> GetById(int id, CancellationToken ct)
    {
        var a = await _airportInfoService.GetByIdAsync(id, ct);
        return a is null ? NotFound() : Ok(a);
    }

    [HttpGet("airport-info/ident/{ident}")]
    public async Task<ActionResult<AirportInfo>> GetByIdent(string ident, CancellationToken ct)
    {
        var a = await _airportInfoService.GetByIdentAsync(ident, ct);
        return a is null ? NotFound() : Ok(a);
    }

    [HttpGet("airport-info/iata/{iata}")]
    public async Task<ActionResult<AirportInfo>> GetByIata(string iata, CancellationToken ct)
    {
        var a = await _airportInfoService.GetByIataAsync(iata, ct);
        return a is null ? NotFound() : Ok(a);
    }

    [HttpGet("airport-info/gps/{gpsCode}")]
    public async Task<ActionResult<AirportInfo>> GetByGps(string gpsCode, CancellationToken ct)
    {
        var a = await _airportInfoService.GetByGpsAsync(gpsCode, ct);
        return a is null ? NotFound() : Ok(a);
    }

    [HttpGet("airport-info")]
    public async Task<ActionResult<IReadOnlyList<AirportInfo>>> GetAll(CancellationToken ct)
        => Ok(await _airportInfoService.GetAllAsync(ct));

    [HttpGet("airport-info/search")]
    public async Task<ActionResult<IReadOnlyList<AirportInfo>>> Search(
        [FromQuery] string? q,
        [FromQuery] AirportType? type,
        [FromQuery] AirportContinent? continent,
        [FromQuery] Iso3166_Alpha2? country,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 99999,
        CancellationToken ct = default)
        => Ok(await _airportInfoService.SearchAsync(q, type, continent, country, skip, take, ct));

    [HttpGet("airport-info/search-multi")]
    public async Task<ActionResult<IReadOnlyList<AirportInfo>>> SearchMulti(
        [FromQuery(Name = "q")] string? query,
        [FromQuery(Name = "type")] List<AirportType>? types,
        [FromQuery(Name = "continent")] List<AirportContinent>? continents,
        [FromQuery(Name = "country")] List<Iso3166_Alpha2>? countries,
        [FromQuery(Name = "hasIata")] bool? hasIata,                     // defaults to true
        [FromQuery(Name = "hasMunicipality")] bool? hasMunicipality,     // defaults to true
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        if (types is { Count: 0 }) types = null;
        if (continents is { Count: 0 }) continents = null;
        if (countries is { Count: 0 }) countries = null;

        if (take <= 0) take = 50;
        if (take <= 500) take = 500;
        if (skip < 0) skip = 0;

        var result = await _airportInfoService.SearchMultiAsync(
            query,
            types,
            continents,
            countries,
            hasIata ?? true,
            hasMunicipality ?? true,
            skip,
            take,
            ct);

        return Ok(result);
    }

    // ---------- CREATE ----------
    [HttpPost("airport-info")]
    public async Task<ActionResult<AirportInfo>> Create([FromBody] AirportInfo input, CancellationToken ct)
    {
        var created = await _airportInfoService.CreateAsync(input, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // ---------- UPDATE ----------
    [HttpPut("airport-info/{id:int}")]
    public async Task<ActionResult<AirportInfo>> Update(int id, [FromBody] AirportInfo input, CancellationToken ct)
    {
        if (id != input.Id) return BadRequest("Id in route and body must match.");
        if (!await _airportInfoService.ExistsAsync(id, ct)) return NotFound();

        var updated = await _airportInfoService.UpdateAsync(input, ct);
        return Ok(updated);
    }

    // ---------- DELETE ----------
    [HttpDelete("airport-info/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => (await _airportInfoService.DeleteAsync(id, ct)) ? NoContent() : NotFound();

    // ---------- BULK ----------
    [HttpPost("airport-info/bulk-upsert")]
    public async Task<ActionResult<object>> BulkUpsert([FromBody] IEnumerable<AirportInfo> batch, CancellationToken ct)
    {
        var changed = await _airportInfoService.BulkUpsertAsync(batch, ct);
        return Ok(new { changed });
    }



    /// <summary>
    /// Accepts a CSV upload with OurAirports-style headers and upserts in batches.
    /// </summary>
    /// <remarks>
    /// Expected headers:
    /// "id","ident","type","name","latitude_deg","longitude_deg","elevation_ft",
    /// "continent","iso_country","iso_region","municipality","scheduled_service",
    /// "gps_code","iata_code","local_code", ... (extra columns ignored)
    /// </remarks>
    public sealed class AirportCsvUpload
    {
        [Required] public IFormFile File { get; set; } = default!;
    }

    [HttpPost("airport-info/bulk-upsert-from-csv")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> BulkUpsertFromCsv(
        [FromForm] AirportCsvUpload form,
        [FromQuery] int batchSize = 1000,
        CancellationToken ct = default)
    {
        if (batchSize <= 0) batchSize = 1000;

        using var stream = form.File.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

        if (reader.EndOfStream) return BadRequest("CSV has no content.");
        var header = await reader.ReadLineAsync();
        if (string.IsNullOrWhiteSpace(header)) return BadRequest("CSV header missing.");

        var headers = ParseCsvLine(header);
        var idx = BuildIndex(headers);

        var batch = new List<AirportInfo>(batchSize);
        var changedTotal = 0;

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cols = ParseCsvLine(line);
            var entity = MapRow(cols, idx);
            if (entity is null) continue;

            batch.Add(entity);
            if (batch.Count >= batchSize)
            {
                changedTotal += await _airportInfoService.BulkUpsertAsync(batch, ct);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
            changedTotal += await _airportInfoService.BulkUpsertAsync(batch, ct);

        return Ok(new { changed = changedTotal });
    }

    private static string[] ParseCsvLine(string line)
    {
        var list = new List<string>(); var sb = new StringBuilder(); var inQ = false;
        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (inQ)
            {
                if (c == '"' && i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                else if (c == '"') { inQ = false; }
                else sb.Append(c);
            }
            else
            {
                if (c == ',') { list.Add(sb.ToString()); sb.Clear(); }
                else if (c == '"') inQ = true;
                else sb.Append(c);
            }
        }
        list.Add(sb.ToString());
        return list.ToArray();
    }

    private sealed record ColIndex(
        int ident, int type, int name, int latitude_deg, int longitude_deg, int elevation_ft,
        int continent, int iso_country, int iso_region, int municipality,
        int scheduled_service, int gps_code, int iata_code, int local_code);

    private static ColIndex BuildIndex(string[] h)
    {
        int Find(string k){ for(int i=0;i<h.Length;i++){ var s=h[i]?.Trim().Trim('"').ToLowerInvariant(); if (s==k) return i; } return -1; }
        return new(
            ident:Find("ident"), type:Find("type"), name:Find("name"),
            latitude_deg:Find("latitude_deg"), longitude_deg:Find("longitude_deg"), elevation_ft:Find("elevation_ft"),
            continent:Find("continent"), iso_country:Find("iso_country"), iso_region:Find("iso_region"),
            municipality:Find("municipality"), scheduled_service:Find("scheduled_service"),
            gps_code:Find("gps_code"), iata_code:Find("iata_code"), local_code:Find("local_code"));
    }

    private static AirportInfo? MapRow(string[] c, ColIndex i)
    {
        string? Get(int x)=> x>=0 && x<c.Length ? c[x] : null;
        string? Nz(string? s)=> string.IsNullOrWhiteSpace(s)? null : s.Trim();

        var ident = Nz(Get(i.ident)); var name = Nz(Get(i.name));
        if (ident is null || name is null) return null;

        return new AirportInfo
        {
            Ident = ident,
            Name  = name,
            Type        = ParseAirportType(Get(i.type)),
            Continent   = ParseContinent(Get(i.continent)),
            IsoCountry  = ParseIsoCountry(Get(i.iso_country)),
            LatitudeDeg = TryDouble(Get(i.latitude_deg)) ?? 0,
            LongitudeDeg= TryDouble(Get(i.longitude_deg)) ?? 0,
            ElevationFt = TryInt(Get(i.elevation_ft)) ?? 0,
            IsoRegion   = Get(i.iso_region)?.Trim() ?? string.Empty,
            Municipality= Get(i.municipality)?.Trim() ?? string.Empty,
            ScheduledService = ParseBool(Get(i.scheduled_service)) ?? false,
            GpsCode     = Nz(Get(i.gps_code)),
            IataCode    = Nz(Get(i.iata_code)),
            LocalCode   = Nz(Get(i.local_code))
        };
    }

    private static double? TryDouble(string? s) =>
        double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : null;

    private static int? TryInt(string? s) =>
        int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : null;

    private static bool? ParseBool(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        var t = s.Trim().ToLowerInvariant();
        return t is "1" or "true" or "yes" or "y" ? true :
            t is "0" or "false" or "no"  or "n" ? false : null;
    }

    private static AirportType ParseAirportType(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return AirportType.Unknown;
        var t = s.Trim().ToLowerInvariant().Replace("_","").Replace("-","");
        return t switch
        {
            "smallairport"  => AirportType.SmallAirport,
            "mediumairport" => AirportType.MediumAirport,
            "largeairport"  => AirportType.LargeAirport,
            "heliport"      => AirportType.Heliport,
            "seaplanebase"  => AirportType.SeaplanePort, // matches enum
            "balloonport"   => AirportType.BalloonPort,
            "closed"        => AirportType.Closed,
            _               => AirportType.Unknown
        };
    }

    private static AirportContinent ParseContinent(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return AirportContinent.Unknown;
        return s.Trim().ToUpperInvariant() switch
        {
            "AF" => AirportContinent.AF,
            "AN" => AirportContinent.AN,
            "AS" => AirportContinent.AS,
            "EU" => AirportContinent.EU,
            "NA" => AirportContinent.NA,
            "OC" => AirportContinent.OC,
            "SA" => AirportContinent.SA,
            _    => AirportContinent.Unknown
        };
    }

    private static Iso3166_Alpha2 ParseIsoCountry(string? s)
        => Enum.TryParse<Iso3166_Alpha2>(s?.Trim(), true, out var v) ? v : default; // your enum’s Unknown/default


}

