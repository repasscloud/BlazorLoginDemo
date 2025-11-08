using Cinturon360.Shared.Models.Policies;

namespace Cinturon360.Shared.Services.Interfaces.Policies;

public interface IRegionService
{
    // CREATE
    Task<Region> CreateAsync(Region region, CancellationToken ct = default);

    // READ
    Task<Region?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Region?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<Region>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Continent>> GetAssignedContinentsAsync(int id, CancellationToken ct = default);

    // UPDATE
    Task<Region> UpdateAsync(Region region, CancellationToken ct = default);

    // DELETE
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    // UTIL
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
}
