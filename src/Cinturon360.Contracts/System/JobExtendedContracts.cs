namespace Cinturon360.Contracts.System;

/// <summary>Response for an enqueued job.</summary>
public sealed record EnqueueJobResponse(string JobId, int Status, DateTimeOffset? ScheduledAt);

/// <summary>Alias for a single job's status.</summary>
public sealed record JobStatusResponse(
    string Id,
    int JobType,
    int Status,
    string? ErrorMessage,
    int Attempts,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt);

/// <summary>List wrapper for stored documents.</summary>
public sealed record DocumentListResponse(IReadOnlyList<StoredDocumentResponse> Items);

/// <summary>Paginated list of jobs.</summary>
public sealed record JobListResponse(
    IReadOnlyList<JobResponse> Items,
    int Total,
    int Page,
    int PageSize);
