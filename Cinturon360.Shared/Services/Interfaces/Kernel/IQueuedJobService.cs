using Cinturon360.Shared.Models.Kernel.SysVar;

namespace Cinturon360.Shared.Services.Interfaces.Kernel;

public interface IQueuedJobService
{
    /// <summary>
    /// Enqueue a new job with the given payload.
    /// </summary>
    Task<QueuedJob> EnqueueAsync<TPayload>(
        TPayload payload,
        string? jobType = null,
        string? correlationId = null,
        DateTimeOffset? availableAfterUtc = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a batch of pending jobs that are due to run.
    /// </summary>
    Task<IReadOnlyList<QueuedJob>> GetPendingJobsAsync(
        string? jobType = null,
        int maxCount = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deserialize the JSON payload of a job into a typed object.
    /// </summary>
    TPayload? DeserializePayload<TPayload>(QueuedJob job);

    /// <summary>
    /// Mark a job as "Processing" and bump AttemptCount.
    /// </summary>
    Task MarkAsProcessingAsync(
        QueuedJob job,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark a job as "Succeeded".
    /// </summary>
    Task MarkAsSucceededAsync(
        QueuedJob job,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark a job as "Failed" and store an error message.
    /// </summary>
    Task MarkAsFailedAsync(
        QueuedJob job,
        string errorMessage,
        CancellationToken cancellationToken = default);
}
