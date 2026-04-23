using System.Text.Json;
using Cinturon360.Shared.Data;
using Cinturon360.Shared.Models.Kernel.SysVar;
using Cinturon360.Shared.Services.Interfaces.Kernel;
using Microsoft.EntityFrameworkCore;

namespace Cinturon360.Shared.Services.Kernel;

public sealed class QueuedJobService : IQueuedJobService
{
    private readonly ApplicationDbContext _db;

    // Local JSON options, only used by this service.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public QueuedJobService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<QueuedJob> EnqueueAsync<TPayload>(
        TPayload payload,
        string? jobType = null,
        string? correlationId = null,
        DateTimeOffset? availableAfterUtc = null,
        CancellationToken cancellationToken = default)
    {
        if (payload is null)
            throw new ArgumentNullException(nameof(payload));

        var json = JsonSerializer.Serialize(payload, JsonOptions);

        var job = new QueuedJob
        {
            JobType = jobType ?? typeof(TPayload).Name,
            PayloadJson = json,
            CorrelationId = correlationId,
            AvailableAfterUtc = availableAfterUtc,
            CreatedUtc = DateTimeOffset.UtcNow,
            Status = JobStatus.Pending,
            AttemptCount = 0
        };

        await _db.QueuedJobs.AddAsync(job, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return job;
    }

    public async Task<IReadOnlyList<QueuedJob>> GetPendingJobsAsync(
        string? jobType = null,
        int maxCount = 100,
        CancellationToken cancellationToken = default)
    {
        if (maxCount <= 0)
            maxCount = 100;

        var now = DateTimeOffset.UtcNow;

        IQueryable<QueuedJob> query = _db.QueuedJobs
            .Where(j => j.Status == JobStatus.Pending &&
                        (j.AvailableAfterUtc == null || j.AvailableAfterUtc <= now))
            .OrderBy(j => j.CreatedUtc);

        if (!string.IsNullOrWhiteSpace(jobType))
        {
            query = query.Where(j => j.JobType == jobType);
        }

        return await query
            .Take(maxCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<QueuedJob?> GetJobByCorrelationIdAsync(
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
            return null;

        var job = await _db.QueuedJobs
            .FirstOrDefaultAsync(j => j.CorrelationId == correlationId, cancellationToken);

        if (job is null)
            return null;

        return job;
    }

    // used explicitly for checking jobs that are available to be processed for travel booking only
    public async Task<QueuedJob?> GetJobByCorrelationIdAndNotCompletedAsync(
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
            return null;

        var job = await _db.QueuedJobs
            .FirstOrDefaultAsync(
                j => j.CorrelationId == correlationId &&
                    !TravelBookingExcludedStatuses.Contains(j.Status),
                cancellationToken);

        if (job is null)
            return null;

        job.Status = JobStatus.Retrieved;
        job.StartedUtc = DateTime.UtcNow;
        job.AttemptCount += 1; // NOT: job.AttemptCount = job.AttemptCount++;

        await _db.SaveChangesAsync(cancellationToken);

        _db.Entry(job).State = EntityState.Detached;
        return job;
    }

    public TPayload? DeserializePayload<TPayload>(QueuedJob job)
    {
        if (job is null) throw new ArgumentNullException(nameof(job));

        return JsonSerializer.Deserialize<TPayload>(job.PayloadJson, JsonOptions);
    }

    public async Task MarkAsProcessingAsync(
        QueuedJob job,
        CancellationToken cancellationToken = default)
    {
        if (job is null) throw new ArgumentNullException(nameof(job));

        job.Status = JobStatus.Processing;
        job.StartedUtc = DateTimeOffset.UtcNow;
        job.AttemptCount++;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAsSucceededAsync(
        QueuedJob job,
        CancellationToken cancellationToken = default)
    {
        if (job is null) throw new ArgumentNullException(nameof(job));

        job.Status = JobStatus.Succeeded;
        job.CompletedUtc = DateTimeOffset.UtcNow;
        job.LastError = null;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAsFailedAsync(
        QueuedJob job,
        string errorMessage,
        CancellationToken cancellationToken = default)
    {
        if (job is null) throw new ArgumentNullException(nameof(job));

        job.Status = JobStatus.Failed;
        job.CompletedUtc = DateTimeOffset.UtcNow;
        job.LastError = errorMessage;

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static readonly JobStatus[] TravelBookingExcludedStatuses =
    {
        JobStatus.Blocked,
        JobStatus.Paused,
        JobStatus.Dequeued,
        JobStatus.Leased,
        JobStatus.Heartbeating,

        JobStatus.Processing,
        JobStatus.Validating,
        JobStatus.Enriching,
        JobStatus.Executing,
        JobStatus.Persisting,
        JobStatus.Publishing,
        JobStatus.Finalizing,

        JobStatus.WaitingExternal,
        JobStatus.WaitingRateLimit,
        JobStatus.WaitingRetryBackoff,
        JobStatus.WaitingManual,

        JobStatus.Succeeded,
        JobStatus.SucceededWithWarnings,
        JobStatus.Completed,

        JobStatus.Failed,
        JobStatus.FailedValidation,
        JobStatus.FailedExternal,
        JobStatus.FailedTimeout,
        JobStatus.FailedConflict,
        JobStatus.FailedSecurity,

        JobStatus.Retrying,
        JobStatus.DeadLettered,
        JobStatus.Quarantined,

        JobStatus.CancelRequested,
        JobStatus.Cancelling,
        JobStatus.Cancelled,
        JobStatus.Superseded,

        JobStatus.Expired,
        JobStatus.Abandoned,
        JobStatus.Stale,

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
