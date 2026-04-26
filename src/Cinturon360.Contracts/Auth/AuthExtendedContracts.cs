namespace Cinturon360.Contracts.Auth;

/// <summary>Token refresh request.</summary>
public sealed record RefreshRequest(string RefreshToken, string SessionId);

/// <summary>Extended login response that includes MFA pending state and refresh token.</summary>
public sealed record LoginResponse(
    bool RequiresMfa,
    string? AccessToken,
    string? RefreshToken,
    DateTimeOffset? ExpiresAt,
    UserInfoResponse? User,
    string? SessionId,
    string? MfaMethod);

/// <summary>MFA challenge verification request.</summary>
public sealed record MfaChallengeRequest(
    string SessionId,
    string MfaCode,
    string MfaMethod);

/// <summary>MFA challenge response — still-pending or resolved.</summary>
public sealed record MfaChallengeResponse(
    bool Succeeded,
    string? AccessToken,
    string? RefreshToken,
    DateTimeOffset? ExpiresAt,
    string? ErrorMessage);

/// <summary>Forgot password request.</summary>
public sealed record ForgotPasswordRequest(string Email);

/// <summary>Password reset confirmation request.</summary>
public sealed record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword,
    string ConfirmPassword);
