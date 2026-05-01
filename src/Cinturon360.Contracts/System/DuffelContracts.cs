namespace Cinturon360.Contracts.System;

public sealed record UpsertDuffelOrgConfigurationRequest(
    bool IsEnabled,
    bool UseSandbox,
    string ApiBaseUrl,
    string ApiToken,
    string AccessScope,
    IReadOnlyList<string>? EnabledCapabilities,
    IReadOnlyList<string>? EnabledSearchFunctions,
    IReadOnlyList<string>? CorporateCodes,
    IReadOnlyList<string>? TourCodes,
    string? Notes
);

public sealed record DuffelOrgConfigurationResponse(
    string ConfigId,
    string OrgId,
    bool IsEnabled,
    bool UseSandbox,
    string ApiBaseUrl,
    string AccessScope,
    IReadOnlyList<string> EnabledCapabilities,
    IReadOnlyList<string> EnabledSearchFunctions,
    IReadOnlyList<string> CorporateCodes,
    IReadOnlyList<string> TourCodes,
    string? Notes,
    string? ApiTokenMasked,
    DateTimeOffset UpdatedAt,
    DateTimeOffset CreatedAt
);

public sealed record EffectiveDuffelConfigResponse(
    string OrgId,
    IReadOnlyList<DuffelOrgConfigurationResponse> Configurations
);
