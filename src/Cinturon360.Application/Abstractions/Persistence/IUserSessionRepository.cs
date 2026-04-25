using Cinturon360.Domain.Entities.Identity;
using Cinturon360.Domain.Enums.Security;

namespace Cinturon360.Application.Abstractions.Persistence;

public interface IUserSessionRepository
{
    Task<UserSession?> GetByJtiAsync(string jti, CancellationToken ct = default);
    Task<IReadOnlyList<UserSession>> GetActiveByUserIdAsync(string userId, CancellationToken ct = default);
    Task AddAsync(UserSession session, CancellationToken ct = default);
}
