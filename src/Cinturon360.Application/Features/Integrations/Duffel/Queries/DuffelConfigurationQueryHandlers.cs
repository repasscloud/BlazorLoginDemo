using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Application.Features.Integrations.Duffel.Commands;
using Cinturon360.Common.Results;
using Cinturon360.Contracts.System;
using Cinturon360.Domain.Enums.System;
using Cinturon360.Domain.Enums.Travel;
using MediatR;

namespace Cinturon360.Application.Features.Integrations.Duffel.Queries;

public sealed class GetDuffelOrgConfigurationQueryHandler(
    IDuffelOrgConfigurationRepository repository)
    : IRequestHandler<GetDuffelOrgConfigurationQuery, Result<DuffelOrgConfigurationResponse?>>
{
    public async Task<Result<DuffelOrgConfigurationResponse?>> Handle(GetDuffelOrgConfigurationQuery request, CancellationToken ct)
    {
        var config = await repository.GetByOrgIdAsync(request.OrgId, ct);
        return Result.Success(config is null ? null : UpsertDuffelOrgConfigurationCommandHandler.Map(config));
    }
}

public sealed class GetEffectiveDuffelConfigurationsForOrgQueryHandler(
    IOrganisationRepository organisationRepository,
    IDuffelOrgConfigurationRepository configurationRepository)
    : IRequestHandler<GetEffectiveDuffelConfigurationsForOrgQuery, Result<EffectiveDuffelConfigResponse>>
{
    private static readonly Error OrgNotFound = new("duffel.org_not_found", "Organisation not found.");

    public async Task<Result<EffectiveDuffelConfigResponse>> Handle(GetEffectiveDuffelConfigurationsForOrgQuery request, CancellationToken ct)
    {
        var org = await organisationRepository.GetByIdAsync(request.OrgId, ct);
        if (org is null)
            return Result.Failure<EffectiveDuffelConfigResponse>(OrgNotFound);

        var baseTmcOrgId = await ResolveTmcOrgIdAsync(org.Id, org.OrgType, org.ParentOrgId, ct);

        var tmc = await organisationRepository.GetByIdAsync(baseTmcOrgId, ct);
        if (tmc is null)
            return Result.Failure<EffectiveDuffelConfigResponse>(OrgNotFound);

        var candidateOrgIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { tmc.Id };

        if (!string.IsNullOrWhiteSpace(tmc.ChainCode))
        {
            var allTmcs = await organisationRepository.GetByTypeAsync(OrgType.Tmc, ct);
            foreach (var sibling in allTmcs.Where(x =>
                         !string.IsNullOrWhiteSpace(x.ChainCode) &&
                         string.Equals(x.ChainCode, tmc.ChainCode, StringComparison.OrdinalIgnoreCase)))
            {
                candidateOrgIds.Add(sibling.Id);
            }
        }

        var configs = await configurationRepository.ListByOrgIdsAsync(candidateOrgIds.ToList(), ct);

        var effective = configs
            .Where(x => x.IsEnabled)
            .Where(x => x.OrgId == tmc.Id || x.AccessScope == DuffelAccessScope.ChainWide)
            .OrderByDescending(x => x.OrgId == tmc.Id)
            .ThenBy(x => x.OrgId)
            .Select(UpsertDuffelOrgConfigurationCommandHandler.Map)
            .ToList();

        return Result.Success(new EffectiveDuffelConfigResponse(
            OrgId: org.Id,
            Configurations: effective));
    }

    private static Task<string> ResolveTmcOrgIdAsync(string orgId, OrgType orgType, string? parentOrgId, CancellationToken ct)
    {
        if (orgType == OrgType.Tmc)
            return Task.FromResult(orgId);

        if (orgType == OrgType.Client && !string.IsNullOrWhiteSpace(parentOrgId))
            return Task.FromResult(parentOrgId);

        return Task.FromResult(orgId);
    }
}
