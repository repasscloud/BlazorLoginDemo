using Cinturon360.Shared.Models.Policies;

namespace Cinturon360.Shared.Services.Interfaces.Policies;

public interface IContinentService
{
    // CREATE
    Task<Continent> CreateAsync(Continent continent, CancellationToken ct = default);

    // READ
    Task<Continent?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Continent?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<Continent>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Country>> GetAssignedCountriesAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Continent>> GetByRegionIdAsync(int regionId, CancellationToken ct = default);

    // UPDATE
    Task<Continent> UpdateAsync(Continent continent, CancellationToken ct = default);

    // DELETE
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    // UTIL
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
}
