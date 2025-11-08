namespace Cinturon360.Shared.Auth;

public static class AppClaimTypes
{
    public const string OrgId = "org_id";
    public const string OrgType = "org_type";  // [Sudo]|Vendor|Tmc|Client
    public const string UserCategory = "user_category";  // [Sudo]|PlatformAdmin|TmcUser|ClientUser
    public const string TmcId = "tmc_id";
    public const string ClientId = "client_id";
}