using BlazorLoginDemo.Shared.Models.Kernel.Billing;

namespace BlazorLoginDemo.Shared.Services.Interfaces.Finance;

public interface ILateFeeConfigService
{
    // CREATE
    Task<LateFeeConfig> CreateAsync(LateFeeConfig config, CancellationToken ct = default);

    // READ
    Task<LateFeeConfig?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<LateFeeConfig>> GetAllAsync(CancellationToken ct = default);
    Task<LateFeeConfig?> GetForLicenseAgreementAsync(string licenseAgreementId, CancellationToken ct = default);

    // UPDATE (replace whole object)
    Task<LateFeeConfig> UpdateAsync(LateFeeConfig config, CancellationToken ct = default);

    // DELETE
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);

    // UTIL
    Task<bool> ExistsAsync(string id, CancellationToken ct = default);
}
