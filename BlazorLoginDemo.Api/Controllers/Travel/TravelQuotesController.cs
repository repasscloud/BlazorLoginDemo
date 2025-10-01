using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using BlazorLoginDemo.Shared.Security;
using BlazorLoginDemo.Shared.Models.Kernel.Travel;
using BlazorLoginDemo.Shared.Services.Interfaces.Travel;

namespace BlazorLoginDemo.Api.Controllers.Travel;

[Route("api/v1/travel/quotes")]
[ServiceFilter(typeof(RequireApiKeyFilter))]
[ApiController]
public sealed class TravelQuotesController : ControllerBase
{
    private readonly ITravelQuoteService _svc;

    public TravelQuotesController(ITravelQuoteService svc) => _svc = svc;

    // ---------- READ (NO TRACKING via service) ----------
    [HttpGet("{id}")]
    public async Task<ActionResult<TravelQuote>> GetById(string id, CancellationToken ct)
    {
        var q = await _svc.GetByIdAsync(id, ct);
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
        var result = await _svc.SearchAsync(organizationId, createdByUserId, tmcAssignedId, type, state, ct);
        return Ok(result);
    }

    // ---------- CREATE ----------
    [HttpPost]
    public async Task<ActionResult<TravelQuote>> Create([FromBody] TravelQuote input, CancellationToken ct)
    {
        var created = await _svc.CreateAsync(input, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // ---------- UPDATE (PUT overwrites entire aggregate) ----------
    [HttpPut("{id}")]
    public async Task<ActionResult<TravelQuote>> Put(string id, [FromBody] TravelQuote input, CancellationToken ct)
    {
        if (!string.Equals(id, input.Id, StringComparison.Ordinal))
            return BadRequest("Id in route and body must match.");

        var updated = await _svc.UpdatePutAsync(input, ct);
        return Ok(updated);
    }

    // ---------- DELETE ----------
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
        => (await _svc.DeleteAsync(id, ct)) ? NoContent() : NotFound();

    // ---------- TARGETED UPDATES ----------
    [HttpPatch("{id}/created-by")]
    public async Task<IActionResult> UpdateCreatedBy(string id, [FromBody] UpdateCreatedByRequest body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.NewUserId)) return BadRequest("NewUserId is required.");
        return (await _svc.ReassignCreatedByAsync(id, body.NewUserId, ct)) ? NoContent() : NotFound();
    }

    [HttpPatch("{id}/state")]
    public async Task<IActionResult> UpdateState(string id, [FromBody] UpdateStateRequest body, CancellationToken ct)
        => (await _svc.UpdateStateAsync(id, body.State, ct)) ? NoContent() : NotFound();

    // ---------- DTO → ENTITY → RETURN Id ----------
    [HttpPost("from-dto")]
    public async Task<ActionResult<object>> CreateFromDto(
        [FromBody] TravelQuoteDto dto,
        CancellationToken ct)
    {
        if (dto is null) return BadRequest("Body required.");
        var (ok, error, travelQuoteId) = await _svc.CreateFromDtoAsync(dto, ct);
        if (!ok || string.IsNullOrWhiteSpace(travelQuoteId))
            return BadRequest(new { error });

        return Ok(new { id = travelQuoteId });
    }

    // ---------- Request contracts ----------
    public sealed record UpdateCreatedByRequest([property: Required] string NewUserId);
    public sealed record UpdateStateRequest([property: Required] QuoteState State);
    // remove: public sealed record CreateFromDtoRequest([property: Required] TravelQuoteDto Dto);
}
