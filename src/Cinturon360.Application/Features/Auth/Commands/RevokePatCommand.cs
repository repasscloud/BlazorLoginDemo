using MediatR;
using Cinturon360.Common.Results;

namespace Cinturon360.Application.Features.Auth.Commands;

/// <summary>Revokes a PAT by token ID.</summary>
public sealed record RevokePatCommand(
    string TokenId,
    string RequestingUserId
) : IRequest<Result>;
