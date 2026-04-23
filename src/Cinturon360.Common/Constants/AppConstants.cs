namespace Cinturon360.Common.Constants;

public static class AppConstants
{
    public const string AppName = "Cinturon360";
    public const string ApiVersion = "v1";
    public const string DefaultCulture = "en-US";
    public const string DefaultTimeZone = "UTC";
}

public static class ClaimTypes
{
    public const string UserId      = "c360:user_id";
    public const string OrgId       = "c360:org_id";
    public const string OrgRole     = "c360:org_role";
    public const string AppRole     = "c360:app_role";
    public const string TenantId    = "c360:tenant_id";
    public const string ServiceAccount = "c360:service_account";
}

public static class PolicyNames
{
    public const string RequireAuthenticated  = "RequireAuthenticated";
    public const string RequireSudo           = "RequireSudo";
    public const string RequireVendor         = "RequireVendor";
    public const string RequireTmc            = "RequireTmc";
    public const string RequireClient         = "RequireClient";
    public const string RequireGlobalAdmin    = "RequireGlobalAdmin";
}

public static class Roles
{
    // Platform-level roles (AppRole claim)
    public const string GlobalAdmin   = "global_admin";
    public const string Support       = "support";
    public const string Finance       = "finance";
    public const string Integrations  = "integrations";

    // Org-level roles (OrgRole claim, scoped to org)
    public const string OrgAdmin      = "org_admin";
    public const string Approver      = "approver";
    public const string Booker        = "booker";
    public const string Traveller     = "traveller";
    public const string ReadOnly      = "read_only";
}
