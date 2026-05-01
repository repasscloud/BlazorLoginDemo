using MediatR;
using Cinturon360.Common.Results;
using Cinturon360.Contracts.System;

namespace Cinturon360.Application.Features.Integrations.Duffel.Queries;

public sealed record GetDuffelOrgConfigurationQuery(string OrgId)
    : IRequest<Result<DuffelOrgConfigurationResponse?>>;

public sealed record GetEffectiveDuffelConfigurationsForOrgQuery(string OrgId)
    : IRequest<Result<EffectiveDuffelConfigResponse>>;
