using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlazorLoginDemo.Shared.Models.Policies;
using BlazorLoginDemo.Shared.Data;

namespace BlazorLoginDemo.Web.Api;

[ApiController]
[Route("api/[controller]")]
public class TravelPoliciesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public TravelPoliciesController(ApplicationDbContext db) => _db = db;

    // ---------------------------
    // TravelPolicy CRUD
    // ---------------------------

    // GET: api/travelpolicies
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TravelPolicyResponse>>> GetPolicies()
    {
        var items = await _db.TravelPolicies
            .Include(tp => tp.Regions)
            .Include(tp => tp.Continents)
            .ThenInclude(c => c.Countries)
            .Include(tp => tp.Countries)
            .Include(tp => tp.DisabledCountries)
            .ThenInclude(dc => dc.Country)
            .AsNoTracking()
            .ToListAsync();

        return items.Select(MapPolicyToResponse).ToList();
    }

    // GET: api/travelpolicies/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TravelPolicyResponse>> GetPolicy(string id)
    {
        var policy = await _db.TravelPolicies
            .Include(tp => tp.Regions)
            .Include(tp => tp.Continents)
            .Include(tp => tp.Countries)
            .Include(tp => tp.DisabledCountries)
            .ThenInclude(dc => dc.Country)
            .AsNoTracking()
            .FirstOrDefaultAsync(tp => tp.Id == id);

        if (policy is null) return NotFound();
        return MapPolicyToResponse(policy);
    }

    // GET: api/travelpolicies/by-client/{clientId}
    [HttpGet("by-client/{clientId}")]
    public async Task<ActionResult<IEnumerable<TravelPolicyResponse>>> GetPoliciesByClient(string clientId)
    {
        var items = await _db.TravelPolicies
            .Where(tp => tp.AvaClientId == clientId)
            .Include(tp => tp.Regions)
            .Include(tp => tp.Continents)
                .ThenInclude(c => c.Countries)
            .Include(tp => tp.Countries)
            .Include(tp => tp.DisabledCountries)
                .ThenInclude(dc => dc.Country)
            .AsNoTracking()
            .ToListAsync();

        if (!items.Any())
            return NotFound();

        return items.Select(MapPolicyToResponse).ToList();
    }


    // POST: api/travelpolicies
    [HttpPost]
    public async Task<ActionResult<TravelPolicyResponse>> CreatePolicy([FromBody] TravelPolicyUpsert dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var policy = new TravelPolicy
        {
            // Id auto-generates via NanoID in the entity
            PolicyName = dto.PolicyName,
            AvaClientId = dto.AvaClientId,
            DefaultCurrencyCode = dto.DefaultCurrencyCode,
            MaxFlightPrice = dto.MaxFlightPrice,
            DefaultFlightSeating = dto.DefaultFlightSeating,
            MaxFlightSeating = dto.MaxFlightSeating,
            IncludedAirlineCodes = dto.IncludedAirlineCodes,
            ExcludedAirlineCodes = dto.ExcludedAirlineCodes,
            CabinClassCoverage = dto.CabinClassCoverage,
            NonStopFlight = dto.NonStopFlight,
            FlightBookingTimeAvailableFrom = dto.FlightBookingTimeAvailableFrom,
            FlightBookingTimeAvailableTo = dto.FlightBookingTimeAvailableTo,
            EnableSaturdayFlightBookings = dto.EnableSaturdayFlightBookings,
            EnableSundayFlightBookings = dto.EnableSundayFlightBookings,
            DefaultCalendarDaysInAdvanceForFlightBooking = dto.DefaultCalendarDaysInAdvanceForFlightBooking
        };

        await ApplyPolicyRelations(policy, dto);
        _db.TravelPolicies.Add(policy);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPolicy), new { id = policy.Id }, MapPolicyToResponse(policy));
    }

    // PUT: api/travelpolicies/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<TravelPolicyResponse>> UpdatePolicy(string id, [FromBody] TravelPolicyUpsert dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var policy = await _db.TravelPolicies
            .Include(tp => tp.Regions)
            .Include(tp => tp.Continents)
            .Include(tp => tp.Countries)
            .Include(tp => tp.DisabledCountries)
            .FirstOrDefaultAsync(tp => tp.Id == id);

        if (policy is null) return NotFound();

        policy.PolicyName = dto.PolicyName;
        policy.AvaClientId = dto.AvaClientId;
        policy.DefaultCurrencyCode = dto.DefaultCurrencyCode;
        policy.MaxFlightPrice = dto.MaxFlightPrice;
        policy.DefaultFlightSeating = dto.DefaultFlightSeating;
        policy.MaxFlightSeating = dto.MaxFlightSeating;
        policy.IncludedAirlineCodes = dto.IncludedAirlineCodes;
        policy.ExcludedAirlineCodes = dto.ExcludedAirlineCodes;
        policy.CabinClassCoverage = dto.CabinClassCoverage;
        policy.NonStopFlight = dto.NonStopFlight;
        policy.FlightBookingTimeAvailableFrom = dto.FlightBookingTimeAvailableFrom;
        policy.FlightBookingTimeAvailableTo = dto.FlightBookingTimeAvailableTo;
        policy.EnableSaturdayFlightBookings = dto.EnableSaturdayFlightBookings;
        policy.EnableSundayFlightBookings = dto.EnableSundayFlightBookings;
        policy.DefaultCalendarDaysInAdvanceForFlightBooking = dto.DefaultCalendarDaysInAdvanceForFlightBooking;

        await ApplyPolicyRelations(policy, dto);
        await _db.SaveChangesAsync();

        // refetch fully for response consistency
        var updated = await _db.TravelPolicies
            .Include(tp => tp.Regions)
            .Include(tp => tp.Continents)
            .ThenInclude(c => c.Countries)
            .Include(tp => tp.Countries)
            .Include(tp => tp.DisabledCountries)
            .ThenInclude(dc => dc.Country)
            .AsNoTracking()
            .FirstAsync(tp => tp.Id == id);

        return MapPolicyToResponse(updated);
    }

    // DELETE: api/travelpolicies/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePolicy(string id)
    {
        var policy = await _db.TravelPolicies
            .Include(tp => tp.DisabledCountries)
            .FirstOrDefaultAsync(tp => tp.Id == id);

        if (policy is null) return NotFound();

        // Remove join rows first to avoid FK issues
        if (policy.DisabledCountries.Any())
            _db.TravelPolicyDisabledCountries.RemoveRange(policy.DisabledCountries);

        _db.TravelPolicies.Remove(policy);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task ApplyPolicyRelations(TravelPolicy policy, TravelPolicyUpsert dto)
    {
        // Regions
        policy.Regions.Clear();
        if (dto.RegionIds?.Count > 0)
        {
            var regions = await _db.Regions.Where(r => dto.RegionIds.Contains(r.Id)).ToListAsync();
            foreach (var r in regions) policy.Regions.Add(r);
        }

        // Continents
        policy.Continents.Clear();
        if (dto.ContinentIds?.Count > 0)
        {
            var continents = await _db.Continents.Where(c => dto.ContinentIds.Contains(c.Id)).ToListAsync();
            foreach (var c in continents) policy.Continents.Add(c);
        }

        // Countries
        policy.Countries.Clear();
        if (dto.CountryIds?.Count > 0)
        {
            var countries = await _db.Countries.Where(cn => dto.CountryIds.Contains(cn.Id)).ToListAsync();
            foreach (var cn in countries) policy.Countries.Add(cn);
        }

        // DisabledCountries (join entity)
        // Clear existing joins
        var existing = await _db.TravelPolicyDisabledCountries
            .Where(x => x.TravelPolicyId == policy.Id)
            .ToListAsync();
        if (existing.Count > 0)
            _db.TravelPolicyDisabledCountries.RemoveRange(existing);

        if (dto.DisabledCountryIds?.Count > 0)
        {
            foreach (var cid in dto.DisabledCountryIds.Distinct())
            {
                _db.TravelPolicyDisabledCountries.Add(new TravelPolicyDisabledCountry
                {
                    TravelPolicyId = policy.Id,
                    CountryId = cid
                });
            }
        }
    }

    private static TravelPolicyResponse MapPolicyToResponse(TravelPolicy policy)
        => new()
        {
            Id = policy.Id,
            PolicyName = policy.PolicyName,
            AvaClientId = policy.AvaClientId,
            DefaultCurrencyCode = policy.DefaultCurrencyCode,
            MaxFlightPrice = policy.MaxFlightPrice,
            DefaultFlightSeating = policy.DefaultFlightSeating,
            MaxFlightSeating = policy.MaxFlightSeating,
            IncludedAirlineCodes = policy.IncludedAirlineCodes,
            ExcludedAirlineCodes = policy.ExcludedAirlineCodes,
            CabinClassCoverage = policy.CabinClassCoverage,
            NonStopFlight = policy.NonStopFlight,
            FlightBookingTimeAvailableFrom = policy.FlightBookingTimeAvailableFrom,
            FlightBookingTimeAvailableTo = policy.FlightBookingTimeAvailableTo,
            EnableSaturdayFlightBookings = policy.EnableSaturdayFlightBookings,
            EnableSundayFlightBookings = policy.EnableSundayFlightBookings,
            DefaultCalendarDaysInAdvanceForFlightBooking = policy.DefaultCalendarDaysInAdvanceForFlightBooking,
            RegionIds = policy.Regions.Select(r => r.Id).ToList(),
            ContinentIds = policy.Continents.Select(c => c.Id).ToList(),
            CountryIds = policy.Countries.Select(cn => cn.Id).ToList(),
            DisabledCountryIds = policy.DisabledCountries.Select(dc => dc.CountryId).ToList()
        };

    // ---------------------------
    // Regions CRUD (under same controller)
    // ---------------------------

    [HttpGet("regions")]
    public async Task<IEnumerable<Region>> GetRegions() =>
        await _db.Regions.AsNoTracking().ToListAsync();

    [HttpGet("regions/{id:int}")]
    public async Task<ActionResult<Region>> GetRegion(int id)
    {
        var r = await _db.Regions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return r is null ? NotFound() : r;
    }

    [HttpPost("regions")]
    public async Task<ActionResult<Region>> CreateRegion([FromBody] RegionUpsert dto)
    {
        var entity = new Region { Name = dto.Name };
        _db.Regions.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetRegion), new { id = entity.Id }, entity);
    }

    [HttpPut("regions/{id:int}")]
    public async Task<ActionResult<Region>> UpdateRegion(int id, [FromBody] RegionUpsert dto)
    {
        var entity = await _db.Regions.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();
        entity.Name = dto.Name;
        await _db.SaveChangesAsync();
        return entity;
    }

    [HttpDelete("regions/{id:int}")]
    public async Task<IActionResult> DeleteRegion(int id)
    {
        var entity = await _db.Regions.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();
        _db.Regions.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ---------------------------
    // Continents CRUD
    // ---------------------------

    [HttpGet("continents")]
    public async Task<IEnumerable<Continent>> GetContinents() =>
        await _db.Continents.AsNoTracking().ToListAsync();

    [HttpGet("continents/{id:int}")]
    public async Task<ActionResult<Continent>> GetContinent(int id)
    {
        var c = await _db.Continents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return c is null ? NotFound() : c;
    }

    [HttpPost("continents")]
    public async Task<ActionResult<Continent>> CreateContinent([FromBody] ContinentUpsert dto)
    {
        var entity = new Continent
        {
            Name = dto.Name,
            IsoCode = dto.IsoCode,
            RegionId = dto.RegionId
        };
        _db.Continents.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetContinent), new { id = entity.Id }, entity);
    }

    [HttpPut("continents/{id:int}")]
    public async Task<ActionResult<Continent>> UpdateContinent(int id, [FromBody] ContinentUpsert dto)
    {
        var entity = await _db.Continents.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();
        entity.Name = dto.Name;
        entity.IsoCode = dto.IsoCode;
        entity.RegionId = dto.RegionId;
        await _db.SaveChangesAsync();
        return entity;
    }

    [HttpDelete("continents/{id:int}")]
    public async Task<IActionResult> DeleteContinent(int id)
    {
        var entity = await _db.Continents.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();
        _db.Continents.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ---------------------------
    // Countries CRUD
    // ---------------------------

    [HttpGet("countries")]
    public async Task<IEnumerable<Country>> GetCountries() =>
        await _db.Countries.AsNoTracking().ToListAsync();

    [HttpGet("countries/{id:int}")]
    public async Task<ActionResult<Country>> GetCountry(int id)
    {
        var c = await _db.Countries.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return c is null ? NotFound() : c;
    }

    [HttpPost("countries")]
    public async Task<ActionResult<Country>> CreateCountry([FromBody] CountryUpsert dto)
    {
        var entity = new Country
        {
            Name = dto.Name,
            IsoCode = dto.IsoCode,
            Flag = dto.Flag,
            ContinentId = dto.ContinentId
        };
        _db.Countries.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCountry), new { id = entity.Id }, entity);
    }

    [HttpPut("countries/{id:int}")]
    public async Task<ActionResult<Country>> UpdateCountry(int id, [FromBody] CountryUpsert dto)
    {
        var entity = await _db.Countries.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();
        entity.Name = dto.Name;
        entity.IsoCode = dto.IsoCode;
        entity.Flag = dto.Flag;
        entity.ContinentId = dto.ContinentId;
        await _db.SaveChangesAsync();
        return entity;
    }

    [HttpDelete("countries/{id:int}")]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        var entity = await _db.Countries.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();
        _db.Countries.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ---------------------------
    // Disabled Countries (join rows) helpers
    // ---------------------------

    // GET: api/travelpolicies/{id}/disabled-countries
    [HttpGet("{id}/disabled-countries")]
    public async Task<ActionResult<IEnumerable<Country>>> GetDisabledCountries(string id)
    {
        var exists = await _db.TravelPolicies.AnyAsync(x => x.Id == id);
        if (!exists) return NotFound();

        var list = await _db.TravelPolicyDisabledCountries
            .Where(x => x.TravelPolicyId == id)
            .Include(x => x.Country)
            .Select(x => x.Country!)
            .AsNoTracking()
            .ToListAsync();

        return list;
    }

    // POST: api/travelpolicies/{id}/disabled-countries (body: { countryIds: [1,2,3] })
    [HttpPost("{id}/disabled-countries")]
    public async Task<IActionResult> SetDisabledCountries(string id, [FromBody] DisabledCountrySet dto)
    {
        var policy = await _db.TravelPolicies.FirstOrDefaultAsync(x => x.Id == id);
        if (policy is null) return NotFound();

        var existing = await _db.TravelPolicyDisabledCountries.Where(x => x.TravelPolicyId == id).ToListAsync();
        if (existing.Count > 0) _db.TravelPolicyDisabledCountries.RemoveRange(existing);

        if (dto.CountryIds?.Count > 0)
        {
            foreach (var cid in dto.CountryIds.Distinct())
            {
                _db.TravelPolicyDisabledCountries.Add(new TravelPolicyDisabledCountry
                {
                    TravelPolicyId = id,
                    CountryId = cid
                });
            }
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ---------------------------
    // DTOs
    // ---------------------------

    public record TravelPolicyUpsert
    {
        [Required] public string PolicyName { get; init; } = default!;
        public string AvaClientId { get; init; } = default!;
        [Required, RegularExpression(@"^[A-Z]{3}$")] public string DefaultCurrencyCode { get; init; } = "AUD";
        public decimal MaxFlightPrice { get; init; }
        public string DefaultFlightSeating { get; init; } = "ECONOMY";
        public string MaxFlightSeating { get; init; } = "ECONOMY";
        public string? IncludedAirlineCodes { get; init; }
        public string? ExcludedAirlineCodes { get; init; }
        public string CabinClassCoverage { get; init; } = "MOST_SEGMENTS";
        public bool NonStopFlight { get; init; }
        public string? FlightBookingTimeAvailableFrom { get; init; }
        public string? FlightBookingTimeAvailableTo { get; init; }
        public bool EnableSaturdayFlightBookings { get; init; }
        public bool EnableSundayFlightBookings { get; init; }
        public int DefaultCalendarDaysInAdvanceForFlightBooking { get; init; }

        public List<int>? RegionIds { get; init; }
        public List<int>? ContinentIds { get; init; }
        public List<int>? CountryIds { get; init; }
        public List<int>? DisabledCountryIds { get; init; }
    }

    public record TravelPolicyResponse
    {
        public string Id { get; init; } = default!;
        public string PolicyName { get; init; } = default!;
        public string AvaClientId { get; init; } = default!;
        public string DefaultCurrencyCode { get; init; } = default!;
        public decimal MaxFlightPrice { get; init; }
        public string DefaultFlightSeating { get; init; } = default!;
        public string MaxFlightSeating { get; init; } = default!;
        public string? IncludedAirlineCodes { get; init; }
        public string? ExcludedAirlineCodes { get; init; }
        public string CabinClassCoverage { get; init; } = default!;
        public bool NonStopFlight { get; init; }
        public string? FlightBookingTimeAvailableFrom { get; init; }
        public string? FlightBookingTimeAvailableTo { get; init; }
        public bool EnableSaturdayFlightBookings { get; init; }
        public bool EnableSundayFlightBookings { get; init; }
        public int DefaultCalendarDaysInAdvanceForFlightBooking { get; init; }

        public List<int> RegionIds { get; init; } = new();
        public List<int> ContinentIds { get; init; } = new();
        public List<int> CountryIds { get; init; } = new();
        public List<int> DisabledCountryIds { get; init; } = new();
    }

    public record RegionUpsert([Required] string Name);
    public record ContinentUpsert([Required] string Name, [Required] string IsoCode, int? RegionId);
    public record CountryUpsert([Required] string Name, [Required] string IsoCode, [Required] string Flag, int? ContinentId);
    public record DisabledCountrySet(List<int> CountryIds);
}
