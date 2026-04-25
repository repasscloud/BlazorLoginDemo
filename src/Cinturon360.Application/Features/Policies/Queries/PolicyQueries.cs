using MediatR;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Common.Results;
using Cinturon360.Domain.Entities.Policy;

namespace Cinturon360.Application.Features.Policies.Queries;

// ── Get by ID ─────────────────────────────────────────────────────────────
public sealed record GetTravelPolicyQuery(string PolicyId) : IRequest<Result<TravelPolicy?>>;

public sealed class GetTravelPolicyHandler : IRequestHandler<GetTravelPolicyQuery, Result<TravelPolicy?>>
{
    private readonly ITravelPolicyRepository _repo;
    public GetTravelPolicyHandler(ITravelPolicyRepository repo) => _repo = repo;

    public async Task<Result<TravelPolicy?>> Handle(GetTravelPolicyQuery request, CancellationToken ct)
    {
        var policy = await _repo.GetByIdAsync(request.PolicyId, ct);
        return Result.Success(policy);
    }
}

// ── List for org ──────────────────────────────────────────────────────────
public sealed record ListOrgPoliciesQuery(string OrgId) : IRequest<Result<IReadOnlyList<TravelPolicy>>>;

public sealed class ListOrgPoliciesHandler : IRequestHandler<ListOrgPoliciesQuery, Result<IReadOnlyList<TravelPolicy>>>
{
    private readonly ITravelPolicyRepository _repo;
    public ListOrgPoliciesHandler(ITravelPolicyRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<TravelPolicy>>> Handle(ListOrgPoliciesQuery request, CancellationToken ct)
    {
        var policies = await _repo.ListForOrgAsync(request.OrgId, ct);
        return Result.Success(policies);
    }
}
