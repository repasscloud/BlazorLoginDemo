namespace Cinturon360.Web.ViewModels.ReferenceData;

public sealed record RailOperatorOption(
    string Code,          // rail provider code
    string Name,          // rail provider name
    string LogoUrl        // rail provider logo URL
);

public sealed record RailOperatorReference
{
    public IReadOnlyList<RailOperatorOption> Options { get; init; } = [];
}
