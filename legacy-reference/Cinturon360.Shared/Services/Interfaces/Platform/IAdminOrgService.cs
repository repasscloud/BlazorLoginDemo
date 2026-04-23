using Cinturon360.Shared.Models.DTOs;
using Cinturon360.Shared.Models.ExternalLib.Amadeus;
using Cinturon360.Shared.Models.Kernel.Billing;
using Cinturon360.Shared.Models.Kernel.Platform;
using Cinturon360.Shared.Models.Static.Geography;
using Cinturon360.Shared.Models.Static.Identity;
using static Cinturon360.Shared.Models.Static.Organization.OrganizationTypes;

namespace Cinturon360.Shared.Services.Interfaces.Platform;

// ===============================
// ORG ADMIN (UNIFIED)
// ===============================
public interface IAdminOrgServiceUnified
{
    // convenience aggregate
    public sealed record OrgAggregate(
        OrganizationUnified Org,
        IReadOnlyList<OrganizationDomainUnified> Domains,
        LicenseAgreementUnified? LicenseAgreement);

    // convenience context for Amadeus TMC accounts
    public sealed record AmadeusTmcContext(
        OrganizationUnified Tmc,
        AmadeusAccount AmadeusAccount
    );

    // CREATE
    public sealed record CreateOrgRequest(
        string Name,
        OrganizationType Type,
        string? ParentOrganizationId,
        bool IsActive,
        IReadOnlyList<string> Domains // plain strings; server creates OrganizationDomainUnified rows
    );

    public sealed record CreateOrgResult(bool Ok, string? Error, string? OrganizationId);

    public sealed record ClientGoverningTmcInfo(
        string TmcId,
        string TmcName,
        string ClientId,
        string ClientName
    );

    public sealed class OrganizationPickerDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public OrganizationType Type { get; set; } = OrganizationType.Client;
        public bool IsActive { get; set; }

        public string? ContactPersonFirstName { get; set; }
        public string? ContactPersonLastName  { get; set; }
        public string? ContactPersonEmail     { get; set; }
        public string? ContactPersonPhone     { get; set; }

        public string? BillingPersonFirstName { get; set; }
        public string? BillingPersonLastName  { get; set; }
        public string? BillingPersonEmail     { get; set; }
        public string? BillingPersonPhone     { get; set; }

        public string? AdminPersonFirstName   { get; set; }
        public string? AdminPersonLastName    { get; set; }
        public string? AdminPersonPhone       { get; set; }
        public string? AdminPersonEmail       { get; set; }

        public string? TaxId                  { get; set; }
        public PassportCountry  Country       { get; set; }
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
    Task<IReadOnlyList<OrganizationPickerDto>> GetAllChildrenOrgsForPickerAsync(string parentOrgId, CancellationToken ct = default);

    Task<ClientGoverningTmcInfo> GetGoverningTmcInfoAsync(string clientOrgId, CancellationToken ct = default);
    Task<AmadeusTmcContext> GetAmadeusTmcContextAsync(string clientOrgId, string tmcOrgId, CancellationToken ct = default);

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
    Task<bool> ValidateTaxIdAsync(string orgId, string taxId, TaxIdType taxIdType, CancellationToken ct = default);
    Task<string?> GetOrgDefaultTravelPolicyIdAsync(string orgId, CancellationToken ct = default);
    Task<OrgFeesMarkupDto?> GetOrgPnrServiceFeesAsync(string orgId, CancellationToken ct = default);
}
