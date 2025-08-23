namespace BlazorLoginDemo.Shared.Services.Interfaces;

public interface IGroupResolver
{
    Task<Guid?> ResolveGroupIdForEmailAsync(string? email, CancellationToken ct = default);
}