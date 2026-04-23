using MediatR;
using Microsoft.Extensions.Logging;

namespace Cinturon360.Application.Behaviors.Authorization;

/// <summary>
/// MediatR pipeline behaviour: placeholder for command-level authorization checks.
/// Implement IAuthorizedRequest on commands that require permission checks.
/// </summary>
public sealed class AuthorizationBehavior<TRequest, TResponse>(
    ILogger<AuthorizationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // TODO: Resolve ICurrentUser, check org-scoped permissions, raise UnauthorizedAccessException as needed.
        return await next();
    }
}
