namespace Cinturon360.Shared.Contracts.Common;

// ===============================
// Rail Data Unified DTO
// ===============================

public class RailDataUnifiedDto
{
    // convenience aggregates
    public sealed record RailOperatorsWebUILiteAggregate(
        IReadOnlyList<RailOperatorsUIDataLite> RailOperators,
        CommandMetadata Metadata
    );
}

public sealed class RailOperatorsUIDataLite
{
    public required string OperatorId { get; set; }
    public required string Name { get; set; }
    public string? LogoSymbolUrl { get; set; }
}