using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Domain.Entities.Travel;
using Cinturon360.Domain.Enums.System;
using Cinturon360.Domain.Enums.Travel;

namespace Cinturon360.Application.Features.Integrations.Duffel.Services;

/// <summary>
/// Resolves the effective Duffel org configuration for a given organisation.
/// If the org is a client, it traverses up to its assigned TMC.
/// Respects chain-wide sharing and selects the first enabled configuration.
/// </summary>
public interface IDuffelConfigResolver
{
    /// <summary>
    /// Returns the first enabled Duffel configuration reachable from the given org,
    /// or null if no configuration is available.
    /// </summary>
    Task<DuffelOrgConfiguration?> ResolveAsync(string orgId, CancellationToken ct = default);
}

internal sealed class DuffelConfigResolver(
    IOrganisationRepository orgRepo,
    IDuffelOrgConfigurationRepository configRepo) : IDuffelConfigResolver
{
    public async Task<DuffelOrgConfiguration?> ResolveAsync(string orgId, CancellationToken ct)
    {
        var org = await orgRepo.GetByIdAsync(orgId, ct);
        if (org is null)
            return null;

        // Determine the TMC org to start from
        var tmcOrgId = org.OrgType switch
        {
            OrgType.Tmc    => org.Id,
            OrgType.Client => !string.IsNullOrWhiteSpace(org.ParentOrgId) ? org.ParentOrgId : org.Id,
            _              => org.Id
        };

        var tmc = await orgRepo.GetByIdAsync(tmcOrgId, ct);
        if (tmc is null)
            return null;

        var candidateOrgIds = new List<string> { tmc.Id };

        // Include chain siblings if the TMC has a chain code
        if (!string.IsNullOrWhiteSpace(tmc.ChainCode))
        {
            var allTmcs = await orgRepo.GetByTypeAsync(OrgType.Tmc, ct);
            foreach (var sibling in allTmcs)
            {
                if (!string.IsNullOrWhiteSpace(sibling.ChainCode)
                    && string.Equals(sibling.ChainCode, tmc.ChainCode, StringComparison.OrdinalIgnoreCase)
                    && !candidateOrgIds.Contains(sibling.Id, StringComparer.OrdinalIgnoreCase))
                {
                    candidateOrgIds.Add(sibling.Id);
                }
            }
        }

        var configs = await configRepo.ListByOrgIdsAsync(candidateOrgIds, ct);

        return configs
            .Where(c => c.IsEnabled)
            .Where(c => c.OrgId == tmc.Id || c.AccessScope == DuffelAccessScope.ChainWide)
            .OrderByDescending(c => c.OrgId == tmc.Id)
            .ThenBy(c => c.OrgId)
            .FirstOrDefault();
    }
}
