using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Cinturon360.Shared.Models.Static.SysVar;
using Cinturon360.Shared.Services.Interfaces.Kernel;
using Cinturon360.Shared.Services.Interfaces.Travel;
using Cinturon360.Shared.Models.Kernel.Travel;
using Cinturon360.Shared.Security;
using Cinturon360.Shared.Models.DTOs;
using System.Text.Json;
using Cinturon360.Shared.Services.Interfaces.External;
using Cinturon360.Shared.Services.Interfaces.Platform;

namespace Cinturon360.Api.Controllers;

[ApiController]
[ServiceFilter(typeof(RequireApiKeyFilter))]
[Produces("application/json")]
[Route("v1/bookings")]
public sealed class WebBookingsController : ControllerBase
{
    private const string CorrelationHeader = "X-Correlation-Id";
    private readonly ITravelQuoteService _travelQuoteService;
    private readonly IQueuedJobService _queuedJobService;
    private readonly IAmadeusFlightSearchService _flightSearchService;
    private readonly IAdminOrgServiceUnified _adminOrgService;
    private readonly ILoggerService _log;

    public WebBookingsController(
        ITravelQuoteService travelQuoteService,
        IQueuedJobService queuedJobService,
        IAmadeusFlightSearchService flightSearchService,
        IAdminOrgServiceUnified adminOrgService,
        ILoggerService log)
    {
        _travelQuoteService = travelQuoteService;
        _queuedJobService = queuedJobService;
        _flightSearchService = flightSearchService;
        _adminOrgService = adminOrgService;
        _log = log;
    }

