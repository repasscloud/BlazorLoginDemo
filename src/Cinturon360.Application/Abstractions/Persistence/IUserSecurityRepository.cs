using Cinturon360.Domain.Entities.Identity;

namespace Cinturon360.Application.Abstractions.Persistence;

public interface IUserSecurityRepository
{
    Task<UserSecurity?> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task AddAsync(UserSecurity security, CancellationToken ct = default);
}
