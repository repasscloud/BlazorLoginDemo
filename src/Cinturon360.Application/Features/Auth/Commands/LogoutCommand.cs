using MediatR;
using Cinturon360.Common.Results;

namespace Cinturon360.Application.Features.Auth.Commands;

/// <summary>Revokes the current user session (logout).</summary>
public sealed record LogoutCommand(
    string SessionId,
    string UserId
) : IRequest<Result>;
