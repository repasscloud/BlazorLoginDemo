using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Cinturon360.Shared.Models.Kernel.Travel;
using Cinturon360.Shared.Services.Interfaces.Travel;
using Cinturon360.Shared.Models.Search;
using Cinturon360.Shared.Models.DTOs;
using Cinturon360.Shared.Services.Interfaces.External;
using Cinturon360.Shared.Services.Interfaces.Kernel;
using Cinturon360.Shared.Models.ExternalLib.Amadeus;
using System.Text.Json;
using System.Diagnostics;
using Cinturon360.Shared.Services.Interfaces.Platform;
using Cinturon360.Shared.Models.Static.System.SysVar;

namespace Cinturon360.Api.Controllers.Travel;

[Route("api/v1/travel/quotes")]
// [ServiceFilter(typeof(RequireApiKeyFilter))]
[ApiController]
public sealed class TravelQuotesController : ControllerBase
{
    private const string CorrelationHeader = "X-Correlation-Id";
    private readonly ITravelQuoteService _travelQuoteService;
    private readonly IAmadeusFlightSearchService _flightSearchService;
    private readonly IQueuedJobService _queuedJobService;
    private readonly IAdminOrgServiceUnified _adminOrgService;
    private readonly ILoggerService _log;

    public TravelQuotesController(
        ITravelQuoteService travelQuoteService,
        IAmadeusFlightSearchService flightSearchService,
        IQueuedJobService queuedJobService,
        IAdminOrgServiceUnified adminOrgService,
        ILoggerService log)
    {
        _travelQuoteService = travelQuoteService;
        _flightSearchService = flightSearchService;
        _queuedJobService = queuedJobService;
        _adminOrgService = adminOrgService;
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
        var config = await _travelQuoteService.GenerateFlightSearchUIOptionsAsync(travelQuoteId, "x", ct);
        return config is null ? NotFound() : Ok(config);
    }

    // [HttpGet("ui/getflightresults/{travelQuoteId}")]
    // public async Task<ActionResult<List<FlightViewOption>?>> GetFlightSearchResults(string travelQuoteId, CancellationToken ct)
    // {
    //     // Retrieve flight search options based on travel quote ID
    //     var quote = await _travelQuoteService.GetByIdAsync(travelQuoteId, ct);
    //     if (quote is null)
    //     {
    //         // because we moved to a queue based flight search, it might not be in the system (ie - processed) yet, so ask the
    //         // queue service to process it now, then try again, else fail gracefully
    //         var dto = await _queuedJobService.RetrieveTravelQuoteFlightUIResultPatchDtoJobAsync(travelQuoteId, ct);
    //         if (dto is not null)
    //         {
    //             // process now
    //             await _travelQuoteService.IngestTravelQuoteFlightUIResultPatchDto(dto, ct);

    //             // try again
    //             quote = await _travelQuoteService.GetByIdAsync(travelQuoteId, ct);
    //             if (quote is not null)
    //             {
    //                 // because this is one-way search we pass false for isReturn
    //                 AmadeusFlightOfferSearch criteria = await _travelQuoteService.BuildAmadeusFlightOfferSearchFromQuote(quote, false, ct);

    //                 var amadeusFlightResultsResponse = await _flightSearchService.GetFlightOffersFromAmadeusFlightOfferSearch(criteria);

    //                 if (amadeusFlightResultsResponse == null)
    //                     return NotFound();

    //                 var uiResults = await _travelQuoteService.GetFlightSearchResultsAsync(travelQuoteId, amadeusFlightResultsResponse, ct);

    //                 return uiResults is null ? NotFound() : Ok(uiResults);
    //             }
    //         }
            
    //         // we should never reach this path at this point, this is called from a series of steps where the quote existence is already validated
    //         await _log.ErrorAsync(
    //             evt: "FLIGHT_SEARCH_OPTIONS_QUOTE_NOT_FOUND",
    //             cat: SysLogCatType.Data,
    //             act: SysLogActionType.Read,
    //             ex: new KeyNotFoundException($"Travel quote with ID '{travelQuoteId}' not found."),
    //             message: $"Travel quote with ID '{travelQuoteId}' not found when retrieving flight search options.",
    //             ent: nameof(TravelQuote),
    //             entId: travelQuoteId);

