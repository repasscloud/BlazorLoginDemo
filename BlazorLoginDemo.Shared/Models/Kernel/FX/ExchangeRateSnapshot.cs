namespace BlazorLoginDemo.Shared.Models.Kernel.FX;
public sealed class ExchangeRateSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string BaseCode { get; set; } = "USD";           // 3-letter ISO
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    // Store the entire rates dictionary as JSON for durability and schema stability
    public Dictionary<string, decimal> Rates { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    // optional metadata
    public DateTimeOffset ProviderLastUpdateUtc { get; set; }
    public DateTimeOffset ProviderNextUpdateUtc { get; set; }
    public string? ProviderResult { get; set; }
}