using Cinturon360.Domain.Entities.System;
using Cinturon360.Domain.Enums.System;

namespace Cinturon360.Application.Abstractions.Persistence;

public interface IJobRepository
{
    Task<Job?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<Job>> GetDueJobsAsync(int batchSize, CancellationToken ct = default);
    Task AddAsync(Job job, CancellationToken ct = default);
    void Update(Job job);
}

public interface IStoredDocumentRepository
{
    Task<StoredDocument?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<StoredDocument>> ListForSubjectAsync(string subjectId, CancellationToken ct = default);
    Task AddAsync(StoredDocument document, CancellationToken ct = default);
    void Remove(StoredDocument document);
}
