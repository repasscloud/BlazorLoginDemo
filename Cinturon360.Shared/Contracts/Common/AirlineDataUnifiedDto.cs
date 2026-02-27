namespace Cinturon360.Shared.Contracts.Common;

// ===============================
// Airline Data Unified DTO
// ===============================

public class AirlineDataUnifiedDto
{
    // convenience aggregates
    public sealed record AirlineWebUILiteAggregate(
        IReadOnlyList<AirlineUIDataLite> Airlines,
        CommandMetadata Metadata
    );
}

public sealed class AirlineUIDataLite
{
    public required string Name { get; set; }
    public required string IataCode { get; set; }
    public string? IcaoCode { get; set; }
    public string? LogoSymbolUrl { get; set; }
    public string? LogoLockupUrl { get; set; }
}