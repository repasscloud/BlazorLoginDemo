namespace Cinturon360.Web.Security;

/// <summary>
/// Claims identity claim type constants used by the BFF session cookie.
/// </summary>
public static class ClaimTypes
{
    public const string UserId       = "c360:uid";
    public const string OrgId        = "c360:oid";
    public const string Email        = "c360:email";
    public const string DisplayName  = "c360:name";
    public const string UserCategory = "c360:cat";
    public const string PlatformRole = "c360:role";
    public const string AccessToken  = "c360:at";
    public const string RefreshToken = "c360:rt";
    public const string TokenExpiry  = "c360:exp";
    public const string SessionId    = "c360:sid";
    public const string Theme        = "c360:theme";
    public const string Language     = "c360:lang";
    public const string TimeZone     = "c360:tz";
}
