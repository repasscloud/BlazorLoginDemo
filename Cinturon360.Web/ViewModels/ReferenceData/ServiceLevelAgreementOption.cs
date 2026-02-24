using Cinturon360.Shared.Models.Static.Support;

namespace Cinturon360.Web.ViewModels.ReferenceData;

public sealed record ServiceLevelAgreementOption(
    SlaTier Tier,        // SlaTier.Standard
    string DisplayName   // "Standard"
);

public sealed record ServiceLevelAgreementReference
{
    public IReadOnlyList<ServiceLevelAgreementOption> Options { get; init; } = [];
}
