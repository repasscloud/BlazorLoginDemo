using BlazorLoginDemo.Shared.Data;
using BlazorLoginDemo.Shared.Models.DTOs;
using BlazorLoginDemo.Shared.Services.Interfaces.External;
using BlazorLoginDemo.Shared.Services.Interfaces.Kernel;
using Microsoft.AspNetCore.Mvc;
using BlazorLoginDemo.Shared.Security;
using BlazorLoginDemo.Shared.Models.Static.SysVar;
using BlazorLoginDemo.Shared.Models.Demo;
using BlazorLoginDemo.Shared.Services.Interfaces.Travel;
using BlazorLoginDemo.Shared.Models.Kernel.Travel;
using NanoidDotNet;
using BlazorLoginDemo.Shared.Models.ExternalLib.Amadeus;
using BlazorLoginDemo.Shared.Models.Search;

namespace BlazorLoginDemo.Api.Controllers.Flights;

[Route("api/v1/flights")]
[ApiController]
public class FlightsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IAmadeusAuthService _authService;
    private readonly IAmadeusFlightSearchService _flightSearchService;
    private readonly ITravelQuoteService _travelQuoteService;
    private readonly ILoggerService _log;

    public FlightsController(
        ApplicationDbContext db,
        IAmadeusAuthService authService,
        IAmadeusFlightSearchService flightSearchService,
        ITravelQuoteService travelQuoteService,
        ILoggerService loggerService)
    {
        _db = db;
        _authService = authService;
        _flightSearchService = flightSearchService;
        _travelQuoteService = travelQuoteService;
        _log = loggerService;
    }

    // POST: api/v1/flights/webapp/search
    // [ServiceFilter(typeof(RequireApiKeyFilter))]
    [HttpPost("webapp/search")]
    public async Task<IActionResult> SearchFlights(FlightOfferSearchRequestDto criteria, CancellationToken ct = default)
    {
        // add .CreatedAt (controlled excplicity by API)
        criteria.CreatedAt = DateTime.UtcNow;

        // check that a record doesn't match criteria.Id already, else return error msg
        var existing = await _db.FlightOfferSearchRequestDtos.FindAsync(criteria.Id);

        if (existing is not null)
        {
            await _log.ErrorAsync(
                evt: "FLIGHT_SEARCH_REQUEST_DUP",
                cat: SysLogCatType.Data,
                act: SysLogActionType.Validate,
                ex: new InvalidOperationException($"Table '{nameof(FlightOfferSearchRequestDto)}' has matching value for {criteria.Id}"),
                message: $"Duplicate FlightOfferSearchRequest detected for id={criteria.Id}",
                ent: nameof(FlightOfferSearchRequestDto),
                entId: criteria.Id,
                note: "duplicate");
            return BadRequest($"A record with Id = {criteria.Id} already exists.");
        }

        // save it to the db
        await _log.DebugAsync(
            evt: "FLIGHT_SEARCH_REQUEST_CREATE",
            cat: SysLogCatType.Data,
            act: SysLogActionType.Create,
            message: $"Created record '{nameof(FlightOfferSearchRequestDto)}' with ID '{criteria.Id}'",
            ent: nameof(FlightOfferSearchRequestDto),
            entId: criteria.Id);
        await _db.FlightOfferSearchRequestDtos.AddAsync(criteria, ct);

        // TravelSearchRecord travelSearchRecord = new TravelSearchRecord
        // {
        //     Id = 0,
        //     SearchId = criteria.Id,
        //     TravelType = TravelComponentType.Flight,
        //     FlightSubComponent = FlightSubComponentType.None,
        //     HotelSubComponent = HotelSubComponentType.None,
        //     CarSubComponent = CarSubComponentType.None,
        //     RailSubComponent = RailSubComponentType.None,
        //     TransferSubComponent = TransferSubComponentType.None,
        //     ActivitySubComponent = ActivitySubComponentType.None,
        //     CreatedAt = DateTime.UtcNow,
        //     ExpiresAt = DateTime.UtcNow.AddDays(30),
        //     Payload = string.Empty,
        // };

        if (criteria.IsOneWay)
        {
            var response = await _flightSearchService.GetFlightOffersAsync(criteria);
            return Ok(response);
        }
        else
        {
            return Ok();
        }
    }

    [HttpGet("quote/{travelQuoteId}/options")]
    public async Task<ActionResult<List<FlightOption>?>> GetFlightSearchOptions(string travelQuoteId, CancellationToken ct = default)
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
            return NotFound();
        }

        AmadeusFlightOfferSearch criteria = await _travelQuoteService.BuildAmadeusFlightOfferSearchFromQuote(quote, ct);

        var response = await _flightSearchService.GetFlightOffersFromAmadeusFlightOfferSearch(criteria);
        return Ok(response);
    }
}