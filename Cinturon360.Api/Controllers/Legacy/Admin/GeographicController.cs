using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Cinturon360.Shared.Models.Policies;
using Cinturon360.Shared.Services.Interfaces.Policies;

namespace Cinturon360.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/geographic")]
public sealed class GeographicController : ControllerBase
{
    private readonly IRegionService _regions;
    private readonly IContinentService _continents;
    private readonly ICountryService _countries;

    public GeographicController(
        IRegionService regions,
        IContinentService continents,
        ICountryService countries)
    {
        _regions = regions;
        _continents = continents;
        _countries = countries;
    }

    // ----------------------------
    // Regions
    // ----------------------------

    [HttpGet("regions")]
    public async Task<IActionResult> GetRegions(CancellationToken ct)
        => Ok(await _regions.GetAllAsync(ct));

    [HttpGet("regions/{id:int}")]
    public async Task<IActionResult> GetRegionById(int id, CancellationToken ct)
    {
        var r = await _regions.GetByIdAsync(id, ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpGet("regions/by-name/{name}")]
    public async Task<IActionResult> GetRegionByName(string name, CancellationToken ct)
    {
        var r = await _regions.GetByNameAsync(name, ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpPost("regions")]
    public async Task<IActionResult> CreateRegion([FromBody] Region region, CancellationToken ct)
    {
        try
        {
            var created = await _regions.CreateAsync(region, ct);
            return CreatedAtAction(nameof(GetRegionById), new { id = created.Id }, created);
        }
        catch (System.ArgumentException ex) { return BadRequest(ex.Message); }
        catch (System.InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    [HttpPut("regions/{id:int}")]
    public async Task<IActionResult> UpdateRegion(int id, [FromBody] Region region, CancellationToken ct)
    {
        if (id != region.Id) return BadRequest("Route id mismatch.");
        try
        {
            var updated = await _regions.UpdateAsync(region, ct);
            return Ok(updated);
        }
        catch (System.ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("regions/{id:int}")]
    public async Task<IActionResult> DeleteRegion(int id, CancellationToken ct)
        => await _regions.DeleteAsync(id, ct) ? NoContent() : NotFound();

    // ----------------------------
    // Continents
    // ----------------------------

    [HttpGet("continents")]
    public async Task<IActionResult> GetContinents(CancellationToken ct)
        => Ok(await _continents.GetAllAsync(ct));

    [HttpGet("continents/{id:int}")]
    public async Task<IActionResult> GetContinentById(int id, CancellationToken ct)
    {
        var c = await _continents.GetByIdAsync(id, ct);
        return c is null ? NotFound() : Ok(c);
    }

    [HttpGet("continents/by-name/{name}")]
    public async Task<IActionResult> GetContinentByName(string name, CancellationToken ct)
    {
        var c = await _continents.GetByNameAsync(name, ct);
        return c is null ? NotFound() : Ok(c);
    }

    // Optional “assigned” endpoint exposed by your interface
    [HttpGet("continents/{id:int}/countries")]
    public async Task<IActionResult> GetCountriesAssignedToContinent(int id, CancellationToken ct)
        => Ok(await _continents.GetAssignedCountriesAsync(id, ct));

    [HttpPost("continents")]
    public async Task<IActionResult> CreateContinent([FromBody] Continent continent, CancellationToken ct)
    {
        try
        {
            var created = await _continents.CreateAsync(continent, ct);
            return CreatedAtAction(nameof(GetContinentById), new { id = created.Id }, created);
        }
        catch (System.ArgumentException ex) { return BadRequest(ex.Message); }
        catch (System.InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    [HttpPut("continents/{id:int}")]
    public async Task<IActionResult> UpdateContinent(int id, [FromBody] Continent continent, CancellationToken ct)
    {
        if (id != continent.Id) return BadRequest("Route id mismatch.");
        try
        {
            var updated = await _continents.UpdateAsync(continent, ct);
            return Ok(updated);
        }
        catch (System.ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("continents/{id:int}")]
    public async Task<IActionResult> DeleteContinent(int id, CancellationToken ct)
        => await _continents.DeleteAsync(id, ct) ? NoContent() : NotFound();

    // ----------------------------
    // Countries
    // ----------------------------

    [HttpGet("countries")]
    public async Task<IActionResult> GetCountries(CancellationToken ct)
        => Ok(await _countries.GetAllAsync(ct));

    [HttpGet("countries/{id:int}")]
    public async Task<IActionResult> GetCountryById(int id, CancellationToken ct)
    {
        var c = await _countries.GetByIdAsync(id, ct);
        return c is null ? NotFound() : Ok(c);
    }

    [HttpGet("countries/by-name/{name}")]
    public async Task<IActionResult> GetCountryByName(string name, CancellationToken ct)
    {
        var c = await _countries.GetByNameAsync(name, ct);
        return c is null ? NotFound() : Ok(c);
    }

    [HttpPost("countries")]
    public async Task<IActionResult> CreateCountry([FromBody] Country country, CancellationToken ct)
    {
        try
        {
            var created = await _countries.CreateAsync(country, ct);
            return CreatedAtAction(nameof(GetCountryById), new { id = created.Id }, created);
        }
        catch (System.ArgumentException ex) { return BadRequest(ex.Message); }
        catch (System.InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    [HttpPut("countries/{id:int}")]
    public async Task<IActionResult> UpdateCountry(int id, [FromBody] Country country, CancellationToken ct)
    {
        if (id != country.Id) return BadRequest("Route id mismatch.");
        try
        {
            var updated = await _countries.UpdateAsync(country, ct);
            return Ok(updated);
        }
        catch (System.ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("countries/{id:int}")]
    public async Task<IActionResult> DeleteCountry(int id, CancellationToken ct)
        => await _countries.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
