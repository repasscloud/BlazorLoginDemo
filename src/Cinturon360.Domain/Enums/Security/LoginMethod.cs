namespace Cinturon360.Domain.Enums.Security;

/// <summary>
/// The authentication method used for a UserAuthMethod record.
/// </summary>
public enum LoginMethod
{
    LocalPassword = 1,
    Google        = 2,
    Microsoft     = 3,
    Apple         = 4,
    Facebook      = 5,
    Oidc          = 6,
    Saml          = 7,
    QrDeviceLink  = 8
}
