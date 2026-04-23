namespace Cinturon360.Web.ViewModels.ReferenceData;

public sealed record AirlineOption(
    string Code,          // airline code
    string Name,          // airline name
    string LogoUrl        // airline logo URL
);

public sealed record AirlineReference
{
    public IReadOnlyList<AirlineOption> Options { get; init; } = [];
}
