using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Common.IdGeneration;
using Cinturon360.Common.Results;
using Cinturon360.Contracts.System;
using Cinturon360.Domain.Entities.Travel;
using Cinturon360.Domain.Enums.System;
using Cinturon360.Domain.Enums.Travel;
using MediatR;

namespace Cinturon360.Application.Features.Integrations.Duffel.Commands;

public sealed class UpsertDuffelOrgConfigurationCommandHandler(
    IDuffelOrgConfigurationRepository repository,
    IOrganisationRepository organisationRepository,
    IUnitOfWork uow)
    : IRequestHandler<UpsertDuffelOrgConfigurationCommand, Result<DuffelOrgConfigurationResponse>>
{
    private static readonly Error OrgNotFound = new("duffel.org_not_found", "Organisation not found.");
    private static readonly Error InvalidOrgType = new("duffel.invalid_org_type", "Duffel config can only be set for TMC organisations.");
    private static readonly Error MissingApiToken = new("duffel.api_token_required", "ApiToken is required.");
    private static readonly Error InvalidAccessScope = new("duffel.invalid_access_scope", "AccessScope must be one of: tmc_only, chain_wide.");

    public async Task<Result<DuffelOrgConfigurationResponse>> Handle(UpsertDuffelOrgConfigurationCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ApiToken))
            return Result.Failure<DuffelOrgConfigurationResponse>(MissingApiToken);

        var org = await organisationRepository.GetByIdAsync(request.OrgId, ct);
        if (org is null)
            return Result.Failure<DuffelOrgConfigurationResponse>(OrgNotFound);

        if (org.OrgType != OrgType.Tmc)
            return Result.Failure<DuffelOrgConfigurationResponse>(InvalidOrgType);

        if (!TryParseAccessScope(request.AccessScope, out var accessScope))
            return Result.Failure<DuffelOrgConfigurationResponse>(InvalidAccessScope);

        var capabilitiesCsv = Csv(request.EnabledCapabilities);
        var functionsCsv = Csv(request.EnabledSearchFunctions);
        var corporateCodesCsv = Csv(request.CorporateCodes);
        var tourCodesCsv = Csv(request.TourCodes);

        var existing = await repository.GetByOrgIdAsync(request.OrgId, ct);
        if (existing is null)
        {
            existing = DuffelOrgConfiguration.Create(
                IdGenerator.New(IdPrefix.DuffelConfig),
                request.OrgId,
                request.IsEnabled,
                request.UseSandbox,
                request.ApiBaseUrl,
                request.ApiToken,
                accessScope,
                capabilitiesCsv,
                functionsCsv,
                corporateCodesCsv,
                tourCodesCsv,
                request.Notes,
                request.UpdatedByUserId);

            await repository.AddAsync(existing, ct);
        }
        else
        {
            existing.Update(
                request.IsEnabled,
                request.UseSandbox,
                request.ApiBaseUrl,
                request.ApiToken,
                accessScope,
                capabilitiesCsv,
                functionsCsv,
                corporateCodesCsv,
                tourCodesCsv,
                request.Notes,
                request.UpdatedByUserId);
        }

        await uow.SaveChangesAsync(ct);

        return Result.Success(Map(existing));
    }

    private static string Csv(IReadOnlyList<string> values)
        => string.Join(",", values
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase));

    private static IReadOnlyList<string> SplitCsv(string csv)
        => string.IsNullOrWhiteSpace(csv)
            ? []
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

    private static bool TryParseAccessScope(string rawScope, out DuffelAccessScope scope)
    {
        scope = DuffelAccessScope.TmcOnly;

        if (string.IsNullOrWhiteSpace(rawScope))
            return true;

        return rawScope.Trim().ToLowerInvariant() switch
        {
            "tmc_only" => Set(out scope, DuffelAccessScope.TmcOnly),
            "chain_wide" => Set(out scope, DuffelAccessScope.ChainWide),
            _ => false
        };
    }

    private static bool Set(out DuffelAccessScope scope, DuffelAccessScope value)
    {
        scope = value;
        return true;
    }

    internal static DuffelOrgConfigurationResponse Map(DuffelOrgConfiguration config)
        => new(
            ConfigId: config.Id,
            OrgId: config.OrgId,
            IsEnabled: config.IsEnabled,
            UseSandbox: config.UseSandbox,
            ApiBaseUrl: config.ApiBaseUrl,
            AccessScope: config.AccessScope == DuffelAccessScope.ChainWide ? "chain_wide" : "tmc_only",
            EnabledCapabilities: SplitCsv(config.EnabledCapabilitiesCsv),
            EnabledSearchFunctions: SplitCsv(config.EnabledSearchFunctionsCsv),
            CorporateCodes: SplitCsv(config.CorporateCodesCsv),
            TourCodes: SplitCsv(config.TourCodesCsv),
            Notes: config.Notes,
            ApiTokenMasked: Mask(config.ApiToken),
            UpdatedAt: config.UpdatedAt,
            CreatedAt: config.CreatedAt);

    private static string? Mask(string? apiToken)
    {
        if (string.IsNullOrWhiteSpace(apiToken))
            return null;

        var last4 = apiToken.Length <= 4 ? apiToken : apiToken[^4..];
        return $"***{last4}";
    }
}
