using BlazorLoginDemo.Shared.Models.Kernel.Platform;
using BlazorLoginDemo.Shared.Models.Static.Platform;

namespace BlazorLoginDemo.Shared.Services.Interfaces.Platform;

public interface IAdminOrgService
{
    // convenience "composite" record if you want to return the org + domains in one go
    public sealed record OrgAggregate(Organization Org, IReadOnlyList<OrganizationDomain> Domains);

    // CREATE
    public sealed record CreateOrgRequest(
        string Name,
        OrganizationType Type,
        string? ParentOrganizationId,
        bool IsActive,
        IReadOnlyList<string> Domains // plain strings; server will create OrganizationDomain rows
    );

    public sealed record CreateOrgResult(bool Ok, string? Error, string? OrganizationId);

    Task<OrgAggregate> CreateAsync(CreateOrgRequest req, CancellationToken ct = default);
    Task<CreateOrgResult> CreateOrgAsync(CreateOrgRequest req, CancellationToken ct = default);

    // READ (kept minimal for now; we’ll expand for search/edit next)
    Task<OrgAggregate?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<OrgAggregate>> SearchAsync(
        string? nameContains,
        OrganizationType? type,
        bool? isActive,
        string? parentOrgId,
        string? domainContains,
        CancellationToken ct = default);

    // UPDATE
    public sealed record UpdateOrgRequest(
        string OrgId,
        string? Name,
        OrganizationType? Type,
        string? ParentOrganizationId,
        bool? IsActive,
        IReadOnlyList<string>? DomainsReplace // when non-null, replace all domains with this set
    );
    Task<OrgAggregate> UpdateAsync(UpdateOrgRequest req, CancellationToken ct = default);

    Task<OrgAggregate> RemoveDomainAsync(
        string orgId,
        string domain,
        CancellationToken ct = default);


    // UTIL
    Task<bool> ExistsAsync(string id, CancellationToken ct = default);
}