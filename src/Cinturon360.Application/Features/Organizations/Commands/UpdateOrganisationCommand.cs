using MediatR;
using Cinturon360.Common.Results;

namespace Cinturon360.Application.Features.Organizations.Commands;

public sealed record UpdateOrganisationCommand(
    string OrgId,
    string Name,
    string? PrimaryEmail,
    string? PrimaryPhone,
    string? Website,
    string LanguageCode,
    string TimeZone,
    string CurrencyCode
) : IRequest<Result>;
