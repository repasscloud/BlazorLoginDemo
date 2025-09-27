using System.ComponentModel.DataAnnotations;
using BlazorLoginDemo.Shared.Models.Static.Platform; // OrganizationType
using BlazorLoginDemo.Shared.Models.Kernel.Billing;
using System.ComponentModel; // enums: BillingType, BillingFrequency, PaymentMethod, ServiceFeeType, PaymentStatus
using System.Text.Json.Serialization; // JSON: avoid self-referencing cycles on navs

namespace BlazorLoginDemo.Shared.Models.Kernel.Platform;

// ------------------------------
// MERGED ORGANIZATION (Org + AvaClient)
// ------------------------------
public sealed class OrganizationUnified
{
    [Key]
    [MaxLength(64)]
    public string Id { get; set; } = NanoidDotNet.Nanoid.Generate();

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
    public string TaxIdType { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public DateTime TaxLastValidated { get; set; } = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    // Physical address
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = string.Empty;

    // Mailing address
    public string? MailingAddressLine1 { get; set; }
    public string? MailingAddressLine2 { get; set; }
    public string? MailingAddressLine3 { get; set; }
    public string? MailingCity { get; set; }
    public string? MailingState { get; set; }
    public string? MailingPostalCode { get; set; }
    public string MailingCountry { get; set; } = string.Empty;

    // Primary contacts
    // General/Commercial Contact
    public string? ContactPersonFirstName { get; set; }
    public string? ContactPersonLastName { get; set; }
    [DefaultValue("")] public string ContactPersonCountryCode { get; set; } = string.Empty;
    public string? ContactPersonPhone { get; set; }
    [EmailAddress] public string? ContactPersonEmail { get; set; }
    public string? ContactPersonJobTitle { get; set; }

    // Billing Contact
    public string? BillingPersonFirstName { get; set; }
    public string? BillingPersonLastName { get; set; }
    [DefaultValue("")] public string BillingPersonCountryCode { get; set; } = string.Empty;
    public string? BillingPersonPhone { get; set; }
    [EmailAddress] public string? BillingPersonEmail { get; set; }
    public string? BillingPersonJobTitle { get; set; }

    // Admin/Technical Contact
    public string? AdminPersonFirstName { get; set; }
    public string? AdminPersonLastName { get; set; }
    [DefaultValue("")] public string AdminPersonCountryCode { get; set; } = string.Empty;
    public string? AdminPersonPhone { get; set; }
    [EmailAddress] public string? AdminPersonEmail { get; set; }
    public string? AdminPersonJobTitle { get; set; }

    // ------------------------------
    // Policies
    // ------------------------------
    public ICollection<TravelPolicy> TravelPolicies { get; set; } = new List<TravelPolicy>();
    public ICollection<ExpensePolicy> ExpensePolicies { get; set; } = new List<ExpensePolicy>();

    // ------------------------------
    // Billing / Licensing (1:1)
    // ------------------------------
    public string? LicenseAgreementId { get; set; }

    [JsonIgnore] // break Org ↔ License self-referencing loop during JSON serialization
    public LicenseAgreementUnified? LicenseAgreement { get; set; }

    public DateTime CreatedAt { get; private set; }  // set by DB only
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
}
