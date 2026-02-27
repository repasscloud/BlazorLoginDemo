using Cinturon360.Shared.Models.Geography;

namespace Cinturon360.Shared.Contracts.Geography;

// ===============================
// GEOGRAPHY (UNIFIED) DTO
// ===============================

public class GeographyUnifiedDto
{
    // convenience aggregates
    public sealed record GeographyAggregate(
        IReadOnlyList<Region> Regions,
        IReadOnlyList<Continent> Continents,
        IReadOnlyList<Country> Countries,
        CommandMetadata Metadata
    );
}