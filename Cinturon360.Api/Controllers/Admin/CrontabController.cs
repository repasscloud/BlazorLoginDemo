using System.Text.Json;
using Cinturon360.Shared.Models.DTOs;
using Cinturon360.Shared.Services.Interfaces.Kernel;
using Cinturon360.Shared.Services.Interfaces.Travel;
using Microsoft.AspNetCore.Mvc;

namespace Cinturon360.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/crontab")]
public sealed class CrontabController : ControllerBase
{
    private readonly IQueuedJobService _queuedJobService;
    private readonly ITravelQuoteService _travelQuoteService;

    public CrontabController(IQueuedJobService queuedJobService, ITravelQuoteService travelQuoteService)
    {
        _queuedJobService = queuedJobService;
        _travelQuoteService = travelQuoteService;
    }

    // ------ Exec ------
    [HttpGet("exec/{jobType}")]
    public async Task<IActionResult> ExecAsync(string jobType, CancellationToken ct)
    {
        var result = await _queuedJobService.GetPendingJobsAsync(jobType, 100, ct);

        if (result.Count == 0)
        {
            return Ok("No pending jobs found.");
        };

        switch (jobType)
        {
            case "FlightSearch":
                foreach (var job in result)
                {
                    var dto = JsonSerializer.Deserialize<TravelQuoteFlightUIResultPatchDto>(job.PayloadJson);
                    if (dto != null)
                    {
                        await _travelQuoteService.IngestTravelQuoteFlightUIResultPatchDto(dto, ct);
                    }
                }
                break;
            default:
                return BadRequest("Unknown job type.");
        }

        return Ok($"Processed {result.Count} jobs of type {jobType}.");
    }
}
