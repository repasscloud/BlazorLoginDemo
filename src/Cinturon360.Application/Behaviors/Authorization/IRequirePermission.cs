namespace Cinturon360.Application.Behaviors.Authorization;

/// <summary>
/// Marker interface for MediatR requests that require a permission check.
/// Commands and queries implementing this interface will be evaluated by
/// <see cref="AuthorizationBehavior{TRequest,TResponse}"/> before reaching the handler.
/// </summary>
public interface IRequirePermission
{
    /// <summary>
    /// The permission code that the current user must hold.
    /// Use constants from <see cref="Cinturon360.Common.Constants.PermissionCodes"/>.
    /// </summary>
    string RequiredPermission { get; }
}
