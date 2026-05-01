namespace Cinturon360.Application.Abstractions.Persistence;

/// <summary>
/// Resolves effective permission codes for a user from their role assignments.
/// </summary>
public interface IPermissionRepository
{
    /// <summary>
    /// Returns all distinct permission codes granted to the user across all their active
    /// role assignments. Scoped to the given orgId when provided (includes global roles).
    /// </summary>
    Task<IReadOnlyList<string>> GetPermissionsForUserAsync(
        string userId,
        CancellationToken ct = default);
}
