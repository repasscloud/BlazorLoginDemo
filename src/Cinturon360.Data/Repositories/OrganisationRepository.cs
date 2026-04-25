using Microsoft.EntityFrameworkCore;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Data.Context;
using Cinturon360.Domain.Entities.Organization;
using Cinturon360.Domain.Enums.System;

namespace Cinturon360.Data.Repositories;

public sealed class OrganisationRepository(AppDbContext db) : IOrganisationRepository
{
    public Task<Organisation?> GetByIdAsync(string id, CancellationToken ct = default)
        => db.Organisations.FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<Organisation?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => db.Organisations.FirstOrDefaultAsync(o => o.Slug == slug, ct);

    public async Task<IReadOnlyList<Organisation>> GetChildrenAsync(string parentOrgId, CancellationToken ct = default)
        => await db.Organisations.Where(o => o.ParentOrgId == parentOrgId).ToListAsync(ct);

    public async Task<IReadOnlyList<Organisation>> GetByTypeAsync(OrgType orgType, CancellationToken ct = default)
        => await db.Organisations.Where(o => o.OrgType == orgType).ToListAsync(ct);

    public async Task<IReadOnlyList<Organisation>> GetAllAsync(CancellationToken ct = default)
        => await db.Organisations.ToListAsync(ct);

    public async Task<IReadOnlyList<Organisation>> ListAsync(
        string? parentOrgId, OrgType? orgType, bool activeOnly, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Organisations.AsQueryable();
        if (parentOrgId is not null)
            query = query.Where(o => o.ParentOrgId == parentOrgId);
        if (orgType is not null)
            query = query.Where(o => o.OrgType == orgType);
        if (activeOnly)
            query = query.Where(o => o.IsActive);
        return await query.OrderBy(o => o.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
    }

    public async Task AddAsync(Organisation org, CancellationToken ct = default)
        => await db.Organisations.AddAsync(org, ct);
}
