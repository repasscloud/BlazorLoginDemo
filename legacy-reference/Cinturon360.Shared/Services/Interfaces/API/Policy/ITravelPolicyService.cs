using Cinturon360.Shared.Models.Geography;
using Cinturon360.Shared.Models.Policies;

namespace Cinturon360.Shared.Services.Interfaces.Policy;

public interface ITravelPolicyService
{
    // CREATE
    Task CreateAsync(TravelPolicy policy, CancellationToken ct = default);
    Task CreateDefaultAsync(TravelPolicy policy, CancellationToken ct = default);

    // READ
    Task<TravelPolicy?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<TravelPolicy>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TravelPolicy>> GetForOrganizationAsync(string organizationId, CancellationToken ct = default);

    // UPDATE (replace whole object)
    Task<bool> UpdateAsync(TravelPolicy policy, CancellationToken ct = default);

    // DELETE
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);

    // UTIL
    Task<bool> ExistsAsync(string id, CancellationToken ct = default);
    Task<bool> SetAsDefaultPolicyAsync(string policyId, string organizationId, bool isNew = false, CancellationToken ct = default);
    Task<bool> SetPolicyAsDefaultAsync(string policyId, CancellationToken ct = default);

    // RESOLUTION: Regions/Continents/Countries minus DisabledCountryIds -> Countries
    Task<IReadOnlyList<Country>> ResolveAllowedCountriesAsync(string travelPolicyId, CancellationToken ct = default);
}
