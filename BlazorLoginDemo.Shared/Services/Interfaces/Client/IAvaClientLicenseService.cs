namespace BlazorLoginDemo.Shared.Services.Interfaces.Client;

public interface IAvaClientLicenseService
{
    // CREATE
    Task<AvaClientLicense> CreateAsync(AvaClientLicense license, CancellationToken ct = default);

    // READ
    Task<AvaClientLicense?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<AvaClientLicense>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AvaClientLicense>> GetForClientAsync(string clientId, CancellationToken ct = default);

    // UPDATE (replace whole object)
    Task<AvaClientLicense> UpdateAsync(AvaClientLicense license, CancellationToken ct = default);

    // DELETE
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);

    // UTIL
    Task<bool> ExistsAsync(string id, CancellationToken ct = default);
}
