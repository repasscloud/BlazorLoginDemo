using System.ComponentModel.DataAnnotations;
using Cinturon360.Shared.Models.Static.Billing;
using Cinturon360.Shared.Models.Static.Organization;
using static Cinturon360.Shared.Models.Static.Organization.OrganizationClassifications;
using static Cinturon360.Shared.Models.Static.Organization.OrganizationTypes;
using static Cinturon360.Shared.Models.Static.Support.SlaTiers;

namespace Cinturon360.Web.Drafts.Platform.Org;

public sealed class NewOrgDraft
{
    // -------------------------
    // Core organization info
    // -------------------------
    [Required(ErrorMessage = "Organization name is required.")]
    [StringLength(
        100,
        MinimumLength = 3,
        ErrorMessage = "Organization name must be between 3 and 100 characters."
    )]
    public string OrgName { get; set; } = string.Empty;

    public OrganizationType OrgType { get; set; } = OrganizationType.Client;

    public bool IsActive { get; set; } = true;
    [StringLength(
        25,
        MinimumLength = 25,
        ErrorMessage = "Parent Org Id must be exactly 25 characters."
    )]
    public string? ParentOrgId { get; set; }

    [Required, RegularExpression(@"^[A-Z]{3}$",
        ErrorMessage = "Currency must be exactly 3 uppercase letters.")]
    public string DefaultCurrencyCode { get; set; } = "AUD";

    public OrganizationClassification OrgClass { get; set; } = OrganizationClassification.Unknown;
    public OrganizationSectors.OrganizationSector OrgSector { get; set; } = OrganizationSectors.OrganizationSector.Unknown;
    public SlaTier ServiceLevelAgreementTier { get; set; } = SlaTier.Basic;
    public string TimeZoneId { get; set; } = TimeZoneInfo.Utc.Id.ToString();

    public TaxationType TaxRegistrationType { get; set; } = TaxationType.None;
    [MaxLength(50)] public string? TaxRegistrationNumber { get; set; }
    // public DateTime TaxLastValidated { get; set; } = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    // -------------------------
    // Address
    // -------------------------
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public int CountryId { get; set; } = 14; // Default to Australia

    public string? MailingAddressLine1 { get; set; }
    public string? MailingAddressLine2 { get; set; }
    public string? MailingAddressLine3 { get; set; }
    public string? MailingCity { get; set; }
    public string? MailingState { get; set; }
    public string? MailingPostalCode { get; set; }
    public int MailingCountryId { get; set; } = 14; // Default to Australia

    public string? ContactPersonFirstName { get; set; }
    public string? ContactPersonLastName { get; set; }
    public int ContactPersonCountryCode { get; set; } = 61; // Default to Australia
    public string? ContactPersonPhone { get; set; }
    public bool ContactPersonPhoneReceiveSMS { get; set; } = false;
    public bool ContactPersonPhoneReceiveWhatsApp { get; set; } = false;
    [EmailAddress] public string? ContactPersonEmail { get; set; }
    public string? ContactPersonJobTitle { get; set; }

    public string? BillingPersonFirstName { get; set; }
    public string? BillingPersonLastName { get; set; }
    public int BillingPersonCountryCode { get; set; } = 61; // Default to Australia
    public string? BillingPersonPhone { get; set; }
    public bool BillingPersonPhoneReceiveSMS { get; set; } = false;
    public bool BillingPersonPhoneReceiveWhatsApp { get; set; } = false;
    [EmailAddress] public string? BillingPersonEmail { get; set; }
    public string? BillingPersonJobTitle { get; set; }

    public string? AdminPersonFirstName { get; set; }
    public string? AdminPersonLastName { get; set; }
    public int AdminPersonCountryCode { get; set; } = 61; // Default to Australia
    public string? AdminPersonPhone { get; set; }
    public bool AdminPersonPhoneReceiveSMS { get; set; } = false;
    public bool AdminPersonPhoneReceiveWhatsApp { get; set; } = false;
    [EmailAddress] public string? AdminPersonEmail { get; set; }
    public string? AdminPersonJobTitle { get; set; }
}
