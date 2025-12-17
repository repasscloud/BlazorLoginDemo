using Cinturon360.Shared.Data;
using Cinturon360.Shared.Models.Kernel.SysVar;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinturon360.Api.Controllers.Admin;

[ApiController]
[Route("v1/cron")]
public sealed class CronController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public CronController(ApplicationDbContext db)
    {
        _db = db;
    }

    // ------ Expire ------
    [HttpGet("expire/hourly")]
    [ProducesResponseType(typeof(ExpireJobsResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExpireHourlyJobsAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var cutoff = now.AddHours(-1);

        var affected = await _db.QueuedJobs
            .Where(j => HourlyJobTypesToExpire.Contains(j.JobType))
            .Where(j => HourlyJobStatusesToExpire.Contains(j.Status))
            .Where(j => j.CreatedUtc < cutoff)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(j => j.Status, JobStatus.Expired)
                .SetProperty(j => j.AttemptCount, -1)
                .SetProperty(j => j.CompletedUtc, now),
                ct);

        return Ok(new ExpireJobsResultDto
        {
            Expired = affected,
            CutoffUtc = cutoff,
            NowUtc = now
        });
    }

    // ------ DTOs ------
    public sealed class ExpireJobsResultDto
    {
        public int Expired { get; init; }
        public DateTime CutoffUtc { get; init; }
        public DateTime NowUtc { get; init; }
    }

    // ------ Static Data ------
    private static readonly string[] HourlyJobTypesToExpire =
    [
        "FlightSearch"
    ];

    private static readonly JobStatus[] HourlyJobStatusesToExpire =
    {
        JobStatus.Unknown,
        JobStatus.Pending,
        JobStatus.Dequeued,
        JobStatus.Leased,
        JobStatus.Retrieved,
        JobStatus.Heartbeating,
        JobStatus.Processing,
        JobStatus.Validating,
        JobStatus.Enriching,
        JobStatus.Executing,
        JobStatus.Finalizing,
        JobStatus.WaitingExternal,
        JobStatus.WaitingRateLimit,
        JobStatus.WaitingRetryBackoff,
        JobStatus.WaitingManual,
        JobStatus.Failed,
        JobStatus.FailedValidation,
        JobStatus.FailedExternal,
        JobStatus.FailedTimeout,
        JobStatus.FailedConflict,
        JobStatus.FailedSecurity,
        JobStatus.CancelRequested,
        JobStatus.Cancelling,
        JobStatus.Cancelled,
        JobStatus.Superseded,
        JobStatus.Reserved90,
        JobStatus.Reserved91,
        JobStatus.Reserved92,
        JobStatus.Reserved93,
        JobStatus.Reserved94,
        JobStatus.Reserved95,
        JobStatus.Reserved96,
        JobStatus.Reserved97,
        JobStatus.Reserved98,
        JobStatus.Reserved99
    };
}