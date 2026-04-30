using Cinturon360.Domain.Common.Base;
using Cinturon360.Domain.Enums.System;

namespace Cinturon360.Domain.Entities.Organization;

/// <summary>
/// A tenant organisation. Can be a Vendor, TMC, or Client type.
/// All data, roles, and billing is scoped by OrgId.
/// Hierarchy: Vendor → TMC → Client. Access never flows upward or sideways.
/// </summary>
public sealed class Organisation : SoftDeletableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public OrgType OrgType { get; private set; }
    public string? ParentOrgId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? LogoStorageKey { get; private set; }

    // Contact info
    public string? PrimaryEmail { get; private set; }
    public string? PrimaryPhone { get; private set; }
    public string? Website { get; private set; }

    // Locale
    public string LanguageCode { get; private set; } = "en";
    public string TimeZone { get; private set; } = "UTC";
    public string CurrencyCode { get; private set; } = "USD";

    // Metadata
    public string? ExternalRef { get; private set; }

    // TMC group / chain codes (applies to OrgType.Tmc only)
    // A chain groups multiple TMC locations under one identity (e.g. Flight Centre Travel Group).
    // ChainCode is assigned by the Vendor; BranchCode identifies an individual location.
    public string? ChainCode { get; private set; }
    public string? ChainName { get; private set; }
    public string? BranchCode { get; private set; }

    /// <summary>
    /// The name shown as the author on support ticket replies from this org's support staff.
    /// Falls back to "Support Team" when null or empty.
    /// e.g. "GPS Support", "Acme Travel Support", "Sunrise Travels Help Desk"
    /// </summary>
    public string? SupportTeamName { get; private set; }

    /// <summary>
    /// Template code used for support-ticket update emails for users in this org.
    /// The system resolves language-specific variants from this code.
    /// </summary>
    public string? SupportTicketEmailTemplateCode { get; private set; }

    private Organisation() { }

    public static Organisation Create(
        string id,
        string name,
        string slug,
        OrgType orgType,
        string? parentOrgId = null,
        string? primaryEmail = null,
        string languageCode = "en",
        string timeZone = "UTC",
        string currencyCode = "USD")
    {
        return new Organisation
        {
            Id = id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Name = name.Trim(),
            Slug = slug.ToLowerInvariant().Trim(),
            OrgType = orgType,
            ParentOrgId = parentOrgId,
            IsActive = true,
            PrimaryEmail = primaryEmail,
            LanguageCode = languageCode,
            TimeZone = timeZone,
            CurrencyCode = currencyCode
        };
    }

    public void UpdateDetails(string name, string? primaryEmail, string? primaryPhone, string? website)
    {
        Name = name.Trim();
        PrimaryEmail = primaryEmail;
        PrimaryPhone = primaryPhone;
        Website = website;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateLocale(string languageCode, string timeZone, string currencyCode)
    {
        LanguageCode = languageCode;
        TimeZone = timeZone;
        CurrencyCode = currencyCode;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetLogo(string storageKey)
    {
        LogoStorageKey = storageKey;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetExternalRef(string externalRef)
    {
        ExternalRef = externalRef;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Set or update the TMC chain/group code and branch code.
    /// ChainCode + ChainName identify the group (assigned by the Vendor).
    /// BranchCode identifies this specific TMC location within the chain.
    /// </summary>
    public void SetChainCode(string? chainCode, string? chainName, string? branchCode)
    {
        ChainCode = chainCode;
        ChainName = chainName;
        BranchCode = branchCode;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Set or clear the support team display name for ticket replies.
    /// Pass null to revert to the default "Support Team" label.
    /// </summary>
    public void SetSupportTeamName(string? name)
    {
        SupportTeamName = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        UpdatedAt       = DateTimeOffset.UtcNow;
    }

    public void SetSupportTicketEmailTemplateCode(string? code)
    {
        SupportTicketEmailTemplateCode = string.IsNullOrWhiteSpace(code) ? null : code.Trim();
        UpdatedAt                      = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reactivate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
