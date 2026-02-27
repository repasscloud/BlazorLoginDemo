namespace Cinturon360.Web.ViewModels.ReferenceData;

public sealed record CountryOption(
    int Id,               // 14
    string DisplayName    // "Australia"
);

public sealed record CountryReference
{
    public IReadOnlyList<CountryOption> Options { get; init; } = [];
}
