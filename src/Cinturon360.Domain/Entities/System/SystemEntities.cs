using Cinturon360.Domain.Common.Base;
using Cinturon360.Domain.Enums.System;

namespace Cinturon360.Domain.Entities.System;

/// <summary>
/// A background job record — tracks scheduled/queued work with retry support.
/// </summary>
public sealed class Job : Entity
{
    public JobType JobType { get; private set; }
    public JobStatus Status { get; private set; } = JobStatus.Queued;
    public string? OrgId { get; private set; }
    public string? UserId { get; private set; }

    // Payload (JSON) that the job handler reads
    public string? Payload { get; private set; }

    public int Attempts { get; private set; }
    public int MaxAttempts { get; private set; } = 3;

    public DateTimeOffset? ScheduledAt { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset? NextRetryAt { get; private set; }

    public string? ErrorMessage { get; private set; }
    public string? TraceId { get; private set; }

    private Job() { }

    public static Job Create(
        string id,
        JobType jobType,
        string? payload = null,
        string? orgId = null,
        string? userId = null,
        DateTimeOffset? scheduledAt = null,
        int maxAttempts = 3)
        => new()
        {
            Id = id,
            JobType = jobType,
            Payload = payload,
            OrgId = orgId,
            UserId = userId,
            ScheduledAt = scheduledAt ?? DateTimeOffset.UtcNow,
            MaxAttempts = maxAttempts,
            Status = JobStatus.Queued,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void Start(string? traceId = null)
    {
        Status = JobStatus.Running;
        StartedAt = DateTimeOffset.UtcNow;
        Attempts++;
        TraceId = traceId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Succeed()
    {
        Status = JobStatus.Succeeded;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Fail(string errorMessage, DateTimeOffset? nextRetryAt = null)
    {
        ErrorMessage = errorMessage;
        if (Attempts < MaxAttempts && nextRetryAt.HasValue)
        {
            Status = JobStatus.Retrying;
            NextRetryAt = nextRetryAt;
        }
        else
        {
            Status = JobStatus.Failed;
            CompletedAt = DateTimeOffset.UtcNow;
        }
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        Status = JobStatus.Cancelled;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// A stored document (PDF, CSV, etc.) uploaded to object storage.
/// Tracks the S3/R2 key and metadata.
/// </summary>
public sealed class StoredDocument : Entity
{
    public DocumentType DocumentType { get; private set; }
    public string? OrgId { get; private set; }
    public string? UserId { get; private set; }
    public string? SubjectId { get; private set; }  // BookingId, InvoiceId, etc.

    public string FileName { get; private set; } = string.Empty;
    public string StorageKey { get; private set; } = string.Empty;  // R2 / S3 key
    public string? ContentType { get; private set; }
    public long FileSizeBytes { get; private set; }
    public bool IsPublic { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }

    private StoredDocument() { }

    public static StoredDocument Create(
        string id,
        DocumentType documentType,
        string fileName,
        string storageKey,
        string? contentType,
        long fileSizeBytes,
        string? orgId = null,
        string? userId = null,
        string? subjectId = null,
        bool isPublic = false,
        DateTimeOffset? expiresAt = null)
        => new()
        {
            Id = id,
            DocumentType = documentType,
            FileName = fileName,
            StorageKey = storageKey,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            OrgId = orgId,
            UserId = userId,
            SubjectId = subjectId,
            IsPublic = isPublic,
            ExpiresAt = expiresAt,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
}
