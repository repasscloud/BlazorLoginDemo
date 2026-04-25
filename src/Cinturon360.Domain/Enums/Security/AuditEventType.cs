namespace Cinturon360.Domain.Enums.Security;

/// <summary>
/// Categories of security and auth audit events recorded in UserAuditEvent.
/// </summary>
public enum AuditEventType
{
    // Auth lifecycle
    LoginSuccess         = 100,
    LoginFailed          = 101,
    LoginLockedOut       = 102,
    Logout               = 103,
    SessionExpired       = 104,
    SessionRevoked       = 105,

    // MFA
    MfaChallenged        = 200,
    MfaPassed            = 201,
    MfaFailed            = 202,
    MfaMethodAdded       = 203,
    MfaMethodRemoved     = 204,

    // Tokens
    PatCreated           = 300,
    PatRevoked           = 301,
    PatExpired           = 302,
    ServiceTokenCreated  = 303,
    ServiceTokenRevoked  = 304,

    // Account state
    AccountCreated       = 400,
    AccountLocked        = 401,
    AccountUnlocked      = 402,
    AccountSuspended     = 403,
    AccountReactivated   = 404,
    EmailVerified        = 405,
    PasswordChanged      = 406,
    PasswordReset        = 407,

    // SSO / provisioning
    SsoLinked            = 500,
    SsoUnlinked          = 501,
    JitProvisioned       = 502,
    ScimProvisioned      = 503,
    ScimDeprovision      = 504,

    // Role / permission changes
    RoleAssigned         = 600,
    RoleRevoked          = 601,
    PermissionOverride   = 602,

    // QR device linking
    QrLinkInitiated      = 700,
    QrLinkCompleted      = 701,
    QrLinkRevoked        = 702
}
