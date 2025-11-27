using Cinturon360.Shared.Services.Interfaces.Kernel;
using Microsoft.AspNetCore.Mvc;

namespace Cinturon360.Api.Controllers.Queue;

[Route("api/v1/queue")]
[ApiController]
public class QueueController : ControllerBase
{
    private readonly IQueuedJobService _queuedJobService;
    public QueueController(IQueuedJobService queuedJobService)
    {
        _queuedJobService = queuedJobService;
    }

    [HttpGet("correlationid/{correlationId}")]
    public async Task<IActionResult> GetByCorrelationIdAsync(string correlationId, CancellationToken ct)
    {
        return Ok(await _queuedJobService.GetJobByCorrelationIdAsync(correlationId, ct));
    }

    [HttpGet("correlationid/{correlationId}/not-completed")]
    public async Task<IActionResult> GetNotCompletedByCorrelationIdAsync(string correlationId, CancellationToken ct)
    {
        return Ok(await _queuedJobService.GetJobByCorrelationIdAndNotCompletedAsync(correlationId, ct));
    }
}