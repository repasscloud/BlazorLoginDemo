using Cinturon360.Shared.Models.Static.Organization;

namespace Cinturon360.Web.ViewModels.Orgs;

public sealed class OrgListItemVm
{
    public required string Id { get; init; }
    public required string OrgName { get; init; }
    public required bool IsActive { get; init; }
    public required OrganizationTypes.OrganizationType OrgType { get; init; }
    public required OrganizationSectors.OrganizationSector OrgSector { get; init; }
    public required string TaxRegistrationNumber { get; init; }
    public required string AddressLine1 { get; init; }
    public required string City { get; init; }
    public required string MailingAddressLine1 { get; init; }
    public required string MailingCity { get; init; }
    public required string ContactPersonLastName { get; init; }
    public required string ContactPersonPhone { get; init; }
    public required string ContactPersonEmail { get; init; }
    public required string BillingPersonLastName { get; init; }
    public required string BillingPersonPhone { get; init; }
    public required string BillingPersonEmail { get; init; }
    public required string AdminPersonLastName { get; init; }
    public required string AdminPersonPhone { get; init; }
    public required string AdminPersonEmail { get; init; }
}