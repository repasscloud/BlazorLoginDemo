namespace Cinturon360.Contracts.Auth;

/// <summary>Returned on successful login or token refresh.</summary>
public sealed record AuthResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    string SessionId,
    UserInfoResponse User
);

/// <summary>Minimal user identity info included in auth responses.</summary>
public sealed record UserInfoResponse(
    string UserId,
    string Email,
    string FullName,
    string? OrgId,
    string? UserCategory,
    string? PlatformRole,
    IReadOnlyList<string> Permissions
);
