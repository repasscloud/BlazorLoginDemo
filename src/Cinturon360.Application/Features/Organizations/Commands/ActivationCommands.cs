using MediatR;
using Cinturon360.Common.Results;

namespace Cinturon360.Application.Features.Organizations.Commands;

public sealed record DeactivateOrganisationCommand(string OrgId) : IRequest<Result>;

public sealed record ReactivateOrganisationCommand(string OrgId) : IRequest<Result>;
