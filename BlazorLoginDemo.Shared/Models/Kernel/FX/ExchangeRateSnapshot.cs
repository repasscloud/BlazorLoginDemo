namespace BlazorLoginDemo.Shared.Models.Kernel.FX;
public sealed class ExchangeRateSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string BaseCode { get; set; } = "USD";  // 3 letters
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    // entire rate map
    public Dictionary<string, decimal> Rates { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    // provider meta
    public DateTimeOffset ProviderLastUpdateUtc { get; set; }
    public DateTimeOffset ProviderNextUpdateUtc { get; set; }
    public string? ProviderResult { get; set; }
}