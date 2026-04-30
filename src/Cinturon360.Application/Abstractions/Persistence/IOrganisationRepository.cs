using Cinturon360.Domain.Entities.Organization;
using Cinturon360.Domain.Enums.System;

namespace Cinturon360.Application.Abstractions.Persistence;

public interface IOrganisationRepository
{
    Task<Organisation?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<Organisation?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Organisation?> GetByIdNoTrackingAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<Organisation>> GetChildrenAsync(string parentOrgId, CancellationToken ct = default);
    Task<IReadOnlyList<Organisation>> GetByTypeAsync(OrgType orgType, CancellationToken ct = default);
    Task<IReadOnlyList<Organisation>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Organisation>> ListAsync(string? parentOrgId, OrgType? orgType, bool activeOnly, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Organisation org, CancellationToken ct = default);
}
