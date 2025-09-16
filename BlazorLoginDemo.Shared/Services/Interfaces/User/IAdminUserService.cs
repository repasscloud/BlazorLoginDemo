using BlazorLoginDemo.Shared.Models.User;

namespace BlazorLoginDemo.Shared.Services.Interfaces.User;
public interface IAdminUserService
{
    // Composite return type for CRUD (Identity + Profile)
    public sealed record ProvisionedUser(string UserId, ApplicationUser Identity, AvaUser Profile);

    // CREATE
    public sealed record CreateUserRequest(
        string Email,
        string Password,
        string? FirstName,
        string? MiddleName,
        string? LastName,
        string? DisplayName,
        string? OrganizationId,
        string RoleName,
        string? ManagerAvaUserId // optional line-approval manager (self-FK to AvaUser)
    );

    // Convenience result for UI flows that want Ok/Error/UserId
    public sealed record CreateUserResult(bool Ok, string? Error, string? UserId);

    Task<ProvisionedUser> CreateAsync(CreateUserRequest req, CancellationToken ct = default);
    Task<CreateUserResult> CreateUserAsync(CreateUserRequest req, CancellationToken ct = default);

    // READ
    Task<ProvisionedUser?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<ProvisionedUser>> GetAllAsync(CancellationToken ct = default);

    // UPDATE (replace key identity/profile fields)
    public sealed record UpdateUserRequest(
        string UserId,
        string? FirstName,
        string? MiddleName,
        string? LastName,
        string? DisplayName,
        string? OrganizationId,
        bool? IsActive,
        string? ManagerAvaUserId
    );
    Task<ProvisionedUser> UpdateAsync(UpdateUserRequest req, CancellationToken ct = default);

    // DELETE
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);

    // UTIL
    Task<bool> ExistsAsync(string id, CancellationToken ct = default);
}
