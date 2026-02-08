using System.Diagnostics;
using System.Security.Claims;
using Cinturon360.Shared.Models.Policies;
using Cinturon360.Shared.Models.Static.Geography;
using Cinturon360.Shared.Models.Static.System.SysVar;
using Cinturon360.Shared.Services.Interfaces.Kernel;
using Cinturon360.Shared.Services.Interfaces.Policy;
using Microsoft.AspNetCore.Mvc;
using static Cinturon360.Shared.Contracts.Policies.TravelPolicyUnifiedDto;

namespace Cinturon360.Api.Controllers;

[ApiController]
[Produces("application/json")]
[Route("v1/policies")]
public sealed class PoliciesController : ControllerBase
{
    private const string CorrelationHeader = "X-Correlation-Id";

    private readonly ITravelPolicyService _travelPolicies;
    private readonly ILoggerService _log;

    public PoliciesController(ITravelPolicyService travelPolicies, ILoggerService log)
    {
        _travelPolicies = travelPolicies;
        _log = log;
    }

    // ------------------------------------------------------------------------
    // TRAVEL POLICIES
    // ------------------------------------------------------------------------

    /// <summary>
    /// List travel policies. If organizationId is omitted, returns all (admin scenario).
    /// </summary>
    [HttpGet("travel")]
    [ProducesResponseType(typeof(IReadOnlyList<TravelPolicy>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<TravelPolicy>>> ListTravelPolicies(
        [FromQuery] string? organizationId,
        CancellationToken ct)
    {
        StampCorrelationId();

        var sw = Stopwatch.StartNew();
        try
        {
            if (string.IsNullOrWhiteSpace(organizationId))
                return Ok(await _travelPolicies.GetAllAsync(ct));

            return Ok(await _travelPolicies.GetForOrganizationAsync(organizationId, ct));
        }
        catch (ArgumentException ex)
        {
            await _log.WarningAsync(
                evt: SysLogEvtType.DATA_READ_ERR,
                cat: SysLogCatType.Data,
                act: SysLogActionType.Read,
                message: "Bad request in ListTravelPolicies",
                ex: ex,
                ent: nameof(TravelPolicy),
                rid: GetCorrelationId(),
                tid: GetTraceId(),
                uid: GetUserId(),
                org: organizationId,
                durMs: (int)sw.ElapsedMilliseconds,
                http: Request.Method,
                stat: StatusCodes.Status400BadRequest,
                path: HttpContext?.Request?.Path.Value,
                note: "errorCode=bad_request");

            return ProblemEx(StatusCodes.Status400BadRequest, "bad_request", ex.Message);
        }
    }

    /// <summary>
    /// Create a travel policy.
    /// Note: your service will set the org default travel policy if one isn't set yet.
    /// </summary>
    [HttpPost("travel")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TravelPolicy), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TravelPolicy>> CreateTravelPolicy(
        [FromBody] TravelPolicy policy,
        CancellationToken ct)
    {
        StampCorrelationId();

        if (policy is null)
            return ProblemEx(StatusCodes.Status400BadRequest, "validation_failed", "Body is required.");

        var sw = Stopwatch.StartNew();

        try
        {
            await _travelPolicies.CreateAsync(policy, ct);

            // Re-read to return the persisted/normalized version.
            var created = await _travelPolicies.GetByIdAsync(policy.Id, ct);
            if (created is null)
            {
                await _log.ErrorAsync(
                    evt: SysLogEvtType.DATA_CREATE_ERR,
                    cat: SysLogCatType.Data,
                    act: SysLogActionType.Create,
                    ex: new InvalidOperationException("Policy was created but could not be retrieved."),
                    message: "CreateTravelPolicy created a record but could not retrieve it",
                    ent: nameof(TravelPolicy),
                    entId: policy.Id,
                    rid: GetCorrelationId(),
                    tid: GetTraceId(),
                    uid: GetUserId(),
                    org: policy.OrganizationUnifiedId,
                    durMs: (int)sw.ElapsedMilliseconds,
                    http: Request.Method,
                    stat: StatusCodes.Status500InternalServerError,
                    path: HttpContext?.Request?.Path.Value,
                    note: "errorCode=create_failed");

                return ProblemEx(StatusCodes.Status500InternalServerError, "create_failed",
                    "Policy was created but could not be retrieved.");
            }

            return CreatedAtAction(
                nameof(GetTravelPolicyById),
                routeValues: new { policyId = created.Id },
                value: created);
        }
        catch (ArgumentException ex)
        {
            await _log.WarningAsync(
                evt: SysLogEvtType.DATA_CREATE_ERR,
                cat: SysLogCatType.Data,
                act: SysLogActionType.Create,
                message: "Validation failure in CreateTravelPolicy",
                ex: ex,
                ent: nameof(TravelPolicy),
                entId: policy.Id,
                rid: GetCorrelationId(),
                tid: GetTraceId(),
                uid: GetUserId(),
                org: policy.OrganizationUnifiedId,
                durMs: (int)sw.ElapsedMilliseconds,
                http: Request.Method,
                stat: StatusCodes.Status400BadRequest,
                path: HttpContext?.Request?.Path.Value,
                note: "errorCode=validation_failed");

            return ProblemEx(StatusCodes.Status400BadRequest, "validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            // Your service throws InvalidOperationException when organization or policy doesn't exist.
            // Map to 404 for "not found".
            await _log.WarningAsync(
                evt: SysLogEvtType.DATA_CREATE_ERR,
                cat: SysLogCatType.Data,
                act: SysLogActionType.Create,
                message: "Not found / invalid operation in CreateTravelPolicy",
                ex: ex,
                ent: nameof(TravelPolicy),
                entId: policy.Id,
                rid: GetCorrelationId(),
                tid: GetTraceId(),
                uid: GetUserId(),
                org: policy.OrganizationUnifiedId,
                durMs: (int)sw.ElapsedMilliseconds,
                http: Request.Method,
                stat: StatusCodes.Status404NotFound,
                path: HttpContext?.Request?.Path.Value,
                note: "errorCode=organization_not_found");

            return ProblemEx(StatusCodes.Status404NotFound, "organization_not_found", ex.Message);
        }
    }

    /// <summary>
    /// Replace/update a travel policy.
    /// </summary>
    [HttpPut("travel/{policyId}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TravelPolicyNoResponseAggregate), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TravelPolicyNoResponseAggregate>> UpdateTravelPolicyAsync(
        [FromRoute] string policyId,
        [FromBody] TravelPolicy policy,
        CancellationToken ct)
    {
        StampCorrelationId();

        if (policy is null)
            return ProblemEx(StatusCodes.Status400BadRequest, "validation_failed", "Body is required.");

        if (string.IsNullOrWhiteSpace(policyId))
            return ProblemEx(StatusCodes.Status400BadRequest, "validation_failed", "policyId is required.");

        // Enforce route/body consistency.
        policy.Id = policyId;

        // Ensure correct 404 instead of a blind EF update.
        if (!await _travelPolicies.ExistsAsync(policyId, ct))
            return ProblemEx(StatusCodes.Status404NotFound, "travel_policy_not_found", $"Travel policy '{policyId}' not found.");

        var sw = Stopwatch.StartNew();

        try
        {
            await _travelPolicies.UpdateAsync(policy, ct);

            var updated = await _travelPolicies.GetByIdAsync(policyId, ct);
            if (updated is null)
                return ProblemEx(StatusCodes.Status500InternalServerError, "update_failed", "Policy was updated but could not be retrieved.");

            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            await _log.WarningAsync(
                evt: SysLogEvtType.DATA_UPDATE_ERR,
                cat: SysLogCatType.Data,
                act: SysLogActionType.Update,
                message: "Validation failure in UpdateTravelPolicy",
                ex: ex,
                ent: nameof(TravelPolicy),
                entId: policyId,
                rid: GetCorrelationId(),
                tid: GetTraceId(),
                uid: GetUserId(),
                org: policy.OrganizationUnifiedId,
                durMs: (int)sw.ElapsedMilliseconds,
                http: Request.Method,
                stat: StatusCodes.Status400BadRequest,
                path: HttpContext?.Request?.Path.Value,
                note: "errorCode=validation_failed");

            return ProblemEx(StatusCodes.Status400BadRequest, "validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await _log.WarningAsync(
                evt: SysLogEvtType.DATA_UPDATE_ERR,
                cat: SysLogCatType.Data,
                act: SysLogActionType.Update,
                message: "Not found / invalid operation in UpdateTravelPolicy",
                ex: ex,
                ent: nameof(TravelPolicy),
                entId: policyId,
                rid: GetCorrelationId(),
                tid: GetTraceId(),
                uid: GetUserId(),
                org: policy.OrganizationUnifiedId,
                durMs: (int)sw.ElapsedMilliseconds,
                http: Request.Method,
                stat: StatusCodes.Status404NotFound,
                path: HttpContext?.Request?.Path.Value,
                note: "errorCode=organization_not_found");

            return ProblemEx(StatusCodes.Status404NotFound, "organization_not_found", ex.Message);
        }
    }

    // ------------------------------------------------------------------------
    // Remaining endpoints unchanged (no ILogger<T> usage below)
    // ------------------------------------------------------------------------

    [HttpGet("travel/{policyId}")]
    [ProducesResponseType(typeof(TravelPolicy), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TravelPolicy>> GetTravelPolicyById([FromRoute] string policyId, CancellationToken ct)
    {
        StampCorrelationId();

        if (string.IsNullOrWhiteSpace(policyId))
            return ProblemEx(StatusCodes.Status400BadRequest, "validation_failed", "policyId is required.");

        var policy = await _travelPolicies.GetByIdAsync(policyId, ct);
        if (policy is null)
            return ProblemEx(StatusCodes.Status404NotFound, "travel_policy_not_found", $"Travel policy '{policyId}' not found.");

        return Ok(policy);
    }

    [HttpDelete("travel/{policyId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTravelPolicy([FromRoute] string policyId, CancellationToken ct)
    {
        StampCorrelationId();

        if (string.IsNullOrWhiteSpace(policyId))
            return ProblemEx(StatusCodes.Status400BadRequest, "validation_failed", "policyId is required.");

        var ok = await _travelPolicies.DeleteAsync(policyId, ct);
        if (!ok)
            return ProblemEx(StatusCodes.Status404NotFound, "travel_policy_not_found", $"Travel policy '{policyId}' not found.");

        return NoContent();
    }

    [HttpPost("travel/{policyId}:set-default")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetDefaultTravelPolicy([FromRoute] string policyId, CancellationToken ct)
    {
        StampCorrelationId();

        if (string.IsNullOrWhiteSpace(policyId))
            return ProblemEx(StatusCodes.Status400BadRequest, "validation_failed", "policyId is required.");

        var ok = await _travelPolicies.SetPolicyAsDefaultAsync(policyId, ct);
        if (!ok)
            return ProblemEx(StatusCodes.Status404NotFound, "travel_policy_not_found", $"Travel policy '{policyId}' not found (or org missing).");

        return NoContent();
    }

    [HttpGet("travel/{policyId}/allowed-countries")]
    [ProducesResponseType(typeof(IReadOnlyList<PassportCountry>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<PassportCountry>>> ResolveAllowedCountries([FromRoute] string policyId, CancellationToken ct)
    {
        StampCorrelationId();

        if (string.IsNullOrWhiteSpace(policyId))
            return ProblemEx(StatusCodes.Status400BadRequest, "validation_failed", "policyId is required.");

        try
        {
            var countries = await _travelPolicies.ResolveAllowedCountriesAsync(policyId, ct);
            return Ok(countries);
        }
        catch (InvalidOperationException ex)
        {
            return ProblemEx(StatusCodes.Status404NotFound, "travel_policy_not_found", ex.Message);
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

        Response.Headers["X-Trace-Id"] = GetTraceId().ToString();
        Response.Headers["Cache-Control"] = "no-store";
    }

    private string GetCorrelationId()
    {
        var incoming = Request.Headers[CorrelationHeader].ToString();
        return string.IsNullOrWhiteSpace(incoming) ? HttpContext.TraceIdentifier : incoming;
    }

    private static Guid GetTraceId()
        => Guid.NewGuid();
        //=> Activity.Current?.Id ?? string.Empty;

    private string? GetUserId()
        => User?.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User?.FindFirstValue("sub")
           ?? User?.FindFirstValue("uid");

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
