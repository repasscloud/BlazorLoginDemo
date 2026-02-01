namespace Cinturon360.Web.ViewModels.ReferenceData;

public sealed record CurrencyOption(
    string Code,          // "AUD"
    string DisplayName    // "Australian Dollar (A$)"
);

public sealed record CurrencyReference
{
    public IReadOnlyList<CurrencyOption> Options { get; init; } = [];
}
