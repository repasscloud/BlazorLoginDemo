using MediatR;
using Microsoft.Extensions.Logging;
using Cinturon360.Application.Abstractions.Security;

namespace Cinturon360.Application.Behaviors.Authorization;

/// <summary>
/// MediatR pipeline behaviour: enforces permission checks on commands and queries
/// that implement <see cref="IRequirePermission"/>.
/// </summary>
public sealed class AuthorizationBehavior<TRequest, TResponse>(
    ICurrentUser currentUser,
    ILogger<AuthorizationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IRequirePermission permissionRequest)
            return await next();

        if (!currentUser.IsAuthenticated)
        {
            logger.LogWarning(
                "Unauthenticated access attempt to {Request}",
                typeof(TRequest).Name);
            throw new UnauthorizedAccessException("Authentication is required.");
        }

        if (!currentUser.HasPermission(permissionRequest.RequiredPermission))
        {
            logger.LogWarning(
                "User {UserId} denied — missing permission '{Permission}' for {Request}",
                currentUser.UserId,
                permissionRequest.RequiredPermission,
                typeof(TRequest).Name);
            throw new UnauthorizedAccessException(
                $"Permission '{permissionRequest.RequiredPermission}' is required.");
        }

        return await next();
    }
}
