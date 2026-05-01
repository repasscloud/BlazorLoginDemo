using Microsoft.EntityFrameworkCore;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Data.Context;
using Cinturon360.Domain.Entities.Travel;

namespace Cinturon360.Data.Repositories;

public sealed class DuffelOrgConfigurationRepository(AppDbContext db) : IDuffelOrgConfigurationRepository
{
    public Task<DuffelOrgConfiguration?> GetByOrgIdAsync(string orgId, CancellationToken ct = default)
        => db.DuffelOrgConfigurations.FirstOrDefaultAsync(x => x.OrgId == orgId, ct);

    public async Task<IReadOnlyList<DuffelOrgConfiguration>> ListByOrgIdsAsync(IReadOnlyList<string> orgIds, CancellationToken ct = default)
    {
        if (orgIds.Count == 0)
            return [];

        return await db.DuffelOrgConfigurations
            .Where(x => orgIds.Contains(x.OrgId))
            .ToListAsync(ct);
    }

    public Task AddAsync(DuffelOrgConfiguration config, CancellationToken ct = default)
        => db.DuffelOrgConfigurations.AddAsync(config, ct).AsTask();
}
