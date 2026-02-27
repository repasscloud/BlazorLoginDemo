namespace Cinturon360.Shared.Security;

public sealed class InboundApiKeyOptions
{
    public string HeaderName { get; init; } = "X-C360-API-KEY";
    public List<string> AllowedKeys { get; init; } = new();
}
