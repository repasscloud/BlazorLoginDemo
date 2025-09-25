using BlazorLoginDemo.Shared.Models.Policies;

namespace BlazorLoginDemo.Shared.Services.Interfaces.Policies;

public interface ICountryService
{
    // CREATE
    Task<Country> CreateAsync(Country country, CancellationToken ct = default);

    // READ
    Task<Country?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Country?> GetByIsoAsync(string isoCode, CancellationToken ct = default);
    Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Country>> GetAllWithContinentAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Country>> GetByContinentAsync(int continentId, CancellationToken ct = default);

    // UPDATE
    Task<Country> UpdateAsync(Country country, CancellationToken ct = default);

    // DELETE
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    // UTIL
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
}
