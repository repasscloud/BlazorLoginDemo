using Cinturon360.Shared.Models.Static.Platform;

namespace Cinturon360.Web.ViewModels.ReferenceData;

public sealed record OrgTypeOption(
    OrganizationType Type,   // OrganizationType.Client
    string DisplayName       // "Client"
);

public sealed record OrgTypeReference
{
    public IReadOnlyList<OrgTypeOption> Options { get; init; } = [];
}
