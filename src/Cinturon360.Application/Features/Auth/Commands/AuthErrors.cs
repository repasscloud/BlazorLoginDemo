using Cinturon360.Common.Results;

namespace Cinturon360.Application.Features.Auth.Commands;

/// <summary>
/// Domain-level error codes for auth failures.
/// </summary>
public static class AuthErrors
{
    public static readonly Error InvalidCredentials      = new("auth.invalid_credentials",      "Invalid email or password.");
    public static readonly Error AccountLocked            = new("auth.account_locked",            "Account is temporarily locked.");
    public static readonly Error AccountSuspended         = new("auth.account_suspended",         "Account is suspended.");
    public static readonly Error MfaRequired              = new("auth.mfa_required",              "MFA verification is required.");
    public static readonly Error MfaInvalid               = new("auth.mfa_invalid",               "MFA code is invalid or expired.");
    public static readonly Error SessionNotFound          = new("auth.session_not_found",         "Session not found or already revoked.");
    public static readonly Error TokenNotFound            = new("auth.token_not_found",           "Token not found.");
    public static readonly Error Unauthorized             = new("auth.unauthorized",              "Unauthorized.");
    public static readonly Error InvalidResetToken        = new("auth.invalid_reset_token",       "Password reset link is invalid or has expired.");
    public static readonly Error EmailAlreadyRegistered   = new("auth.email_already_registered",  "An account with that email already exists.");
    public static readonly Error WeakPassword             = new("auth.weak_password",             "Password does not meet the minimum requirements.");
}
