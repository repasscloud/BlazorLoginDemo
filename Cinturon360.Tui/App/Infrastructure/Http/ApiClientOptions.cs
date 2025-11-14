namespace Cinturon360.Tui.App.Infrastructure.Http;

/// <summary>
/// Base config for an API client. You can later bind from config.
/// </summary>
public sealed class ApiClientOptions
{
    public string Name { get; set; } = string.Empty;
    public string BaseAddress { get; set; } = string.Empty;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);
}
