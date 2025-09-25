namespace BlazorLoginDemo.Shared.Security;

public sealed class InboundApiKeyOptions
{
    public string HeaderName { get; init; } = "X-Ava-ApiKey";
    public List<string> AllowedKeys { get; init; } = new();
}
