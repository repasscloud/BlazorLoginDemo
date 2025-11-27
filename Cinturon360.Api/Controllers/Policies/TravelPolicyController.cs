using System.Diagnostics;
using Cinturon360.Shared.Models.Policies;
using Cinturon360.Shared.Services.Interfaces.Policy;
using Microsoft.AspNetCore.Mvc;

namespace Cinturon360.Api.Controllers;

[ApiController]
[Produces("application/json")]
[Route("v1/policies")]
public sealed class PoliciesController : ControllerBase
{
    private const string CorrelationHeader = "X-Correlation-Id";

    private readonly ITravelPolicyService _travelPolicies;
    private readonly ILogger<PoliciesController> _log;

    public PoliciesController(ITravelPolicyService travelPolicies, ILogger<PoliciesController> log)
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

        try
        {
            if (string.IsNullOrWhiteSpace(organizationId))
                return Ok(await _travelPolicies.GetAllAsync(ct));

            return Ok(await _travelPolicies.GetForOrganizationAsync(organizationId, ct));
        }
        catch (ArgumentException ex)
        {
            _log.LogWarning(ex, "Bad request in ListTravelPolicies");
            return ProblemEx(StatusCodes.Status400BadRequest, "bad_request", ex.Message);
        }
    }

    /// <summary>
    /// Get a travel policy by id.
    /// </summary>
    [HttpGet("travel/{policyId}")]
    [ProducesResponseType(typeof(TravelPolicy), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TravelPolicy>> GetTravelPolicyById(
        [FromRoute] string policyId,
        CancellationToken ct)
    {
        StampCorrelationId();

        if (string.IsNullOrWhiteSpace(policyId))
            return ProblemEx(StatusCodes.Status400BadRequest, "validation_failed", "policyId is required.");

        var policy = await _travelPolicies.GetByIdAsync(policyId, ct);
        if (policy is null)
            return ProblemEx(StatusCodes.Status404NotFound, "travel_policy_not_found", $"Travel policy '{policyId}' not found.");

        return Ok(policy);
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

        try
        {
            // Optional: support idempotency keys later (store and replay by key).
            // var idem = Request.Headers["Idempotency-Key"].ToString();

            await _travelPolicies.CreateAsync(policy, ct);

            // Re-read to return the persisted/normalized version.
            var created = await _travelPolicies.GetByIdAsync(policy.Id, ct);
            if (created is null)
            {
                // Should be rare; treat as server error.
                return ProblemEx(StatusCodes.Status500InternalServerError, "create_failed", "Policy was created but could not be retrieved.");
            }

            return CreatedAtAction(
                nameof(GetTravelPolicyById),
                routeValues: new { policyId = created.Id },
                value: created);
        }
        catch (ArgumentException ex)
        {
            _log.LogWarning(ex, "Validation failure in CreateTravelPolicy");
            return ProblemEx(StatusCodes.Status400BadRequest, "validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            // Your service throws InvalidOperationException when organization or policy doesn't exist.
            // Map to 404 for "not found".
            _log.LogWarning(ex, "Not found / invalid operation in CreateTravelPolicy");
            return ProblemEx(StatusCodes.Status404NotFound, "organization_not_found", ex.Message);
        }
    }

    /// <summary>
    /// Replace/update a travel policy.
    /// </summary>
    [HttpPut("travel/{policyId}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TravelPolicy), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TravelPolicy>> UpdateTravelPolicy(
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
            _log.LogWarning(ex, "Validation failure in UpdateTravelPolicy");
            return ProblemEx(StatusCodes.Status400BadRequest, "validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _log.LogWarning(ex, "Not found / invalid operation in UpdateTravelPolicy");
            return ProblemEx(StatusCodes.Status404NotFound, "organization_not_found", ex.Message);
        }
    }

    /// <summary>
    /// Delete a travel policy.
    /// </summary>
    [HttpDelete("travel/{policyId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTravelPolicy(
        [FromRoute] string policyId,
        CancellationToken ct)
    {
        StampCorrelationId();

        if (string.IsNullOrWhiteSpace(policyId))
            return ProblemEx(StatusCodes.Status400BadRequest, "validation_failed", "policyId is required.");

        var ok = await _travelPolicies.DeleteAsync(policyId, ct);
        if (!ok)
            return ProblemEx(StatusCodes.Status404NotFound, "travel_policy_not_found", $"Travel policy '{policyId}' not found.");

        // Note: your current service delete does not clear/update an org's DefaultTravelPolicyId.
        // That fix belongs in the service layer (or DB constraint logic), not here.
        return NoContent();
    }

    /// <summary>
    /// Set a travel policy as the default for its organization.
    /// </summary>
    [HttpPost("travel/{policyId}:set-default")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetDefaultTravelPolicy(
        [FromRoute] string policyId,
        CancellationToken ct)
    {
        StampCorrelationId();

        if (string.IsNullOrWhiteSpace(policyId))
            return ProblemEx(StatusCodes.Status400BadRequest, "validation_failed", "policyId is required.");

        var ok = await _travelPolicies.SetPolicyAsDefaultAsync(policyId, ct);
        if (!ok)
            return ProblemEx(StatusCodes.Status404NotFound, "travel_policy_not_found", $"Travel policy '{policyId}' not found (or org missing).");

        return NoContent();
    }

    /// <summary>
    /// Resolve the effective allowed countries for a travel policy
    /// (regions/continents/countries minus disabled -> countries).
    /// </summary>
    [HttpGet("travel/{policyId}/allowed-countries")]
    [ProducesResponseType(typeof(IReadOnlyList<Country>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<Country>>> ResolveAllowedCountries(
        [FromRoute] string policyId,
        CancellationToken ct)
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
            // Service throws when TravelPolicy not found.
            return ProblemEx(StatusCodes.Status404NotFound, "travel_policy_not_found", ex.Message);
        }
    }

    // ------------------------------------------------------------------------
    // Enterprise-ish response helpers
    // ------------------------------------------------------------------------

    private void StampCorrelationId()
    {
        var corr = GetCorrelationId();

        // Always echo correlation id back for tracing.
        if (!Response.Headers.ContainsKey(CorrelationHeader))
            Response.Headers[CorrelationHeader] = corr;

        // Optional extras that are useful in enterprises.
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
