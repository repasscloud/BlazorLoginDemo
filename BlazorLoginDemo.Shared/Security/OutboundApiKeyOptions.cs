namespace BlazorLoginDemo.Shared.Security;

public sealed class OutboundApiKeyOptions
{
    public string HeaderName { get; init; } = "X-Ava-ApiKey";
    public string Key { get; init; } = string.Empty;
}