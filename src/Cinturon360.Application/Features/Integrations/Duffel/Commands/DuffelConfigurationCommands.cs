using MediatR;
using Cinturon360.Common.Results;
using Cinturon360.Contracts.System;

namespace Cinturon360.Application.Features.Integrations.Duffel.Commands;

public sealed record UpsertDuffelOrgConfigurationCommand(
    string OrgId,
    bool IsEnabled,
    bool UseSandbox,
    string ApiBaseUrl,
    string ApiToken,
    string AccessScope,
    IReadOnlyList<string> EnabledCapabilities,
    IReadOnlyList<string> EnabledSearchFunctions,
    IReadOnlyList<string> CorporateCodes,
    IReadOnlyList<string> TourCodes,
    string? Notes,
    string? UpdatedByUserId
) : IRequest<Result<DuffelOrgConfigurationResponse>>;
