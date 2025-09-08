using BlazorLoginDemo.Shared.Models.User;

namespace BlazorLoginDemo.Shared.Services.Interfaces.User;

public interface IAvaUserService
{
    // CREATE
    Task<AvaUser> CreateAsync(AvaUser user, CancellationToken ct = default);

    // READ
    Task<AvaUser?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<AvaUser?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<AvaUser?> GetByAspNetUserIdAsync(string aspNetUsersId, CancellationToken ct = default);
    Task<IReadOnlyList<AvaUser>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AvaUser>> SearchUsersAsync(string query, int page = 0, int take = 50, CancellationToken ct = default);

    // UPDATE (replace whole object)
    Task<AvaUser> UpdateAsync(AvaUser user, CancellationToken ct = default);

    // DELETE
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);

    // UTIL
    Task<bool> ExistsAsync(string id, CancellationToken ct = default);
    Task<bool> AssignTravelPolicyToUserAsync(string id, string travelPolicyId, CancellationToken ct = default);

    Task<int> IngestUsersAsync(CancellationToken ct = default);
    Task<bool> AssignAvaClientToUserAsync(string id, string clientId, CancellationToken ct = default);
}
