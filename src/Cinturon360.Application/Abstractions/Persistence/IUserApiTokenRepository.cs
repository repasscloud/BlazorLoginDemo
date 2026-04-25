using Cinturon360.Domain.Entities.Identity;

namespace Cinturon360.Application.Abstractions.Persistence;

public interface IUserApiTokenRepository
{
    Task<UserApiToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task<IReadOnlyList<UserApiToken>> GetActiveByUserIdAsync(string userId, CancellationToken ct = default);
    Task AddAsync(UserApiToken token, CancellationToken ct = default);
}
