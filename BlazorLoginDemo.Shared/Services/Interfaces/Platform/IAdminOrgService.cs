using BlazorLoginDemo.Shared.Models.DTOs;
using BlazorLoginDemo.Shared.Models.Kernel.Billing;
using BlazorLoginDemo.Shared.Models.Kernel.Platform;
using BlazorLoginDemo.Shared.Models.Static.Platform;

namespace BlazorLoginDemo.Shared.Services.Interfaces.Platform;

// ===============================
// ORG ADMIN (UNIFIED)
// ===============================
public interface IAdminOrgServiceUnified
{
    // convenience aggregate
    public sealed record OrgAggregate(OrganizationUnified Org, IReadOnlyList<OrganizationDomainUnified> Domains, LicenseAgreementUnified? LicenseAgreement);


    // CREATE
    public sealed record CreateOrgRequest(
        string Name,
        OrganizationType Type,
        string? ParentOrganizationId,
        bool IsActive,
        IReadOnlyList<string> Domains // plain strings; server creates OrganizationDomainUnified rows
    );

    public sealed record CreateOrgResult(bool Ok, string? Error, string? OrganizationId);

    public sealed class OrganizationPickerDto
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public BlazorLoginDemo.Shared.Models.Static.Platform.OrganizationType Type { get; init; }
        public bool IsActive { get; init; }

        public string? ContactPersonFirstName { get; init; }
        public string? ContactPersonLastName  { get; init; }
        public string? ContactPersonEmail     { get; init; }
        public string? ContactPersonPhone     { get; init; }

        public string? BillingPersonFirstName { get; init; }
        public string? BillingPersonLastName  { get; init; }
        public string? BillingPersonEmail     { get; init; }
        public string? BillingPersonPhone     { get; init; }

        public string? AdminPersonFirstName   { get; init; }
        public string? AdminPersonLastName    { get; init; }
        public string? AdminPersonPhone       { get; init; }
        public string? AdminPersonEmail       { get; init; }

        public string? TaxId                  { get; init; }
        public string  Country                { get; init; } = string.Empty;
    }

    Task<OrgAggregate> CreateAsync(CreateOrgRequest req, CancellationToken ct = default);
    Task<CreateOrgResult> CreateOrgAsync(CreateOrgRequest req, CancellationToken ct = default);


    // READ / SEARCH
    Task<OrgAggregate?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<OrgAggregate>> SearchAsync(
        string? nameContains = null,
        OrganizationType? type = null,
        bool? isActive = null,
        string? parentOrgId = null,
        string? domainContains = null,
        CancellationToken ct = default);
    Task<IReadOnlyList<OrganizationPickerDto>> GetAllForPickerAsync(CancellationToken ct = default);

    // UPDATE
    public sealed record UpdateOrgRequest(
        string OrgId,
        string? Name,
        OrganizationType? Type,
        string? ParentOrganizationId,
        bool? IsActive,
        IReadOnlyList<string>? DomainsReplace // when non-null, fully replace domain list
    );
    Task<OrgAggregate> UpdateAsync(UpdateOrgRequest req, CancellationToken ct = default);
    Task<bool> UpdateOrgAsync(OrganizationUnified req, CancellationToken ct = default);

    Task<OrgAggregate> RemoveDomainAsync(string orgId, string domain, CancellationToken ct = default);

    // LICENSE AGREEMENT (1:1)
    Task<OrgAggregate> UpsertLicenseAgreementAsync(string orgId, LicenseAgreementUnified model, CancellationToken ct = default);
    Task<bool> DeleteLicenseAgreementAsync(string orgId, CancellationToken ct = default);

    // UTILS
    Task<bool> ExistsAsync(string id, CancellationToken ct = default);
    Task<bool> ValidateTaxIdAsync(string orgId, string taxId, string taxIdType, CancellationToken ct = default);
    Task<string?> GetOrgDefaultTravelPolicyIdAsync(string orgId, CancellationToken ct = default);
    Task<OrgFeesMarkupDto?> GetOrgPnrServiceFeesAsync(string orgId, CancellationToken ct = default);
}
