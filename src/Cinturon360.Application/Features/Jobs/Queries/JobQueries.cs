using MediatR;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Common.Results;
using Cinturon360.Domain.Entities.System;

namespace Cinturon360.Application.Features.Jobs.Queries;

// ── Get job status ────────────────────────────────────────────────────────
public sealed record GetJobStatusQuery(string JobId) : IRequest<Result<Job?>>;

public sealed class GetJobStatusHandler : IRequestHandler<GetJobStatusQuery, Result<Job?>>
{
    private readonly IJobRepository _repo;
    public GetJobStatusHandler(IJobRepository repo) => _repo = repo;

    public async Task<Result<Job?>> Handle(GetJobStatusQuery request, CancellationToken ct)
    {
        var job = await _repo.GetByIdAsync(request.JobId, ct);
        return Result.Success(job);
    }
}

// ── List documents ────────────────────────────────────────────────────────
public sealed record ListDocumentsQuery(string SubjectId) : IRequest<Result<IReadOnlyList<StoredDocument>>>;

public sealed class ListDocumentsHandler : IRequestHandler<ListDocumentsQuery, Result<IReadOnlyList<StoredDocument>>>
{
    private readonly IStoredDocumentRepository _repo;
    public ListDocumentsHandler(IStoredDocumentRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<StoredDocument>>> Handle(ListDocumentsQuery request, CancellationToken ct)
    {
        var docs = await _repo.ListForSubjectAsync(request.SubjectId, ct);
        return Result.Success(docs);
    }
}
