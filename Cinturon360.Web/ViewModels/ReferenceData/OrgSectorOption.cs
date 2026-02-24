using Cinturon360.Shared.Models.Static.Platform;

namespace Cinturon360.Web.ViewModels.ReferenceData;

public sealed record OrgSectorOption(
    OrganizationSector Type,  // OrganizationSector.Technology
    string DisplayName        // "Technology"
);

public sealed record OrgSectorReference
{
    public IReadOnlyList<OrgSectorOption> Options { get; init; } = [];
}
