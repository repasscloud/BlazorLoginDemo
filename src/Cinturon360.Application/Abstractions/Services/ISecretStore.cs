namespace Cinturon360.Application.Abstractions.Services;

/// <summary>
/// Secret storage abstraction. Production should back this with Azure Key Vault.
/// </summary>
public interface ISecretStore
{
    Task<string> SetSecretAsync(
        string name,
        string value,
        IReadOnlyDictionary<string, string>? tags,
        CancellationToken cancellationToken);

    Task<string> GetSecretValueAsync(string name, CancellationToken cancellationToken);

    Task DisableSecretAsync(string name, CancellationToken cancellationToken);

    Task DeleteSecretAsync(string name, CancellationToken cancellationToken);
}
