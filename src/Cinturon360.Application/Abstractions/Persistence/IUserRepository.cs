using Cinturon360.Domain.Entities.Identity;

namespace Cinturon360.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetByOrgIdAsync(string orgId, CancellationToken ct = default);
}
