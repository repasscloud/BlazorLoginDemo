using MediatR;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Common.Results;
using Cinturon360.Contracts.Organizations;
using Cinturon360.Domain.Enums.System;

namespace Cinturon360.Application.Features.Organizations.Queries;

public sealed class GetOrganisationByIdQueryHandler(IOrganisationRepository orgRepository)
    : IRequestHandler<GetOrganisationByIdQuery, Result<OrganisationDetail>>
{
    public async Task<Result<OrganisationDetail>> Handle(GetOrganisationByIdQuery q, CancellationToken ct)
    {
        var org = await orgRepository.GetByIdAsync(q.OrgId, ct);
        if (org is null)
            return Result.Failure<OrganisationDetail>(new("organisation.not_found", "Organisation not found."));

        return Result.Success(new OrganisationDetail(
            OrgId: org.Id,
            Name: org.Name,
            Slug: org.Slug,
            OrgType: org.OrgType.ToString(),
            ParentOrgId: org.ParentOrgId,
            IsActive: org.IsActive,
            PrimaryEmail: org.PrimaryEmail,
            PrimaryPhone: org.PrimaryPhone,
            Website: org.Website,
            LanguageCode: org.LanguageCode,
            TimeZone: org.TimeZone,
            CurrencyCode: org.CurrencyCode,
            CreatedAt: org.CreatedAt));
    }
}

public sealed class GetOrganisationBySlugQueryHandler(IOrganisationRepository orgRepository)
    : IRequestHandler<GetOrganisationBySlugQuery, Result<OrganisationDetail>>
{
    public async Task<Result<OrganisationDetail>> Handle(GetOrganisationBySlugQuery q, CancellationToken ct)
    {
        var org = await orgRepository.GetBySlugAsync(q.Slug, ct);
        if (org is null)
            return Result.Failure<OrganisationDetail>(new("organisation.not_found", "Organisation not found."));

        return Result.Success(new OrganisationDetail(
            OrgId: org.Id,
            Name: org.Name,
            Slug: org.Slug,
            OrgType: org.OrgType.ToString(),
            ParentOrgId: org.ParentOrgId,
            IsActive: org.IsActive,
            PrimaryEmail: org.PrimaryEmail,
            PrimaryPhone: org.PrimaryPhone,
            Website: org.Website,
            LanguageCode: org.LanguageCode,
            TimeZone: org.TimeZone,
            CurrencyCode: org.CurrencyCode,
            CreatedAt: org.CreatedAt));
    }
}

public sealed class ListOrganisationsQueryHandler(IOrganisationRepository orgRepository)
    : IRequestHandler<ListOrganisationsQuery, Result<IReadOnlyList<OrganisationSummary>>>
{
    public async Task<Result<IReadOnlyList<OrganisationSummary>>> Handle(ListOrganisationsQuery q, CancellationToken ct)
    {
        OrgType? orgType = null;
        if (q.OrgType is not null && Enum.TryParse<OrgType>(q.OrgType, ignoreCase: true, out var parsed))
            orgType = parsed;

        var orgs = await orgRepository.ListAsync(q.ParentOrgId, orgType, q.ActiveOnly, q.Page, q.PageSize, ct);
        var result = orgs.Select(org => new OrganisationSummary(
            OrgId: org.Id,
            Name: org.Name,
            Slug: org.Slug,
            OrgType: org.OrgType.ToString(),
            ParentOrgId: org.ParentOrgId,
            IsActive: org.IsActive,
            CreatedAt: org.CreatedAt)).ToList();

        return Result.Success<IReadOnlyList<OrganisationSummary>>(result);
    }
}

public sealed class GetOrganisationHierarchyQueryHandler(IOrganisationRepository orgRepository)
    : IRequestHandler<GetOrganisationHierarchyQuery, Result<OrganisationHierarchyNode>>
{
    public async Task<Result<OrganisationHierarchyNode>> Handle(GetOrganisationHierarchyQuery q, CancellationToken ct)
    {
        var root = await orgRepository.GetByIdAsync(q.RootOrgId, ct);
        if (root is null)
            return Result.Failure<OrganisationHierarchyNode>(new("organisation.not_found", "Root organisation not found."));

        var allOrgs = await orgRepository.GetAllAsync(ct);
        var node = BuildNode(root.Id, allOrgs);
        return Result.Success(node);
    }

    private static OrganisationHierarchyNode BuildNode(string orgId, IReadOnlyList<Domain.Entities.Organization.Organisation> all)
    {
        var org = all.First(o => o.Id == orgId);
        var children = all
            .Where(o => o.ParentOrgId == orgId)
            .Select(child => BuildNode(child.Id, all))
            .ToList();

        return new OrganisationHierarchyNode(
            OrgId: org.Id,
            Name: org.Name,
            OrgType: org.OrgType.ToString(),
            IsActive: org.IsActive,
            Children: children);
    }
}
