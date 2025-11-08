using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Cinturon360.Shared.Models.User;
using Cinturon360.Shared.Models.Static;
using Cinturon360.Shared.Validation;
using NanoidDotNet;
using Cinturon360.Shared.Models.Policies;

namespace Cinturon360.Shared.Models.Kernel.Client;

public class AvaClient
{
    [Key]
    public string Id { get; set; } = Nanoid.Generate();

    [Required]
    public required string CompanyName { get; set; }

    public TaxIdType TaxIdType { get; set; } = TaxIdType.None;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TaxId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? TaxLastValidated { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AddressLine1 { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AddressLine2 { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AddressLine3 { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? City { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? State { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PostalCode { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Country { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MailingAddressLine1 { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MailingAddressLine2 { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MailingAddressLine3 { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MailingCity { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MailingState { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MailingPostalCode { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MailingCountry { get; set; }

    // contact person
    [Required]
    public required string ContactPersonFirstName { get; set; }

    [Required]
    public required string ContactPersonLastName { get; set; }

    [Required]
    public required string ContactPersonCountryCode { get; set; }

    [Required]
    public required string ContactPersonPhone { get; set; }

    [Required]
    [EmailAddress]
    public required string ContactPersonEmail { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContactPersonJobTitle { get; set; }

    // billing person
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BillingPersonFirstName { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BillingPersonLastName { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BillingPersonCountryCode { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BillingPersonPhone { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BillingPersonEmail { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BillingPersonJobTitle { get; set; }

    // admin person
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AdminPersonFirstName { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AdminPersonLastName { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AdminPersonCountryCode { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AdminPersonPhone { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AdminPersonEmail { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AdminPersonJobTitle { get; set; }

    // financial
    [StringLength(3)]
    [CurrencyTypeValidation]
    public required string DefaultCurrency { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AvaClientLicenseId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LicenseAgreementId { get; set; }
    
    // policies
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DefaultTravelPolicyId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TravelPolicy? DefaultTravelPolicy { get; set; }
    
    // A company can have several travel policies
    public ICollection<TravelPolicy> TravelPolicies { get; set; } = new List<TravelPolicy>();

    // Navigation property for related users (optional)
    public ICollection<AvaUser> Users { get; set; } = new List<AvaUser>();
}