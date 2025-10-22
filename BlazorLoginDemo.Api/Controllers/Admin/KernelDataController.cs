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
        if (take > 500) take = 500;
        if (skip < 0)  skip = 0;

        var result = await _airportInfoService.SearchMultiAsync(
            query,
            types,
            continents,
            countries,
            hasIata        ?? true,
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
}
