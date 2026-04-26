using System.Security.Claims;

namespace Cinturon360.Web.Services.Navigation;

/// <summary>
/// Determines which navigation items and route subtrees are visible
/// for the current user based on their category and platform role.
/// This is a UI hint only — authorization must still be enforced at the page/API level.
/// </summary>
public sealed class NavigationPermissionService
{
    public static bool CanAccessSudo(ClaimsPrincipal principal)
        => principal.FindFirstValue(Security.ClaimTypes.UserCategory) == "Platform";

    public static bool CanAccessVendor(ClaimsPrincipal principal)
    {
        var cat = principal.FindFirstValue(Security.ClaimTypes.UserCategory);
        return cat is "Vendor" or "Platform";
    }

    public static bool CanAccessTmc(ClaimsPrincipal principal)
    {
        var cat = principal.FindFirstValue(Security.ClaimTypes.UserCategory);
        return cat is "Tmc" or "Vendor" or "Platform";
    }

    public static bool CanAccessClient(ClaimsPrincipal principal)
    {
        var cat = principal.FindFirstValue(Security.ClaimTypes.UserCategory);
        return cat is "Client" or "Tmc" or "Vendor" or "Platform";
    }

    public static string GetDefaultRouteForUser(ClaimsPrincipal principal)
    {
        var cat = principal.FindFirstValue(Security.ClaimTypes.UserCategory);
        return cat switch
        {
            "Platform" => "/sudo",
            "Vendor"   => "/vendor",
            "Tmc"      => "/tmc",
            _          => "/client",
        };
    }
}
