namespace Cinturon360.Application.Abstractions.Security;

/// <summary>
/// Resolves the current authenticated principal from request context.
/// Implemented in Infrastructure/API layer via IHttpContextAccessor.
/// </summary>
public interface ICurrentUser
{
    string? UserId { get; }
    string? OrgId { get; }
    bool IsAuthenticated { get; }
    bool IsPlatformUser { get; }
    IReadOnlyList<string> Permissions { get; }

    bool HasPermission(string permissionCode);
}
