using Cinturon360.Shared.Models.Static.Organization;
using static Cinturon360.Shared.Models.Static.Organization.OrganizationTypes;

namespace Cinturon360.Web.ViewModels.ReferenceData;

public sealed record OrgTypeOption(
    OrganizationType Id,
    string DisplayName
);

public sealed record OrgTypeReference
{
    public IReadOnlyList<OrgTypeOption> Options { get; init; } =
        OrganizationTypes.All
            .Select(t => new OrgTypeOption(
                Id: t.Type,
                DisplayName: t.Name
            ))
            .ToList();

}