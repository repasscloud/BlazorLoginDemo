namespace Cinturon360.Web.ViewModels.ReferenceData;

// Generic option for ACRISS dimensions
public sealed record AcrissOption(
    char Code,          // 'M'
    string CodeUpper,   // "MINI", "ECONOMY_ELITE"
    string DisplayName  // "Mini", "Economy Elite"
);


// Full reference set
public sealed record AcrissReference
{
    public IReadOnlyList<AcrissOption> Categories { get; init; } = [];
    public IReadOnlyList<AcrissOption> Bodies { get; init; } = [];
    public IReadOnlyList<AcrissOption> Transmissions { get; init; } = [];
    public IReadOnlyList<AcrissOption> Fuels { get; init; } = [];
}