    // ---------- DTO → ENTITY → RETURN Id ----------
    [HttpPost("draft/new")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> CreateFromDto([FromBody] TravelQuoteDto dto, CancellationToken ct)
    {
        StampCorrelationId();

        var sw = Stopwatch.StartNew();
        var rid = GetCorrelationId();
        Guid tid = dto.Tid;

        if (dto is null)
        {
            await _log.WarningAsync(
                evt: "TRAVEL_QUOTE_DRAFT_CREATE_BODY_MISSING",
                cat: SysLogCatType.Api,
                act: SysLogActionType.Create,
                message: "Body required.",
                rid: rid,
                tid: tid,
                durMs: (int)sw.ElapsedMilliseconds,
                http: Request.Method,
                stat: StatusCodes.Status400BadRequest,
                path: HttpContext?.Request?.Path.Value,
                note: "errorCode=validation_failed");

            return ValidationProblem(title: "validation_failed", detail: "Body is required.");
        }

        // If you want RID to flow into deeper layers (and you accept having it on the DTO),
        // stamp it here. This requires Rid to be settable (not init-only).
        dto.Rid ??= rid;

        // create a travel quote and return its id
        var (ok, error, travelQuoteId) = await _travelQuoteService.CreateFromDtoAsync(dto, ct);

        if (!ok || string.IsNullOrWhiteSpace(travelQuoteId))
        {
            await _log.WarningAsync(
                evt: "TRAVEL_QUOTE_DRAFT_CREATE_FAILED",
                cat: SysLogCatType.Api,
                act: SysLogActionType.Create,
                message: "CreateFromDtoAsync returned failure.",
                ent: "TravelQuote",
                entId: null,
                rid: rid,
                tid: tid,
                org: dto.OrganizationId,
                uid: dto.CreatedByUserId,
                durMs: (int)sw.ElapsedMilliseconds,
                http: Request.Method,
                stat: StatusCodes.Status400BadRequest,
                path: HttpContext?.Request?.Path.Value,
                note: $"errorCode=bad_request; reason={error}");

            return BadRequest(new { error });
        }

        await _log.InformationAsync(
            evt: "TRAVEL_QUOTE_DRAFT_CREATED",
            cat: SysLogCatType.Api,
            act: SysLogActionType.Create,
            message: "Draft travel quote created.",
            ent: "TravelQuote",
            entId: travelQuoteId,
            rid: rid,
            tid: tid,
            org: dto.OrganizationId,
            uid: dto.CreatedByUserId,
            durMs: (int)sw.ElapsedMilliseconds,
            http: Request.Method,
            stat: StatusCodes.Status201Created,
            path: HttpContext?.Request?.Path.Value);

        sw.Stop();

        // If/when you add a GET endpoint, swap to CreatedAtAction.
        return StatusCode(StatusCodes.Status201Created, new { id = travelQuoteId, rid });
    }

    // ---------- QUEUED JOB ENQUEUEING ----------
    [HttpPost("drafts/{quoteId}/flight-searches")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> QueueFlightSearch(
        [FromBody] TravelQuoteFlightUIResultPatchDto dto,
        CancellationToken ct)
    {
        if (dto is null)
        {
            await _log.WarningAsync(
                evt: "FLIGHT_SEARCH_QUEUED_JOB_ENQUEUE_BODY_MISSING",
                cat: SysLogCatType.Api,
                act: SysLogActionType.Create,
                message: "Body required.",
                rid: GetCorrelationId(),
                tid: Guid.Empty,
                durMs: 0,
                http: Request.Method,
                stat: StatusCodes.Status400BadRequest,
                path: HttpContext?.Request?.Path.Value,
                note: "errorCode=validation_failed");

            return BadRequest("Body required.");
        }

        await _queuedJobService.EnqueueAsync(
            payload: dto,
            jobType: "FlightSearch",
            correlationId: dto.Id,
            availableAfterUtc: null,
            cancellationToken: ct);

        await _log.InformationAsync(
            evt: "FLIGHT_SEARCH_QUEUED_JOB_ENQUEUED",
            cat: SysLogCatType.Api,
            act: SysLogActionType.Create,
            message: "Flight search queued job enqueued.",
            ent: "TravelQuoteFlightUIResult",
            entId: dto.Id.ToString(),
            rid: GetCorrelationId(),
            tid: dto.Tid,
            org: dto.Org,
            uid: dto.Uid,
            durMs: 0,
            http: Request.Method,
            stat: StatusCodes.Status200OK);

        return Ok();
    }
    
    
    // ---------- FLIGHT SEARCH PROCESSING ----------
    [HttpPost("drafts/{quoteId}/flight-quotes")]
    [HttpPost("drafts/{quoteId}/flight-quotes/{legIndex:int}")]
    [ProducesResponseType(typeof(List<FlightViewOption>), StatusCodes.Status200OK)]
    // [ProducesResponseType(typeof(ProcessingResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<List<FlightViewOption>?>> GetFlightSearchResults(
        string quoteId,
        [FromRoute] int? legIndex,
        [FromBody] LogTraceDto dto,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(quoteId))
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid quoteId",
                Detail = "quoteId is required."
            });

        StampCorrelationId();

        var sw = Stopwatch.StartNew();
        var rid = GetCorrelationId();
        Guid tid = dto.Tid;

        var idx = 0;
        if (legIndex.HasValue)
        {
            if (legIndex.Value < 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid legIndex",
                    Detail = "legIndex must be greater than or equal to 0.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext?.Request?.Path.Value
                });
            }

            idx = legIndex.Value;
        }
        
        // Step 1: retrieve queued job by correlationId (quoteId) [this will only return a value if it's not .Succeeded, .Failed, .Cancelled]
        var queuedJob = await _queuedJobService.GetJobByCorrelationIdAndNotCompletedAsync(correlationId: quoteId, cancellationToken:ct);
        if (queuedJob is null)
            return NotFound(new ProblemDetails
            {
                Title = "Travel quote job not found in queue",
                Detail = $"No queued job found for quoteId '{quoteId}'."
            });
        
        var quoteJson = queuedJob.PayloadJson;
        if (string.IsNullOrWhiteSpace(quoteJson))
            return NotFound(new ProblemDetails
            {
                Title = "Travel quote job payload is empty",
                Detail = $"Queued job payload is empty for quoteId '{quoteId}'."
            });


        // deserialize payload, as we will need it for processing on the API side
        TravelQuoteFlightUIResultPatchDto? payload;
        try
        {
            payload = _queuedJobService.DeserializePayload<TravelQuoteFlightUIResultPatchDto>(queuedJob);
        }
        catch (JsonException ex)
        {
            await _log.ErrorAsync(
                evt: "QUEUED_JOB_PAYLOAD_DESERIALIZATION_FAILED",
                cat: SysLogCatType.Data,
                act: SysLogActionType.Read,
                ex: ex,
                rid: rid,
                tid: tid,
                durMs: (int)sw.ElapsedMilliseconds,
                http: Request.Method,
                stat: StatusCodes.Status400BadRequest,
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

        // null check after deserialization
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
        var quote = await _travelQuoteService.GetByIdAsync(quoteId, ct);
        if (quote is null)
            return NotFound(new ProblemDetails
            {
                Title = "Travel quote not found after ingestion",
                Detail = $"TravelQuote with id '{quoteId}' not found after ingesting queued job."
            });

        // get TmcId
        var tmcAmadeusContext = await _adminOrgService.GetAmadeusTmcContextAsync(clientOrgId: quote.OrganizationId, tmcOrgId: quote.TmcAssignedId, ct);
        if (tmcAmadeusContext == null)
        {
            // we should not be hitting this if the quote was properly validated earlier
            await _log.ErrorAsync(
                evt: "FLIGHT_SEARCH_TMC_CONTEXT_NOT_FOUND",
                cat: SysLogCatType.Data,
                act: SysLogActionType.Read,
                ex: new KeyNotFoundException($"TMC not found for organization ID '{quote.OrganizationId}'."),
                message: $"TMC not found for organization ID '{quote.OrganizationId}' when retrieving flight search options.",
                ent: nameof(TravelQuote),
                entId: quote.Id);
            return NotFound("TMC not found for the organization associated with the travel quote.");
        }

        string tmcId = tmcAmadeusContext.Tmc.Id;

        // Step 2: build criteria from hydrated quote
        var criteria = await _travelQuoteService.BuildAmadeusFlightOfferSearchFromQuote(
            quote,
            returnTrip: false,
            ct);

        // Step 2.1: handle legIndex if provided
        if (legIndex.HasValue)
        {
            var originDestinations = criteria.OriginDestinations;
            if (originDestinations is null || originDestinations.Count == 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid flight search criteria",
                    Detail = "No origin destinations are available to select a leg.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext?.Request?.Path.Value
                });
            }

            if (idx >= originDestinations.Count)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid legIndex",
                    Detail = $"legIndex {idx} is out of range. Must be between 0 and {originDestinations.Count - 1}.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext?.Request?.Path.Value
                });
            }

            var selectedOriginDestination = originDestinations[idx];
            originDestinations.Clear();
            originDestinations.Add(selectedOriginDestination);

            var selectedOriginDestinationId = selectedOriginDestination.Id;
            if (!string.IsNullOrWhiteSpace(selectedOriginDestinationId))
            {
                var cabinRestrictions = criteria.SearchCriteria?.Filters?.CabinRestrictions;
                if (cabinRestrictions != null)
                {
                    foreach (var restriction in cabinRestrictions)
                    {
                        var originDestinationIds = restriction?.OriginDestinationIds;
                        if (originDestinationIds is null)
                            continue;

                        originDestinationIds.Clear();
                        originDestinationIds.Add(selectedOriginDestinationId);
                    }
                }
            }
        }

        // // Step 2.2: save criteria to db_dump for review
        // await _queuedJobService.EnqueueAsync(
        //     payload: criteria,
        //     jobType: "FlightSearchCriteriaDump",
        //     correlationId: $"dbdump_{quoteId}",
        //     availableAfterUtc: null,
        //     cancellationToken: ct);

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
            quoteId,
            amadeusFlightResultsResponse,
            ct);

        if (uiResults is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "No flight results available",
                Detail = $"UI results mapping returned null for '{quoteId}'."
            });
        }

        // mark queued job as completed
        await _queuedJobService.MarkAsSucceededAsync(queuedJob, ct);

        await _log.InformationAsync(
            evt: "FLIGHT_SEARCH_RESULTS_RETRIEVED",
            cat: SysLogCatType.Api,
            act: SysLogActionType.Read,
            message: "Flight search results retrieved successfully.",
            ent: "TravelQuoteFlightUIResult",
            entId: quoteId,
            rid: rid,
            tid: tid,
            org: quote.OrganizationId,
            uid: dto.Uid,
            durMs: (int)sw.ElapsedMilliseconds,
            http: Request.Method,
            stat: StatusCodes.Status200OK);

        return Ok(uiResults);
    }


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
