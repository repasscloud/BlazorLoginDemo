namespace Cinturon360.Shared.Models.Kernel.SysVar;

public enum JobStatus
{
    Pending = 0,
    Processing = 1,
    Succeeded = 2,
    Failed = 3
}

public sealed class QueuedJob
{
    // Primary key (DB identity or GUID, your choice)
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Logical type key so you know what this payload is.
    /// Examples: "FlightSearch", "ReportRun", "EmailSend"
    /// </summary>
    public string JobType { get; init; } = default!;

    /// <summary>
    /// JSON representation of the job payload.
    /// </summary>
    public string PayloadJson { get; init; } = default!;

    /// <summary>
    /// When the job was created/queued (UTC).
    /// </summary>
    public DateTimeOffset CreatedUtc { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Optional: do not process before this time (for scheduled/delayed jobs).
    /// </summary>
    public DateTimeOffset? AvailableAfterUtc { get; init; }

    /// <summary>
    /// Current status of the job.
    /// </summary>
    public JobStatus Status { get; set; } = JobStatus.Pending;

    /// <summary>
    /// When the job was picked up for processing (UTC).
    /// </summary>
    public DateTimeOffset? StartedUtc { get; set; }

    /// <summary>
    /// When the job finished processing (UTC).
    /// </summary>
    public DateTimeOffset? CompletedUtc { get; set; }

    /// <summary>
    /// How many times processing has been attempted.
    /// </summary>
    public int AttemptCount { get; set; } = 0;

    /// <summary>
    /// Optional correlation id (e.g., TravelQuoteId, request id, etc.).
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Optional: last error message if the job failed.
    /// </summary>
    public string? LastError { get; set; }
}
