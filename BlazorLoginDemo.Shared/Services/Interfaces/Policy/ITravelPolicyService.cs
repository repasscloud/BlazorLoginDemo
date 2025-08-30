namespace BlazorLoginDemo.Shared.Services.Interfaces.Policy;
public interface ITravelPolicyService
{
    // CREATE
    Task<TravelPolicy> CreateAsync(TravelPolicy policy, CancellationToken ct = default);
    Task<TravelPolicy> CreateDefaultAsync(TravelPolicy policy, CancellationToken ct = default);

    // READ
    Task<TravelPolicy?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<TravelPolicy>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TravelPolicy>> GetForClientAsync(string clientId, CancellationToken ct = default);

    // UPDATE (replace whole object)
    Task<TravelPolicy> UpdateAsync(TravelPolicy policy, CancellationToken ct = default);

    // DELETE
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);

    // UTIL
    Task<bool> ExistsAsync(string id, CancellationToken ct = default);
}
