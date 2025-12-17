namespace Cinturon360.Shared.Models.Kernel.SysVar;

public enum JobStatus
{
    // 0–9: Not started / waiting
    Unknown             = 0,  // default / not set / legacy
    Pending             = 1,  // accepted into queue, not yet leased
    Scheduled           = 2,  // due later (delayed execution / cron)
    Blocked             = 3,  // waiting on prerequisite (dependency / external lock)
    Paused              = 4,  // administratively paused (system-wide or per-job)

    // 10–19: Lease / ownership (who has it)
    Dequeued            = 10, // removed from queue for work (about to be leased/locked)
    Leased              = 11, // worker has a lease/lock (visibility timeout active)
    Retrieved           = 12, // worker fetched payload (kept from your original)
    Heartbeating        = 13, // actively extending lease / reporting liveness

    // 20–29: Active work
    Processing          = 20, // main execution in progress
    Validating          = 21, // validating inputs / schema / auth
    Enriching           = 22, // fetching/augmenting data from other systems
    Executing           = 23, // the core “do the thing” step
    Persisting          = 24, // writing results/state (db, files, s3, etc.)
    Publishing          = 25, // emitting events/webhooks/messages
    Finalizing          = 26, // cleanup, closeout, releasing resources

    // 30–39: Waiting mid-flight (still alive but blocked)
    WaitingExternal     = 30, // waiting on external system response
    WaitingRateLimit    = 31, // backoff due to rate limiting
    WaitingRetryBackoff = 32, // backoff before retry
    WaitingManual       = 33, // waiting for human action (approval/review)

    // 40–49: Partial/terminal success variants
    Succeeded           = 40, // completed successfully
    SucceededWithWarnings = 41, // completed but had recoverable issues
    Completed           = 42, // generic “done” (use if you don’t want success semantics)

    // 50–59: Failure states (terminal)
    Failed              = 50, // terminal failure (no more retries)
    FailedValidation    = 51, // bad input / schema / business rule violation
    FailedExternal      = 52, // external dependency failure (upstream/downstream)
    FailedTimeout       = 53, // exceeded time limit
    FailedConflict      = 54, // concurrency / optimistic lock / duplicate processing
    FailedSecurity      = 55, // authz/authn/secret issues

    // 60–69: Retry semantics (non-terminal)
    RetryRequested      = 60, // explicit retry requested (operator/system)
    Retrying            = 61, // currently retry attempt in progress
    DeadLettered        = 62, // moved to DLQ/quarantine after exhausting retries
    Quarantined         = 63, // isolated for investigation (don’t auto-run)

    // 70–79: Cancellation / stop conditions (terminal)
    CancelRequested     = 70, // cancellation requested (job should stop ASAP)
    Cancelling          = 71, // cooperative cancellation underway
    Cancelled           = 72, // terminal cancelled
    Superseded          = 73, // replaced by a newer job (idempotent update pattern)

    // 80–89: Expiration / staleness (terminal-ish)
    Expired             = 80, // TTL exceeded before completion
    Abandoned           = 81, // lease lost and job not recoverable / orphaned
    Stale               = 82, // exceeded freshness SLA (might still run, but flagged)

    // 90–99: Reserved for future generic cross-product statuses
    Reserved90          = 90,
    Reserved91          = 91,
    Reserved92          = 92,
    Reserved93          = 93,
    Reserved94          = 94,
    Reserved95          = 95,
    Reserved96          = 96,
    Reserved97          = 97,
    Reserved98          = 98,
    Reserved99          = 99
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
