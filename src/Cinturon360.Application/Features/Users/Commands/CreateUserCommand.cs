using MediatR;
using Cinturon360.Common.Results;
using Cinturon360.Domain.Enums.Security;

namespace Cinturon360.Application.Features.Users.Commands;

/// <summary>Creates a new user account and their security record.</summary>
public sealed record CreateUserCommand(
    string Email,
    string FirstName,
    string LastName,
    UserCategory UserCategory,
    string? HomeOrgId,
    string? PlaintextPassword = null,
    PlatformRole? PlatformRole = null,
    string? CreatedByUserId = null
) : IRequest<Result<string>>;
