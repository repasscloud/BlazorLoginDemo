using Cinturon360.Domain.Common.Base;
using Cinturon360.Domain.Enums.Travel;

namespace Cinturon360.Domain.Entities.Travel;

/// <summary>
/// Organisation-level Duffel provider configuration.
/// This is intended for TMC organisations and can optionally be shared chain-wide.
/// </summary>
public sealed class DuffelOrgConfiguration : Entity
{
    public string OrgId { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; } = true;
    public bool UseSandbox { get; private set; }
    public string ApiBaseUrl { get; private set; } = "https://api.duffel.com";
    public string ApiToken { get; private set; } = string.Empty;
    public DuffelAccessScope AccessScope { get; private set; } = DuffelAccessScope.TmcOnly;

    // Comma-separated values to keep schema flexible while product integrations are phased in.
    public string EnabledCapabilitiesCsv { get; private set; } = string.Empty;
    public string EnabledSearchFunctionsCsv { get; private set; } = string.Empty;
    public string CorporateCodesCsv { get; private set; } = string.Empty;
    public string TourCodesCsv { get; private set; } = string.Empty;

    public string? Notes { get; private set; }
    public string? UpdatedByUserId { get; private set; }

    private DuffelOrgConfiguration() { }

    public static DuffelOrgConfiguration Create(
        string id,
        string orgId,
        bool isEnabled,
        bool useSandbox,
        string apiBaseUrl,
        string apiToken,
        DuffelAccessScope accessScope,
        string enabledCapabilitiesCsv,
        string enabledSearchFunctionsCsv,
        string corporateCodesCsv,
        string tourCodesCsv,
        string? notes,
        string? updatedByUserId)
        => new()
        {
            Id = id,
            OrgId = orgId,
            IsEnabled = isEnabled,
            UseSandbox = useSandbox,
            ApiBaseUrl = string.IsNullOrWhiteSpace(apiBaseUrl) ? "https://api.duffel.com" : apiBaseUrl.Trim(),
            ApiToken = apiToken.Trim(),
            AccessScope = accessScope,
            EnabledCapabilitiesCsv = enabledCapabilitiesCsv,
            EnabledSearchFunctionsCsv = enabledSearchFunctionsCsv,
            CorporateCodesCsv = corporateCodesCsv,
            TourCodesCsv = tourCodesCsv,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            UpdatedByUserId = string.IsNullOrWhiteSpace(updatedByUserId) ? null : updatedByUserId.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void Update(
        bool isEnabled,
        bool useSandbox,
        string apiBaseUrl,
        string apiToken,
        DuffelAccessScope accessScope,
        string enabledCapabilitiesCsv,
        string enabledSearchFunctionsCsv,
        string corporateCodesCsv,
        string tourCodesCsv,
        string? notes,
        string? updatedByUserId)
    {
        IsEnabled = isEnabled;
        UseSandbox = useSandbox;
        ApiBaseUrl = string.IsNullOrWhiteSpace(apiBaseUrl) ? "https://api.duffel.com" : apiBaseUrl.Trim();
        ApiToken = apiToken.Trim();
        AccessScope = accessScope;
        EnabledCapabilitiesCsv = enabledCapabilitiesCsv;
        EnabledSearchFunctionsCsv = enabledSearchFunctionsCsv;
        CorporateCodesCsv = corporateCodesCsv;
        TourCodesCsv = tourCodesCsv;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        UpdatedByUserId = string.IsNullOrWhiteSpace(updatedByUserId) ? null : updatedByUserId.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
