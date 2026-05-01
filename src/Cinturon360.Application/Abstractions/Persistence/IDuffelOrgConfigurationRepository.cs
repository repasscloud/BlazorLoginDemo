using Cinturon360.Domain.Entities.Travel;

namespace Cinturon360.Application.Abstractions.Persistence;

public interface IDuffelOrgConfigurationRepository
{
    Task<DuffelOrgConfiguration?> GetByOrgIdAsync(string orgId, CancellationToken ct = default);
    Task<IReadOnlyList<DuffelOrgConfiguration>> ListByOrgIdsAsync(IReadOnlyList<string> orgIds, CancellationToken ct = default);
    Task AddAsync(DuffelOrgConfiguration config, CancellationToken ct = default);
}
