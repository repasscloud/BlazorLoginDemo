using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Cinturon360.Application.Abstractions.Services;
using Microsoft.Extensions.Configuration;

namespace Cinturon360.Infrastructure.Secrets;

internal sealed class AzureKeyVaultSecretStore : ISecretStore
{
    private readonly SecretClient _client;

    public AzureKeyVaultSecretStore(IConfiguration configuration)
    {
        var vaultUri = configuration["KeyVault:VaultUri"]
            ?? throw new InvalidOperationException("Missing KeyVault:VaultUri.");

        _client = new SecretClient(new Uri(vaultUri), new DefaultAzureCredential());
    }

    public async Task<string> SetSecretAsync(
        string name,
        string value,
        IReadOnlyDictionary<string, string>? tags,
        CancellationToken cancellationToken)
    {
        var secret = new KeyVaultSecret(name, value);

        if (tags is not null)
        {
            foreach (var tag in tags)
                secret.Properties.Tags[tag.Key] = tag.Value;
        }

        var saved = await _client.SetSecretAsync(secret, cancellationToken);
        return saved.Value.Properties.Name;
    }

    public async Task<string> GetSecretValueAsync(string name, CancellationToken cancellationToken)
    {
        var secret = await _client.GetSecretAsync(name, cancellationToken: cancellationToken);
        return secret.Value.Value;
    }

    public async Task DisableSecretAsync(string name, CancellationToken cancellationToken)
    {
        var secret = await _client.GetSecretAsync(name, cancellationToken: cancellationToken);
        secret.Value.Properties.Enabled = false;
        await _client.UpdateSecretPropertiesAsync(secret.Value.Properties, cancellationToken);
    }

    public async Task DeleteSecretAsync(string name, CancellationToken cancellationToken)
    {
        var operation = await _client.StartDeleteSecretAsync(name, cancellationToken);
        await operation.WaitForCompletionAsync(cancellationToken);
    }
}
