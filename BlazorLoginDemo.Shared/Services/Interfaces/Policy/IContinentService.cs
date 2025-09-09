using BlazorLoginDemo.Shared.Models.Policies;

namespace BlazorLoginDemo.Shared.Services.Interfaces.Policies;

public interface IContinentService
{
    // CREATE
    Task<Continent> CreateAsync(Continent continent, CancellationToken ct = default);

    // READ
    Task<Continent?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Continent?> GetByIsoAsync(string isoCode, CancellationToken ct = default);
    Task<IReadOnlyList<Continent>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Continent>> GetAllWithCountriesAsync(CancellationToken ct = default);

    // UPDATE
    Task<Continent> UpdateAsync(Continent continent, CancellationToken ct = default);

    // DELETE
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    // UTIL
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
}
