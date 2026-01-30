using System.ComponentModel.DataAnnotations;
using Cinturon360.Shared.Models.Static.Platform; // OrganizationType
using Cinturon360.Shared.Models.Kernel.Billing;
using System.ComponentModel; // enums: BillingType, BillingFrequency, PaymentMethod, ServiceFeeType, PaymentStatus
using System.Text.Json.Serialization;
using Cinturon360.Shared.Models.Policies;
using Cinturon360.Shared.Helpers;
using Cinturon360.Shared.Models.Static.Identity;
using Cinturon360.Shared.Models.Static.Geography; // JSON: avoid self-referencing cycles on navs

namespace Cinturon360.Shared.Models.Kernel.Platform;

// ------------------------------
// MERGED ORGANIZATION (Org + AvaClient)
// ------------------------------
public sealed class OrganizationUnified
{
    [Key]
    [MaxLength(25)]
    public string Id { get; private set; } = IDGeneratorHelper.GenerateId(IdGenType.Organization);

    [Required, MaxLength(128)]
    public required string Name { get; set; }

    // Tier in the hierarchy
    [Required]
    public OrganizationType Type { get; set; }

    // Parent/child links
    public string? ParentOrganizationId { get; set; }
    public OrganizationUnified? Parent { get; set; }
    public ICollection<OrganizationUnified> Children { get; set; } = new List<OrganizationUnified>();

    public bool IsActive { get; set; } = true;

    // Tenant domains (login / discovery)
    public ICollection<OrganizationDomainUnified> Domains { get; set; } = new List<OrganizationDomainUnified>();

    // ------------------------------
    // Contact & Company Info (from AvaClient)
    // ------------------------------
    [MaxLength(3)][DefaultValue("AUD")] public string DefaultCurrency { get; set; } = "AUD";

    // Company registered details
    public TaxIdType TaxIdType { get; set; } = TaxIdType.None;
    public string? TaxId { get; set; }
    public DateTime TaxLastValidated { get; set; } = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    // Physical address
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public PassportCountry Country { get; set; } = PassportCountry.AUS;

    // Mailing address
    public string? MailingAddressLine1 { get; set; }
    public string? MailingAddressLine2 { get; set; }
    public string? MailingAddressLine3 { get; set; }
    public string? MailingCity { get; set; }
    public string? MailingState { get; set; }
    public string? MailingPostalCode { get; set; }
    public PassportCountry MailingCountry { get; set; } = PassportCountry.AUS;

    // Primary contacts
    // General/Commercial Contact
    public string? ContactPersonFirstName { get; set; }
    public string? ContactPersonLastName { get; set; }
    public CountryDialingCode ContactPersonCountryCode { get; set; } = CountryDialingCode.Australia;
    public string? ContactPersonPhone { get; set; }
    [EmailAddress] public string? ContactPersonEmail { get; set; }
    public string? ContactPersonJobTitle { get; set; }

    // Billing Contact
    public string? BillingPersonFirstName { get; set; }
    public string? BillingPersonLastName { get; set; }
    public CountryDialingCode BillingPersonCountryCode { get; set; } = CountryDialingCode.Australia;
    public string? BillingPersonPhone { get; set; }
    [EmailAddress] public string? BillingPersonEmail { get; set; }
    public string? BillingPersonJobTitle { get; set; }

    // Admin/Technical Contact
    public string? AdminPersonFirstName { get; set; }
    public string? AdminPersonLastName { get; set; }
    public CountryDialingCode AdminPersonCountryCode { get; set; } = CountryDialingCode.Australia;
    public string? AdminPersonPhone { get; set; }
    [EmailAddress] public string? AdminPersonEmail { get; set; }
    public string? AdminPersonJobTitle { get; set; }

    // ------------------------------
    // Policies
    // ------------------------------
    public string? DefaultTravelPolicyId { get; set; }
    public string? DefaultExpensePolicyId { get; set; }
    public ICollection<TravelPolicy> TravelPolicies { get; set; } = new List<TravelPolicy>();
    public ICollection<ExpensePolicy> ExpensePolicies { get; set; } = new List<ExpensePolicy>();

    // ------------------------------
    // Billing / Licensing (1:1)
    // ------------------------------
    [MaxLength(18)]
    public string? LicenseAgreementId { get; set; }

    [JsonIgnore] // break Org ↔ License self-referencing loop during JSON serialization
    public LicenseAgreementUnified? LicenseAgreement { get; set; }

    public DateTime CreatedAt { get; private set; }  // set by DB only
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
}
