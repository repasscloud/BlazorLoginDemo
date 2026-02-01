using System.Diagnostics;
using Cinturon360.Shared.Services.Interfaces.Kernel;
using Microsoft.AspNetCore.Mvc;
using Cinturon360.Shared.Contracts.Geography;
using Cinturon360.Shared.Services.Interfaces.API.Geography;
using Cinturon360.Shared.Contracts;
using Cinturon360.Shared.Models.Static.System.SysVar;

namespace Cinturon360.Api.Controllers;

[ApiController]
[Produces("application/json")]
[Route("v1/geography")]
public sealed class GeographyController : ControllerBase
{
    private const string CorrelationHeader = "X-Correlation-Id";

    private readonly IRegionService _regionService;
    private readonly IContinentService _continentService;
    private readonly ICountryService _countryService;
    private readonly ILoggerService _log;

    public GeographyController(
        IRegionService regionService,
        IContinentService continentService,
        ICountryService countryService,
        ILoggerService log)
    {
        _regionService = regionService;
        _continentService = continentService;
        _countryService = countryService;
        _log = log;
    }

    // ------------------------------------------------------------------------
    // TRAVEL POLICIES
    // ------------------------------------------------------------------------

    /// <summary>
    /// List travel policies. If organizationId is omitted, returns all (admin scenario).
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(typeof(GeographyUnifiedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GeographyUnifiedDto.GeographyAggregate>> GetGeographyUnifiedDto(
        [FromQuery] string? rid,
        CancellationToken ct)
    {
        StampCorrelationId();

        var sw = Stopwatch.StartNew();
        try
        {
            var regions = await _regionService.GetAllAsync(ct);
            var continents = await _continentService.GetAllAsync(ct);
            var countries = await _countryService.GetAllAsync(ct);

            await _log.InformationAsync(
                evt: SysLogEvtType.DATA_READ,
                cat: SysLogCatType.Api,
                act: SysLogActionType.Read,
                message: "Successfully retrieved geography aggregate",
                ent: nameof(GeographyUnifiedDto),
                rid: rid ?? GetCorrelationId(),
                tid: Guid.Parse(GetTraceId()),
                uid: null,
                org: null,
                durMs: (int)sw.ElapsedMilliseconds,
                http: Request.Method,
                stat: StatusCodes.Status200OK);

            return Ok(new GeographyUnifiedDto.GeographyAggregate(
                regions,
                continents,
                countries,
                new CommandMetadata
                {
                    RequestId = rid ?? GetCorrelationId(),
                    TransactionId = Guid.Parse(GetTraceId()),
                    UserId = null,
                    OrganizationId = null
                }
            ));
        }
        catch (ArgumentException ex)
        {
            await _log.WarningAsync(
                evt: SysLogEvtType.DATA_READ_ERR,
                cat: SysLogCatType.Api,
                act: SysLogActionType.Read,
                message: "Bad request in GetGeographyUnifiedDto",
                ex: ex,
                ent: nameof(GeographyUnifiedDto),
                rid: GetCorrelationId(),
                tid: Guid.Parse(GetTraceId()),
                uid: null,
                org: null,
                durMs: (int)sw.ElapsedMilliseconds,
                http: Request.Method,
                stat: StatusCodes.Status400BadRequest);

            return ProblemEx(StatusCodes.Status400BadRequest, "bad_request", ex.Message);
        }
        finally
        {
            sw.Stop();
        }
    }


    // ------------------------------------------------------------------------
    // Enterprise-ish response helpers
    // ------------------------------------------------------------------------

    private void StampCorrelationId()
    {
        var corr = GetCorrelationId();

        if (!Response.Headers.ContainsKey(CorrelationHeader))
            Response.Headers[CorrelationHeader] = corr;

        Response.Headers["X-Trace-Id"] = GetTraceId();
        Response.Headers["Cache-Control"] = "no-store";
    }

    private string GetCorrelationId()
    {
        var incoming = Request.Headers[CorrelationHeader].ToString();
        return string.IsNullOrWhiteSpace(incoming) ? HttpContext.TraceIdentifier : incoming;
    }

    private static string GetTraceId()
        => $"{Guid.NewGuid():N}";

    private ObjectResult ProblemEx(int status, string errorCode, string title, string? detail = null)
    {
        var pd = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = HttpContext?.Request?.Path.Value
        };

        pd.Extensions["errorCode"] = errorCode;
        pd.Extensions["correlationId"] = GetCorrelationId();
        pd.Extensions["traceId"] = GetTraceId();

        return StatusCode(status, pd);
    }
}
