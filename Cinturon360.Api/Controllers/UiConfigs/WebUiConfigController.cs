using System.Diagnostics;
using Cinturon360.Shared.Models.DTOs;
using Cinturon360.Shared.Models.Search;
using Cinturon360.Shared.Models.Static.System.SysVar;
using Cinturon360.Shared.Services.Interfaces.Kernel;
using Cinturon360.Shared.Services.Interfaces.Travel;
using Microsoft.AspNetCore.Mvc;

namespace Cinturon360.Api.Controllers;

[ApiController]
[Produces("application/json")]
[Route("v1/webui/config")]
public sealed class WebUiConfigController : ControllerBase
{
    private const string CorrelationHeader = "X-Correlation-Id";

    private readonly ITravelQuoteService _travelQuoteService;
    private readonly ILoggerService _log;

    public WebUiConfigController(ILoggerService log, ITravelQuoteService travelQuoteService)
    {
        _log = log;
        _travelQuoteService = travelQuoteService;
    }

    [HttpPost("flight-search/{id}/")]
    public async Task<IActionResult> GetFlightSearchConfig(
        [FromRoute] string id,
        [FromBody] LogTraceDto dto,
        CancellationToken ct)
    {
        StampCorrelationId();

        var sw = Stopwatch.StartNew();
        var rid = GetCorrelationId();

        var config = await _travelQuoteService.GenerateFlightSearchUIOptionsAsync(id, rid, ct);

        await _log.InformationAsync(
            evt: SysLogEvtType.DATA_READ,
            cat: SysLogCatType.Api,
            act: SysLogActionType.Read,
            message: config is null
                ? "Flight search config not found"
                : "Flight search config generated",
            ent: nameof(FlightSearchPageConfig),
            entId: id,
            rid: rid,
            tid: dto.TID,
            durMs: (int)sw.ElapsedMilliseconds,
            http: Request.Method,
            stat: config is null ? StatusCodes.Status404NotFound : StatusCodes.Status200OK,
            path: HttpContext?.Request?.Path.Value,
            note: "egress:api");

        sw.Stop();

        return config is null ? NotFound() : Ok(config);
    }


    [HttpGet("hotel-search/{id}")]
    public Task<IActionResult> GetHotelSearchConfig([FromRoute] string id, CancellationToken ct)
        => throw new NotImplementedException();

    [HttpGet("car-search/{id}")]
    public Task<IActionResult> GetCarSearchConfig([FromRoute] string id, CancellationToken ct)
        => throw new NotImplementedException();

    [HttpGet("rail-search/{id}")]
    public Task<IActionResult> GetRailSearchConfig([FromRoute] string id, CancellationToken ct)
        => throw new NotImplementedException();

    [HttpGet("esim/{id}")]
    public Task<IActionResult> GetEsimConfig([FromRoute] string id, CancellationToken ct)
        => throw new NotImplementedException();


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