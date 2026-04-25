using MediatR;
using Cinturon360.Common.Results;
using Cinturon360.Contracts.Auth;

namespace Cinturon360.Application.Features.Auth.Commands;

/// <summary>Creates a Personal Access Token for the requesting user.</summary>
public sealed record CreatePatCommand(
    string UserId,
    string Name,
    DateTimeOffset? ExpiresAt = null,
    string? Scopes = null
) : IRequest<Result<CreatePatResponse>>;
