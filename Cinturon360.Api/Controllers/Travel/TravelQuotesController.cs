using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Cinturon360.Shared.Security;
using Cinturon360.Shared.Models.Kernel.Travel;
using Cinturon360.Shared.Services.Interfaces.Travel;
using Cinturon360.Shared.Models.Search;
using Cinturon360.Shared.Models.DTOs;
using Cinturon360.Shared.Services.Interfaces.External;
using Cinturon360.Shared.Services.Interfaces.Kernel;
using Cinturon360.Shared.Models.Static.SysVar;
using Cinturon360.Shared.Models.ExternalLib.Amadeus;

namespace Cinturon360.Api.Controllers.Travel;

[Route("api/v1/travel/quotes")]
// [ServiceFilter(typeof(RequireApiKeyFilter))]
[ApiController]
public sealed class TravelQuotesController : ControllerBase
{
    private readonly ITravelQuoteService _travelQuoteService;
    private readonly IAmadeusFlightSearchService _flightSearchService;
    private readonly ILoggerService _log;

    public TravelQuotesController(ITravelQuoteService travelQuoteService, IAmadeusFlightSearchService flightSearchService, ILoggerService log)
    {
        _travelQuoteService = travelQuoteService;
        _flightSearchService = flightSearchService;
        _log = log;
    }

