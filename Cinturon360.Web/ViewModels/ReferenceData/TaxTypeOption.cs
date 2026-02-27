using Cinturon360.Shared.Models.Static.Billing;

namespace Cinturon360.Web.ViewModels.ReferenceData;

public sealed record TaxTypeOption(
    TaxationType Id,
    string DisplayName,
    string RegexPattern,
    string Description
);

public sealed record TaxTypeReference
{
    public IReadOnlyList<TaxTypeOption> Options { get; init; } = [];
}