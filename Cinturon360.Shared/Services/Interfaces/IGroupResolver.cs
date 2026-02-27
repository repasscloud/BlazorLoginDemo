namespace Cinturon360.Shared.Services.Interfaces;

public interface IGroupResolver
{
    Task<Guid?> ResolveGroupIdForEmailAsync(string? email, CancellationToken ct = default);
}