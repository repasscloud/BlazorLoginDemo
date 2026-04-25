using MediatR;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Application.Abstractions.Services;
using Cinturon360.Common.IdGeneration;
using Cinturon360.Common.Results;
using Cinturon360.Domain.Entities.System;
using Cinturon360.Domain.Enums.System;

namespace Cinturon360.Application.Features.Jobs.Commands;

// ── Errors ─────────────────────────────────────────────────────────────────
public static class JobErrors
{
    public static readonly Error NotFound     = new("job.not_found",      "Job not found.");
    public static readonly Error DocumentNotFound = new("job.document_not_found", "Stored document not found.");
}

// ── Enqueue job ────────────────────────────────────────────────────────────
public sealed record EnqueueJobCommand(
    JobType JobType,
    string? Payload,
    string? OrgId,
    string? UserId,
    DateTimeOffset? ScheduledAt,
    int MaxAttempts = 3) : IRequest<Result<string>>;

public sealed class EnqueueJobHandler : IRequestHandler<EnqueueJobCommand, Result<string>>
{
    private readonly IJobRepository _repo;
    private readonly IUnitOfWork _uow;

    public EnqueueJobHandler(IJobRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result<string>> Handle(EnqueueJobCommand request, CancellationToken ct)
    {
        var id = IdGenerator.New(IdPrefix.Job);
        var job = Job.Create(id, request.JobType, request.Payload, request.OrgId, request.UserId,
            request.ScheduledAt, request.MaxAttempts);

        await _repo.AddAsync(job, ct);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(id);
    }
}

// ── Upload document ────────────────────────────────────────────────────────
public sealed record UploadDocumentCommand(
    DocumentType DocumentType,
    string FileName,
    string StorageKey,
    string? ContentType,
    long FileSizeBytes,
    string? OrgId,
    string? UserId,
    string? SubjectId,
    bool IsPublic,
    DateTimeOffset? ExpiresAt) : IRequest<Result<string>>;

public sealed class UploadDocumentHandler : IRequestHandler<UploadDocumentCommand, Result<string>>
{
    private readonly IStoredDocumentRepository _repo;
    private readonly IUnitOfWork _uow;

    public UploadDocumentHandler(IStoredDocumentRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result<string>> Handle(UploadDocumentCommand request, CancellationToken ct)
    {
        var id = IdGenerator.New(IdPrefix.Document);
        var doc = StoredDocument.Create(id, request.DocumentType, request.FileName, request.StorageKey,
            request.ContentType, request.FileSizeBytes, request.OrgId, request.UserId,
            request.SubjectId, request.IsPublic, request.ExpiresAt);

        await _repo.AddAsync(doc, ct);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(id);
    }
}
