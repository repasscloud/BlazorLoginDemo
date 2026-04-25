namespace Cinturon360.Contracts.System;

public sealed record EnqueueJobRequest(
    int JobType,
    string? Payload,
    string? OrgId,
    string? UserId,
    DateTimeOffset? ScheduledAt,
    int MaxAttempts = 3);

public sealed record UploadDocumentRequest(
    int DocumentType,
    string FileName,
    string StorageKey,
    string? ContentType,
    long FileSizeBytes,
    string? OrgId,
    string? UserId,
    string? SubjectId,
    bool IsPublic,
    DateTimeOffset? ExpiresAt);

public sealed record JobResponse(
    string Id,
    int JobType,
    int Status,
    string? OrgId,
    string? UserId,
    string? Payload,
    int Attempts,
    int MaxAttempts,
    DateTimeOffset? ScheduledAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    string? ErrorMessage);

public sealed record StoredDocumentResponse(
    string Id,
    int DocumentType,
    string FileName,
    string StorageKey,
    string? ContentType,
    long FileSizeBytes,
    bool IsPublic,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ExpiresAt);
