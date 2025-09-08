using BlazorLoginDemo.Shared.Models.Kernel.User;

namespace BlazorLoginDemo.Shared.Services.Interfaces.User;

public interface IAvaUserSysPreferenceService
{
    // CREATE
    Task<AvaUserSysPreference> CreateAsync(AvaUserSysPreference preference, CancellationToken ct = default);

    // READ
    Task<AvaUserSysPreference?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<AvaUserSysPreference?> GetByUserIdAsync(string avaUserId, CancellationToken ct = default);
    Task<IReadOnlyList<AvaUserSysPreference>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AvaUserSysPreference>> GetForUserAsync(string aspNetUserId, CancellationToken ct = default);

    // UPDATE
    Task<AvaUserSysPreference> UpdateAsync(AvaUserSysPreference preference, CancellationToken ct = default);

    // DELETE
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);

    // UTIL
    Task<bool> ExistsAsync(string id, CancellationToken ct = default);
}
