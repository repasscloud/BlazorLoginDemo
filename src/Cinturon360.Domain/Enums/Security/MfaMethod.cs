namespace Cinturon360.Domain.Enums.Security;

/// <summary>
/// Multi-factor authentication method types.
/// </summary>
public enum MfaMethod
{
    Totp     = 1,
    Sms      = 2,
    EmailOtp = 3,
    Push     = 4
}
