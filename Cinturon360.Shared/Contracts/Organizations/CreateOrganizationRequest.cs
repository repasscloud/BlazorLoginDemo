using Cinturon360.Shared.Models.Static.Billing;
using Cinturon360.Shared.Models.Static.Geography;
using static Cinturon360.Shared.Models.Static.Organization.OrganizationClassifications;
using static Cinturon360.Shared.Models.Static.Organization.OrganizationSectors;
using static Cinturon360.Shared.Models.Static.Organization.OrganizationTypes;
using static Cinturon360.Shared.Models.Static.Support.SlaTiers;

namespace Cinturon360.Shared.Contracts.Organizations;

public sealed class CreateOrganizationRequest
{
    public string OrganizationName { get; set; } = string.Empty;
    public OrganizationType OrganizationType { get; set; } = OrganizationType.Client;
    public string? ParentOrganizationId { get; set; }
    public bool IsActive { get; set; } = true;
    public string DefaultCurrencyCode { get; set; } = "AUD";
    public OrganizationClassification OrganizationClassification { get; set; } = OrganizationClassification.Unknown;
    public OrganizationSector OrganizationSector { get; set; } = OrganizationSector.Unknown;
    public SlaTier SlaTier { get; set; } = SlaTier.Basic;
    public string TimeZoneId { get; set; } = "Australia/Sydney";
    public TaxationType TaxationType { get; set; } = TaxationType.None;
    public string? TaxId { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public int Country { get; set; } = 14;  // Default to Australia
    public string? MailingAddressLine1 { get; set; }
    public string? MailingAddressLine2 { get; set; }
    public string? MailingAddressLine3 { get; set; }
    public string? MailingCity { get; set; }
    public string? MailingState { get; set; }
    public string? MailingPostalCode { get; set; }
    public int MailingCountry { get; set; } = 14;  // Default to Australia
    public string? ContactPersonFirstName { get; set; }
    public string? ContactPersonLastName { get; set; }
    public int ContactPersonCountryCode { get; set; } = 61;  // Default to Australia
    public string? ContactPersonPhoneNumber { get; set; }
    public bool ContactPersonPhoneNumberIsMobile { get; set; } = false;
    public bool ContactPersonPhoneNumberIsWhatsApp { get; set; } = false;
    public string? ContactPersonEmail { get; set; }
    public string? ContactPersonJobTitle { get; set; }
    public string? BillingPersonFirstName { get; set; }
    public string? BillingPersonLastName { get; set; }
    public int BillingPersonCountryCode { get; set; } = 61;  // Default to Australia
    public string? BillingPersonPhoneNumber { get; set; }
    public bool BillingPersonPhoneNumberIsMobile { get; set; } = false;
    public bool BillingPersonPhoneNumberIsWhatsApp { get; set; } = false;
    public string? BillingPersonEmail { get; set; }
    public string? BillingPersonJobTitle { get; set; }
    public string? AdminPersonFirstName { get; set; }
    public string? AdminPersonLastName { get; set; }
    public int AdminPersonCountryCode { get; set; } = 61;  // Default to Australia
    public string? AdminPersonPhoneNumber { get; set; }
    public bool AdminPersonPhoneNumberIsMobile { get; set; } = false;
    public bool AdminPersonPhoneNumberIsWhatsApp { get; set; } = false;
    public string? AdminPersonEmail { get; set; }
    public string? AdminPersonJobTitle { get; set; }
}