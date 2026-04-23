namespace Cinturon360.Web.ViewModels.ReferenceData;

public sealed record IDDOption(
    string CountryCode,   // "61"
    string DisplayName    // "Australia (+61)"
);

public sealed record IDDReference
{
    public IReadOnlyList<IDDOption> Options { get; init; } = [];
}

// source: https://raw.githubusercontent.com/dr5hn/countries-states-cities-database/master/json/countries.json