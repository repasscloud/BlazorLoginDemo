using MediatR;
using Cinturon360.Common.Results;
using Cinturon360.Contracts.Organizations;

namespace Cinturon360.Application.Features.Organizations.Queries;

public sealed record GetOrganisationByIdQuery(string OrgId) : IRequest<Result<OrganisationDetail>>;

public sealed record GetOrganisationBySlugQuery(string Slug) : IRequest<Result<OrganisationDetail>>;

public sealed record ListOrganisationsQuery(
    string? ParentOrgId = null,
    string? OrgType = null,
    bool ActiveOnly = true,
    int Page = 1,
    int PageSize = 50
) : IRequest<Result<IReadOnlyList<OrganisationSummary>>>;

public sealed record GetOrganisationHierarchyQuery(string RootOrgId) : IRequest<Result<OrganisationHierarchyNode>>;
