using BlazorLoginDemo.Shared.Models.Policies;

namespace BlazorLoginDemo.Shared.Services.Interfaces.Policies;

public interface ITravelPolicyDisabledCountryService
{
    // Single-key ops (composite key = TravelPolicyId + CountryId)
    Task<TravelPolicyDisabledCountry?> FindAsync(string policyId, int countryId, CancellationToken ct = default);
    Task<TravelPolicyDisabledCountry> AddAsync(string policyId, int countryId, CancellationToken ct = default);
    Task<bool> RemoveAsync(string policyId, int countryId, CancellationToken ct = default);
    Task<bool> IsDisabledAsync(string policyId, int countryId, CancellationToken ct = default);

    // Reads
    Task<IReadOnlyList<int>> GetDisabledCountryIdsAsync(string policyId, CancellationToken ct = default);
    Task<IReadOnlyList<Country>> GetDisabledCountriesAsync(string policyId, CancellationToken ct = default);

    // Bulk ops
    /// <summary>Replace the disabled set with exactly <paramref name="countryIds"/>. Returns number of changes (adds+removes).</summary>
    Task<int> SetDisabledCountriesAsync(string policyId, IEnumerable<int> countryIds, CancellationToken ct = default);
    Task<int> ClearAsync(string policyId, CancellationToken ct = default);
}
