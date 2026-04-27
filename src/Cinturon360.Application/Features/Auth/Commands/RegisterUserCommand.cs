using MediatR;
using Cinturon360.Common.Results;
using Cinturon360.Contracts.Auth;

namespace Cinturon360.Application.Features.Auth.Commands;

public sealed record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? IpAddress,
    string? UserAgent) : IRequest<Result<AuthResponse>>;
