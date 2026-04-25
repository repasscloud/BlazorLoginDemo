namespace Cinturon360.Contracts.Auth;

/// <summary>Request to create a Personal Access Token.</summary>
public sealed record CreatePatRequest(
    string Name,
    DateTimeOffset? ExpiresAt = null,
    string? Scopes = null
);

/// <summary>
/// Response when a PAT is created. RawToken is shown ONCE — not stored on the server.
/// </summary>
public sealed record CreatePatResponse(
    string TokenId,
    string Name,
    string RawToken,
    string TokenPrefix,
    DateTimeOffset? ExpiresAt
);

/// <summary>Summary of a PAT (no raw token).</summary>
public sealed record PatSummary(
    string TokenId,
    string Name,
    string TokenPrefix,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? LastUsedAt,
    bool IsRevoked
);