    //         return NotFound(null);
    //     }

    //     // because this is one-way search we pass false for isReturn
    //     AmadeusFlightOfferSearch criteria = await _travelQuoteService.BuildAmadeusFlightOfferSearchFromQuote(quote, false, ct);

    //     var amadeusFlightResultsResponse = await _flightSearchService.GetFlightOffersFromAmadeusFlightOfferSearch(criteria);

    //     if (amadeusFlightResultsResponse == null)
    //         return NotFound();

    //     var uiResults = await _travelQuoteService.GetFlightSearchResultsAsync(travelQuoteId, amadeusFlightResultsResponse, ct);

    //     return uiResults is null ? NotFound() : Ok(uiResults);
    // }



    [HttpGet("ui/getflightresults/{travelQuoteId}")]
    [ProducesResponseType(typeof(List<FlightViewOption>), StatusCodes.Status200OK)]
    // [ProducesResponseType(typeof(ProcessingResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<List<FlightViewOption>?>> GetFlightSearchResults(
        string travelQuoteId,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(travelQuoteId))
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid travelQuoteId",
                Detail = "travelQuoteId is required."
            });
            
        var queuedJob = await _queuedJobService.GetJobByCorrelationIdAndNotCompletedAsync(travelQuoteId, ct);
        if (queuedJob is null)
            return NotFound(new ProblemDetails
            {
                Title = "Travel quote job not found in queue",
                Detail = $"No queued job found for travelQuoteId '{travelQuoteId}'."
            });
        
        var quoteJson = queuedJob.PayloadJson;
        if (string.IsNullOrWhiteSpace(quoteJson))
            return NotFound(new ProblemDetails
            {
                Title = "Travel quote job payload is empty",
                Detail = $"Queued job payload is empty for travelQuoteId '{travelQuoteId}'."
            });

        TravelQuoteFlightUIResultPatchDto? payload = null;

        try
        {
            payload = _queuedJobService.DeserializePayload<TravelQuoteFlightUIResultPatchDto>(queuedJob);
        }
        catch (JsonException ex)
        {
            await _log.ErrorAsync(
                evt: SysLogEvtType.QUEUE_MSG_FAIL,
                cat: SysLogCatType.Data,
                act: SysLogActionType.Read,
                ex: ex,
                message: $"Failed to deserialize payload for QueuedJob {queuedJob.Id} ({queuedJob.JobType}).",
                ent: nameof(queuedJob),
                entId: queuedJob.Id.ToString());

            return BadRequest(new ProblemDetails
            {
                Title = "Invalid queued job payload",
                Detail = "The queued job payload could not be deserialized.",
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext?.Request?.Path.Value
            });
        }

        if (payload is null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid queued job payload",
                Detail = "The queued job payload could not be deserialized (null).",
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext?.Request?.Path.Value
            });
        }

        // mark job as processing
        await _queuedJobService.MarkAsProcessingAsync(queuedJob, ct);

        // process now
        await _travelQuoteService.IngestTravelQuoteFlightUIResultPatchDto(payload, ct);

        // retrieve quote (should exist now)
        var quote = await _travelQuoteService.GetByIdAsync(travelQuoteId, ct);
        if (quote is null)
            return NotFound(new ProblemDetails
            {
                Title = "Travel quote not found after ingestion",
                Detail = $"TravelQuote with id '{travelQuoteId}' not found after ingesting queued job."
            });

        // Step 2: build criteria from hydrated quote
        var criteria = await _travelQuoteService.BuildAmadeusFlightOfferSearchFromQuote(
            quote,
            returnTrip: false,
            ct);

        // Set 2.1: get TMC ID
        var tmcInfo = await _adminOrgService.GetGoverningTmcInfoAsync(quote.OrganizationId, ct);
        if (tmcInfo == null)
        {
            await _log.ErrorAsync(
                evt: SysLogEvtType.DATA_READ_ERR,
                cat: SysLogCatType.Data,
                act: SysLogActionType.Read,
                ex: new KeyNotFoundException($"TMC not found for organization ID '{quote.OrganizationId}'."),
                message: $"TMC not found for organization ID '{quote.OrganizationId}' when retrieving flight search options.",
                ent: nameof(TravelQuote),
                entId: quote.Id);
            return NotFound("TMC not found for the organization associated with the travel quote.");
        }

        string tmcId = tmcInfo.TmcId;

        // Step 3: call provider
        var amadeusFlightResultsResponse =
            await _flightSearchService.GetFlightOffersFromAmadeusFlightOfferSearch(criteria, tmcId, ct);

        if (amadeusFlightResultsResponse is null)
        {
            // Another improvement:
            // A provider failure/no-response is not really a 404.
            return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
            {
                Title = "Flight provider did not return a response",
                Detail = "Amadeus response was null."
            });
        }

        // Step 4: map to UI model
        var uiResults = await _travelQuoteService.GetFlightSearchResultsAsync(
            travelQuoteId,
            amadeusFlightResultsResponse,
            ct);

        if (uiResults is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "No flight results available",
                Detail = $"UI results mapping returned null for '{travelQuoteId}'."
            });
        }

        // mark queued job as completed
        await _queuedJobService.MarkAsSucceededAsync(queuedJob, ct);

        return Ok(uiResults);
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
                evt: SysLogEvtType.DATA_READ_ERR,
                cat: SysLogCatType.Data,
                act: SysLogActionType.Read,
                ex: new KeyNotFoundException($"Travel quote with ID '{travelQuoteId}' not found."),
                message: $"Travel quote with ID '{travelQuoteId}' not found when retrieving flight search options.",
                ent: nameof(TravelQuote),
                entId: travelQuoteId);

            return NotFound(null);
        }

        // get Tmc ID
        var tmcInfo = await _adminOrgService.GetGoverningTmcInfoAsync(quote.OrganizationId, ct);
        if (tmcInfo == null)
        {
            await _log.ErrorAsync(
                evt: SysLogEvtType.DATA_READ_ERR,
                cat: SysLogCatType.Data,
                act: SysLogActionType.Read,
                ex: new KeyNotFoundException($"TMC not found for organization ID '{quote.OrganizationId}'."),
                message: $"TMC not found for organization ID '{quote.OrganizationId}' when retrieving flight search options.",
                ent: nameof(TravelQuote),
                entId: quote.Id);
            return NotFound("TMC not found for the organization associated with the travel quote.");
        }

        string tmcId = tmcInfo.TmcId;

        // because this is return search we pass true for isReturn
        AmadeusFlightOfferSearch criteria = await _travelQuoteService.BuildAmadeusFlightOfferSearchFromQuote(quote, true, ct);

        var response = await _flightSearchService.GetFlightOffersFromAmadeusFlightOfferSearch(criteria, tmcId, ct);

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

    [HttpPost("queue-search/flight")]
    public async Task<ActionResult> RunFlightSearch(
        [FromBody] TravelQuoteFlightUIResultPatchDto dto,
        CancellationToken ct)
    {
        if (dto is null) return BadRequest("Body required.");

        await _queuedJobService.EnqueueAsync(
            payload: dto,
            jobType: "FlightSearch",
            correlationId: dto.Id,
            availableAfterUtc: null,
            cancellationToken: ct);

        //await _travelQuoteService.IngestTravelQuoteFlightUIResultPatchDto(dto, ct);
        return Ok();  // always OK even if no matching quote found
    }

    // ---------- Request contracts ----------
    public sealed record UpdateCreatedByRequest([property: Required] string NewUserId);
    public sealed record UpdateStateRequest([property: Required] QuoteState State);
    // remove: public sealed record CreateFromDtoRequest([property: Required] TravelQuoteDto Dto);

        // ------------------------------------------------------------------------
    // Correlation helpers
    // ------------------------------------------------------------------------

    private void StampCorrelationId()
    {
        var rid = GetCorrelationId();

        if (!Response.Headers.ContainsKey(CorrelationHeader))
            Response.Headers[CorrelationHeader] = rid;

        Response.Headers["X-Trace-Id"] = GetTraceId();
        Response.Headers["Cache-Control"] = "no-store";
    }

    private string GetCorrelationId()
    {
        var incoming = Request.Headers[CorrelationHeader].ToString();
        return string.IsNullOrWhiteSpace(incoming) ? HttpContext.TraceIdentifier : incoming;
    }

    private static string GetTraceId()
        => Activity.Current?.Id ?? string.Empty;
}
