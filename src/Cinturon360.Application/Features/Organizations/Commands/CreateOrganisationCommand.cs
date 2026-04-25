using MediatR;
using Cinturon360.Common.Results;
using Cinturon360.Domain.Enums.System;

namespace Cinturon360.Application.Features.Organizations.Commands;

public sealed record CreateOrganisationCommand(
    string Name,
    string Slug,
    OrgType OrgType,
    string? ParentOrgId = null,
    string? PrimaryEmail = null,
    string LanguageCode = "en",
    string TimeZone = "UTC",
    string CurrencyCode = "USD",
    string? CreatedByUserId = null
) : IRequest<Result<string>>;
