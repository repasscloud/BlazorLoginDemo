using Microsoft.EntityFrameworkCore;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Data.Context;
using Cinturon360.Domain.Entities.System;
using Cinturon360.Domain.Enums.System;

namespace Cinturon360.Data.Repositories;

internal sealed class JobRepository : IJobRepository
{
    private readonly AppDbContext _db;
    public JobRepository(AppDbContext db) => _db = db;

    public Task<Job?> GetByIdAsync(string id, CancellationToken ct)
        => _db.Jobs.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Job>> GetDueJobsAsync(int batchSize, CancellationToken ct)
        => await _db.Jobs
            .Where(x => (x.Status == JobStatus.Queued || x.Status == JobStatus.Retrying)
                        && x.ScheduledAt <= DateTimeOffset.UtcNow)
            .OrderBy(x => x.ScheduledAt)
            .Take(batchSize)
            .ToListAsync(ct);

    public async Task AddAsync(Job job, CancellationToken ct)
        => await _db.Jobs.AddAsync(job, ct);

    public void Update(Job job)
        => _db.Jobs.Update(job);
}

internal sealed class StoredDocumentRepository : IStoredDocumentRepository
{
    private readonly AppDbContext _db;
    public StoredDocumentRepository(AppDbContext db) => _db = db;

    public Task<StoredDocument?> GetByIdAsync(string id, CancellationToken ct)
        => _db.StoredDocuments.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<StoredDocument>> ListForSubjectAsync(string subjectId, CancellationToken ct)
        => await _db.StoredDocuments.Where(x => x.SubjectId == subjectId).ToListAsync(ct);

    public async Task AddAsync(StoredDocument document, CancellationToken ct)
        => await _db.StoredDocuments.AddAsync(document, ct);

    public void Remove(StoredDocument document)
        => _db.StoredDocuments.Remove(document);
}
