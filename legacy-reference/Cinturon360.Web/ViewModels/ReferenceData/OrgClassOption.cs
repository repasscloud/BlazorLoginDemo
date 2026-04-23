using static Cinturon360.Shared.Models.Static.Organization.OrganizationClassifications;

namespace Cinturon360.Web.ViewModels.ReferenceData;

public sealed record OrgClassOption(
    OrganizationClassification Type,   // OrganizationClassification.PreStartup
    string DisplayName                 // "Pre-Startup"
);

public sealed record OrgClassReference
{
    public IReadOnlyList<OrgClassOption> Options { get; init; } = [];
}
