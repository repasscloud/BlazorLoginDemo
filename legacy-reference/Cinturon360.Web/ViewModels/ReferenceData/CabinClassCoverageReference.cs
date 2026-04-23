using Cinturon360.Shared.Models.Static.Travel;

namespace Cinturon360.Web.ViewModels.ReferenceData;

public sealed record CabinClassCoverageOption(
    CabinClassCoverageType Type,  // enum-safe value
    string DisplayName,           // "Most Segments"
    string Description            // helper / tooltip text
);

public sealed record CabinClassCoverageReference
{
    public IReadOnlyList<CabinClassCoverageOption> Options { get; init; } = [];
}
