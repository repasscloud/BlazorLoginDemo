using Cinturon360.Shared.Models.Static.Support;

namespace Cinturon360.Web.ViewModels.ReferenceData;

public sealed record SlaTierOption(
    SlaTier Type,   // SlaTier.Basic
    string DisplayName       // "Basic"
);

public sealed record SlaTierReference
{
    public IReadOnlyList<SlaTierOption> Options { get; init; } = [];
}
