using System.ComponentModel.DataAnnotations;
using Cinturon360.Shared.Models.Static.Platform;
using Cinturon360.Shared.Models.Static.Identity;
using Cinturon360.Shared.Models.Static.Geography;
using Cinturon360.Shared.Models.Static.Support;

namespace Cinturon360.Web.Drafts.Platform.Org;

public sealed class NewOrgDraft
{
    // -------------------------
    // Core organization info
    // -------------------------

    [Required, StringLength(100)]
    public string OrgName { get; set; } = string.Empty;

    public OrganizationType OrgType { get; set; } = OrganizationType.Client;

    [MaxLength(25)]
    public string? ParentOrgId { get; set; }

    public bool IsActive { get; set; } = true;
    
    [Required, RegularExpression(@"^[A-Z]{3}$",
        ErrorMessage = "Currency must be exactly 3 uppercase letters.")]
    public string DefaultCurrencyCode { get; set; } = "AUD";

    public OrganizationClassification OrgClass { get; set; } = OrganizationClassification.Unknown;
    public OrganizationSector OrgSector { get; set; } = OrganizationSector.Unknown;
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
    public PassportCountry Country { get; set; } = PassportCountry.AUS;

    public string? MailingAddressLine1 { get; set; }
    public string? MailingAddressLine2 { get; set; }
    public string? MailingAddressLine3 { get; set; }
    public string? MailingCity { get; set; }
    public string? MailingState { get; set; }
    public string? MailingPostalCode { get; set; }
    public PassportCountry MailingCountry { get; set; } = PassportCountry.AUS;

    public string? ContactPersonFirstName { get; set; }
    public string? ContactPersonLastName { get; set; }
    public CountryDialingCode ContactPersonCountryCode { get; set; } = CountryDialingCode.Australia;
    public string? ContactPersonPhone { get; set; }
    public bool ContactPersonPhoneReceiveSMS { get; set; } = false;
    public bool ContactPersonPhoneReceiveWhatsApp { get; set; } = false;
    [EmailAddress] public string? ContactPersonEmail { get; set; }
    public string? ContactPersonJobTitle { get; set; }

    public string? BillingPersonFirstName { get; set; }
    public string? BillingPersonLastName { get; set; }
    public CountryDialingCode BillingPersonCountryCode { get; set; } = CountryDialingCode.Australia;
    public string? BillingPersonPhone { get; set; }
    public bool BillingPersonPhoneReceiveSMS { get; set; } = false;
    public bool BillingPersonPhoneReceiveWhatsApp { get; set; } = false;
    [EmailAddress] public string? BillingPersonEmail { get; set; }
    public string? BillingPersonJobTitle { get; set; }

    public string? AdminPersonFirstName { get; set; }
    public string? AdminPersonLastName { get; set; }
    public CountryDialingCode AdminPersonCountryCode { get; set; } = CountryDialingCode.Australia;
    public string? AdminPersonPhone { get; set; }
    public bool AdminPersonPhoneReceiveSMS { get; set; } = false;
    public bool AdminPersonPhoneReceiveWhatsApp { get; set; } = false;
    [EmailAddress] public string? AdminPersonEmail { get; set; }
    public string? AdminPersonJobTitle { get; set; }
}