    // ---------- READ (NO TRACKING via service) ----------
    [HttpGet("{id}")]
    public async Task<ActionResult<TravelQuote>> GetById(string id, CancellationToken ct)
    {
        var q = await _travelQuoteService.GetByIdAsync(id, ct);
        return q is null ? NotFound() : Ok(q);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TravelQuote>>> Search(
        [FromQuery] string? organizationId,
        [FromQuery] string? createdByUserId,
        [FromQuery] string? tmcAssignedId,
        [FromQuery] TravelQuoteType? type,
        [FromQuery] QuoteState? state,
        CancellationToken ct = default)
    {
        var result = await _travelQuoteService.SearchAsync(organizationId, createdByUserId, tmcAssignedId, type, state, ct);
        return Ok(result);
    }

    [HttpGet("ui/flightsearchpageconfig/{travelQuoteId}")]
    public async Task<ActionResult<FlightSearchPageConfig>> GetFlightSearchPageConfig(string travelQuoteId, CancellationToken ct)
    {
        var config = await _travelQuoteService.GenerateFlightSearchUIOptionsAsync(travelQuoteId, ct);
        return config is null ? NotFound() : Ok(config);
    }

    [HttpGet("ui/getflightresults/{travelQuoteId}")]
    public async Task<ActionResult<List<FlightViewOption>?>> GetFlightSearchResults(string travelQuoteId, CancellationToken ct)
    {
        // Retrieve flight search options based on travel quote ID
        var quote = await _travelQuoteService.GetByIdAsync(travelQuoteId, ct);
        if (quote == null)
        {
            // we should never reach this path at this point, this is called from a series of steps where the quote existence is already validated
            await _log.ErrorAsync(
                evt: "FLIGHT_SEARCH_OPTIONS_QUOTE_NOT_FOUND",
                cat: SysLogCatType.Data,
                act: SysLogActionType.Read,
                ex: new KeyNotFoundException($"Travel quote with ID '{travelQuoteId}' not found."),
                message: $"Travel quote with ID '{travelQuoteId}' not found when retrieving flight search options.",
                ent: nameof(TravelQuote),
                entId: travelQuoteId);

            return NotFound(null);
        }

        // because this is one-way search we pass false for isReturn
        AmadeusFlightOfferSearch criteria = await _travelQuoteService.BuildAmadeusFlightOfferSearchFromQuote(quote, false, ct);

        var amadeusFlightResultsResponse = await _flightSearchService.GetFlightOffersFromAmadeusFlightOfferSearch(criteria);

        if (amadeusFlightResultsResponse == null)
            return NotFound();

        var uiResults = await _travelQuoteService.GetFlightSearchResultsAsync(travelQuoteId, amadeusFlightResultsResponse, ct);

        return uiResults is null ? NotFound() : Ok(uiResults);
    }

    [HttpGet("ui/getreturnflightresults/{travelQuoteId}")]
    public async Task<ActionResult<List<FlightViewOption>?>> GetReturnFlightSearchResults(string travelQuoteId, CancellationToken ct)
    {
        // Retrieve flight search options based on travel quote ID
        var quote = await _travelQuoteService.GetByIdAsync(travelQuoteId, ct);
        if (quote == null)
        {
            // we should never reach this path at this point, this is called from a series of steps where the quote existence is already validated
            await _log.ErrorAsync(
                evt: "FLIGHT_SEARCH_OPTIONS_QUOTE_NOT_FOUND",
                cat: SysLogCatType.Data,
                act: SysLogActionType.Read,
                ex: new KeyNotFoundException($"Travel quote with ID '{travelQuoteId}' not found."),
                message: $"Travel quote with ID '{travelQuoteId}' not found when retrieving flight search options.",
                ent: nameof(TravelQuote),
                entId: travelQuoteId);

            return NotFound(null);
        }

        // because this is return search we pass true for isReturn
        AmadeusFlightOfferSearch criteria = await _travelQuoteService.BuildAmadeusFlightOfferSearchFromQuote(quote, true, ct);

        var response = await _flightSearchService.GetFlightOffersFromAmadeusFlightOfferSearch(criteria);

        var results = await _travelQuoteService.GetFlightSearchResultsAsync(travelQuoteId, response, ct);

        return results is null ? NotFound() : Ok(results);
    }

    // ---------- CREATE ----------
    [HttpPost]
    public async Task<ActionResult<TravelQuote>> Create([FromBody] TravelQuote input, CancellationToken ct)
    {
        var created = await _travelQuoteService.CreateAsync(input, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // ---------- UPDATE (PUT overwrites entire aggregate) ----------
    [HttpPut("{id}")]
    public async Task<ActionResult<TravelQuote>> Put(string id, [FromBody] TravelQuote input, CancellationToken ct)
    {
        if (!string.Equals(id, input.Id, StringComparison.Ordinal))
            return BadRequest("Id in route and body must match.");

        var updated = await _travelQuoteService.UpdatePutAsync(input, ct);
        return Ok(updated);
    }

    // ---------- DELETE ----------
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
        => (await _travelQuoteService.DeleteAsync(id, ct)) ? NoContent() : NotFound();

    // ---------- TARGETED UPDATES ----------
    [HttpPatch("{id}/created-by")]
    public async Task<IActionResult> UpdateCreatedBy(string id, [FromBody] UpdateCreatedByRequest body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.NewUserId)) return BadRequest("NewUserId is required.");
        return (await _travelQuoteService.ReassignCreatedByAsync(id, body.NewUserId, ct)) ? NoContent() : NotFound();
    }

    [HttpPatch("{id}/state")]
    public async Task<IActionResult> UpdateState(string id, [FromBody] UpdateStateRequest body, CancellationToken ct)
        => (await _travelQuoteService.UpdateStateAsync(id, body.State, ct)) ? NoContent() : NotFound();

    // ---------- DTO → ENTITY → RETURN Id ----------
    [HttpPost("from-dto")]
    public async Task<ActionResult<object>> CreateFromDto(
        [FromBody] TravelQuoteDto dto,
        CancellationToken ct)
    {
        if (dto is null) return BadRequest("Body required.");
        var (ok, error, travelQuoteId) = await _travelQuoteService.CreateFromDtoAsync(dto, ct);
        if (!ok || string.IsNullOrWhiteSpace(travelQuoteId))
            return BadRequest(new { error });

        return Ok(new { id = travelQuoteId });
    }

    [HttpGet("cron/expire-pending-quotes")]
    public async Task<IActionResult> ExpirePendingQuotes(CancellationToken ct)
    {
        var expiredCount = await _travelQuoteService.ExpireOldQuotesAsync(ct);
        return Ok(new { expiredCount });
    }

    [HttpPost("run-search/flight")]
    public async Task<ActionResult> RunFlightSearch(
        [FromBody] TravelQuoteFlightUIResultPatchDto dto,
        CancellationToken ct)
    {
        if (dto is null) return BadRequest("Body required.");

        await _travelQuoteService.IngestTravelQuoteFlightUIResultPatchDto(dto, ct);
        return Ok();  // always OK even if no matching quote found
    }

    // ---------- Request contracts ----------
    public sealed record UpdateCreatedByRequest([property: Required] string NewUserId);
    public sealed record UpdateStateRequest([property: Required] QuoteState State);
    // remove: public sealed record CreateFromDtoRequest([property: Required] TravelQuoteDto Dto);
}
