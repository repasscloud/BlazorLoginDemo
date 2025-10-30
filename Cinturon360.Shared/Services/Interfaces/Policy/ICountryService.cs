using Cinturon360.Shared.Models.Policies;

namespace Cinturon360.Shared.Services.Interfaces.Policies;

public interface ICountryService
{
    // CREATE
    Task<Country> CreateAsync(Country country, CancellationToken ct = default);

    // READ
    Task<Country?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Country?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken ct = default);

    // UPDATE
    Task<Country> UpdateAsync(Country country, CancellationToken ct = default);

    // DELETE
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    // UTIL
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
}
