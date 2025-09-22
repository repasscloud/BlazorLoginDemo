using BlazorLoginDemo.Shared.Models.Kernel.Platform;

namespace BlazorLoginDemo.Shared.Services.Interfaces.Platform;

public interface IAdminUserServiceUnified
{
    public sealed record UserAggregate(string UserId, ApplicationUser User, OrganizationUnified? Organization);

    public sealed record CreateUserRequest(
        string Email,
        string Password,
        string? FirstName,
        string? MiddleName,
        string? LastName,
        string? DisplayName,
        string? OrganizationId,
        string? RoleName,
        string? ManagerUserId // optional ApplicationUser.Id
    );

    public sealed record CreateUserResult(bool Ok, string? Error, string? UserId);

    Task<UserAggregate> CreateAsync(CreateUserRequest req, CancellationToken ct = default);
    Task<CreateUserResult> CreateUserAsync(CreateUserRequest req, CancellationToken ct = default);

    // READ
    Task<UserAggregate?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<UserAggregate>> SearchAsync(
        string? emailContains = null,
        string? nameContains = null,
        string? organizationId = null,
        bool? isActive = null,
        CancellationToken ct = default);

    // UPDATE (selected fields)
    public sealed record UpdateUserRequest(
        string UserId,
        string? FirstName,
        string? MiddleName,
        string? LastName,
        string? DisplayName,
        string? OrganizationId,
        bool? IsActive,
        string? ManagerUserId
    );
    Task<UserAggregate> UpdateAsync(UpdateUserRequest req, CancellationToken ct = default);

    /// <summary>
    /// Updates a user's domain/profile fields in bulk. Identity base fields (UserName, PasswordHash, etc.)
    /// are not modified. Navigation properties are not traversed.
    /// </summary>
    Task<bool> UpdateUserAsync(ApplicationUser req, CancellationToken ct = default);

    // DELETE
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);

    // UTIL
    Task<bool> ExistsAsync(string id, CancellationToken ct = default);
}