using static Cinturon360.Shared.Models.Static.Organization.OrganizationSectors;
using static Cinturon360.Shared.Models.Static.Organization.OrganizationTypes;

namespace Cinturon360.Shared.Models.Organizations;

public sealed record OrganizationListItem(
    // visible fields
    string Id,
    string OrgName,
    bool IsActive,
    OrganizationType OrgType,
    OrganizationSector OrganizationSector,

    // convenience search data
    string? TaxRegistrationNumber,

    string? AddressLine1,
    string? City,
    
    string? MailingAddressLine1,
    string? MailingCity,
    
    string? ContactPersonLastName,
    string? ContactPersonPhone,
    string? ContactPersonEmail,
    
    string? BillingPersonLastName,
    string? BillingPersonPhone,
    string? BillingPersonEmail,
    
    string? AdminPersonLastName,
    string? AdminPersonPhone,
    string? AdminPersonEmail
);