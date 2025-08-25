namespace BlazorLoginDemo.Shared.Services.Interfaces.Client;

public interface IAvaClientService
{
    // CREATE
    Task<AvaClient> CreateAsync(AvaClient client, CancellationToken ct = default);

    // READ
    Task<AvaClient?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<AvaClient>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AvaClient>> SearchClientAsync(string query, int take = 50, CancellationToken ct = default);

    // UPDATE (replace the whole object)
    Task<AvaClient> UpdateAsync(AvaClient client, CancellationToken ct = default);

    // DELETE
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);

    // UTIL
    Task<bool> ExistsAsync(string id, CancellationToken ct = default);
}