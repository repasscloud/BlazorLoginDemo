using BlazorLoginDemo.Shared.Models.Policies;

namespace BlazorLoginDemo.Shared.Services.Interfaces.Policy;

public interface ITravelPolicyService
{
    // CREATE
    Task<TravelPolicy> CreateAsync(TravelPolicy policy, CancellationToken ct = default);
    Task<TravelPolicy> CreateDefaultAsync(TravelPolicy policy, CancellationToken ct = default);

    // READ
    Task<TravelPolicy?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<TravelPolicy>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TravelPolicy>> GetForOrganizationAsync(string organizationId, CancellationToken ct = default);

    // UPDATE (replace whole object)
    Task<TravelPolicy> UpdateAsync(TravelPolicy policy, CancellationToken ct = default);

    // DELETE
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);

    // UTIL
    Task<bool> ExistsAsync(string id, CancellationToken ct = default);
    Task<bool> SetAsDefaultPolicyAsync(string policyId, string organizationId, bool isNew = false, CancellationToken ct = default);
    Task<bool> SetPolicyAsDefaultAsync(string policyId, CancellationToken ct = default);

    // RESOLUTION: Regions/Continents/Countries minus DisabledCountryIds -> Countries
    Task<IReadOnlyList<Country>> ResolveAllowedCountriesAsync(string travelPolicyId, CancellationToken ct = default);
}
