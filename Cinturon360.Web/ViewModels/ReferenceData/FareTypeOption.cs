using Cinturon360.Shared.Models.Static.Travel;

namespace Cinturon360.Web.ViewModels.ReferenceData;

public sealed record FareTypeOption(
    FlightTravelClassType Type,   // FlightTravelClassType.Economy
    string DisplayName            // "Economy"
);

public sealed record FareTypeReference
{
    public IReadOnlyList<FareTypeOption> Options { get; init; } = [];
}
