using System.Collections.Concurrent;
using Cinturon360.Application.Abstractions.Services;
using Microsoft.Extensions.Configuration;

namespace Cinturon360.Infrastructure.Secrets;

/// <summary>
/// Development fallback when Key Vault is not configured.
/// Supports runtime set/get for secrets created through the API.
/// </summary>
internal sealed class InMemorySecretStore(IConfiguration configuration) : ISecretStore
{
    private readonly ConcurrentDictionary<string, string> _secrets = new(StringComparer.OrdinalIgnoreCase);

    public Task<string> SetSecretAsync(string name, string value, IReadOnlyDictionary<string, string>? tags, CancellationToken cancellationToken)
    {
        _secrets[name] = value;
        return Task.FromResult(name);
    }

    public Task<string> GetSecretValueAsync(string name, CancellationToken cancellationToken)
    {
        if (_secrets.TryGetValue(name, out var runtimeValue))
            return Task.FromResult(runtimeValue);

        var configValue = configuration[$"Secrets:{name}"];
        if (string.IsNullOrWhiteSpace(configValue))
            throw new InvalidOperationException($"Secret '{name}' not found.");

        return Task.FromResult(configValue);
    }

    public Task DisableSecretAsync(string name, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task DeleteSecretAsync(string name, CancellationToken cancellationToken)
    {
        _secrets.TryRemove(name, out _);
        return Task.CompletedTask;
    }
}
