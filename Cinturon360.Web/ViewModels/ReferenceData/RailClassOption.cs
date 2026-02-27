using Cinturon360.Shared.Models.Static.Travel;

namespace Cinturon360.Web.ViewModels.ReferenceData;

public sealed record RailClassOption(
    RailTravelClassType Type,   // RailTravelClassType.SECOND
    string DisplayName          // "Second"
);

public sealed record RailClassReference
{
    public IReadOnlyList<RailClassOption> Options { get; init; } = [];
}
