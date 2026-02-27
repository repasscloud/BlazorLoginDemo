using Cinturon360.Shared.Models.Geography;

namespace Cinturon360.Web.ViewModels.ReferenceData;

public sealed class GeographyReference
{
    public IReadOnlyList<Region> Regions { get; init; } = [];
    public IReadOnlyList<Continent> Continents { get; init; } = [];
    public IReadOnlyList<Country> Countries { get; init; } = [];
}
