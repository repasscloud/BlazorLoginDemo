using BlazorLoginDemo.Shared.Models.Kernel.Billing;

namespace BlazorLoginDemo.Shared.Services.Interfaces.Client;

public interface ILicenseAgreementService
{
    // CREATE
    Task<LicenseAgreement> CreateAsync(LicenseAgreement agreement, CancellationToken ct = default);

    // READ
    Task<LicenseAgreement?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<LicenseAgreement>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<LicenseAgreement>> GetForClientAsync(string clientId, CancellationToken ct = default);

    // UPDATE (replace whole object)
    Task<LicenseAgreement> UpdateAsync(LicenseAgreement agreement, CancellationToken ct = default);

    // DELETE
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);

    // UTIL
    Task<bool> ExistsAsync(string id, CancellationToken ct = default);
}
