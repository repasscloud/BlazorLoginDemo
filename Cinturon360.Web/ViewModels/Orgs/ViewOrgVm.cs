using Cinturon360.Web.ViewModels.ReferenceData;

namespace Cinturon360.Web.ViewModels.Orgs;

public sealed record ViewOrgVm
{
    // Context
    public string Rid { get; init; } = default!;
    public string UserId { get; init; } = default!;

    // -------------------------
    // Core organization info
    // -------------------------
    public required string Id { get; init; }
    public required string OrgName { get; init; } = string.Empty;
    public required string OrgType { get; init; } = string.Empty;

    public bool IsActive { get; init; } = true;
    public required string ParentOrgId { get; init; } = string.Empty;

    public required string DefaultCurrencyCode { get; init; } = "AUD";

    public required string OrgClass { get; init; } = string.Empty;
    public required string OrgSector { get; init; } = string.Empty;
    public required string ServiceLevelAgreementTier { get; init; } = string.Empty;
    public required string TimeZoneId { get; init; } = string.Empty;

    // -------------------------
    // Taxation
    // -------------------------
    public required string TaxRegistrationType { get; init; } = string.Empty;
    public required string TaxRegistrationNumber { get; init; } = string.Empty;
    public required DateTime TaxLastValidated { get; init; } = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public required bool TaxValidationPassed { get; init; } = false;

    // -------------------------
    // Address
    // -------------------------
    public required string AddressLine1 { get; init; } = string.Empty;
    public required string AddressLine2 { get; init; } = string.Empty;
    public required string AddressLine3 { get; init; } = string.Empty;
    public required string City { get; init; } = string.Empty;
    public required string State { get; init; } = string.Empty;
    public required string PostalCode { get; init; } = string.Empty;
    public required string Country { get; init; } = string.Empty;

    public required string MailingAddressLine1 { get; init; } = string.Empty;
    public required string MailingAddressLine2 { get; init; } = string.Empty;
    public required string MailingAddressLine3 { get; init; } = string.Empty;
    public required string MailingCity { get; init; } = string.Empty;
    public required string MailingState { get; init; } = string.Empty;
    public required string MailingPostalCode { get; init; } = string.Empty;
    public required string MailingCountry { get; init; } = string.Empty;

    public required string ContactPersonFirstName { get; init; } = string.Empty;
    public required string ContactPersonLastName { get; init; } = string.Empty;
    public int ContactPersonCountryCode { get; init; } = 61; // Default to Australia
    public required string ContactPersonPhone { get; init; }
    public bool ContactPersonPhoneReceiveSMS { get; init; } = false;
    public bool ContactPersonPhoneReceiveWhatsApp { get; init; } = false;
    public required string ContactPersonEmail { get; init; } = string.Empty;
    public required string ContactPersonJobTitle { get; init; } = string.Empty;

    public required string BillingPersonFirstName { get; init; } = string.Empty;
    public required string BillingPersonLastName { get; init; } = string.Empty;
    public int BillingPersonCountryCode { get; init; } = 61; // Default to Australia
    public required string BillingPersonPhone { get; init; } = string.Empty;
    public bool BillingPersonPhoneReceiveSMS { get; init; } = false;
    public bool BillingPersonPhoneReceiveWhatsApp { get; init; } = false;
    public required string BillingPersonEmail { get; init; } = string.Empty;
    public required string BillingPersonJobTitle { get; init; } = string.Empty;

    public required string AdminPersonFirstName { get; init; } = string.Empty;
    public required string AdminPersonLastName { get; init; } = string.Empty;
    public int AdminPersonCountryCode { get; init; } = 61; // Default to Australia
    public required string AdminPersonPhone { get; init; } = string.Empty;
    public bool AdminPersonPhoneReceiveSMS { get; init; } = false;
    public bool AdminPersonPhoneReceiveWhatsApp { get; init; } = false;
    public required string AdminPersonEmail { get; init; } = string.Empty;
    public required string AdminPersonJobTitle { get; init; } = string.Empty;

    // -------------------------
    // additional info
    // -------------------------
    public required List<string> Domains { get; init; } = new();
    public required List<string> ChildOrgNames { get; init; } = new();
    public required List<string> TravelPolicyNames { get; init; } = new();
    public required List<string> ExpensePolicyNames { get; init; } = new();
    public required string LicenseAgreementId { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
    public DateTime LastUpdatedAtUtc { get; init; } = DateTime.UtcNow;
}
