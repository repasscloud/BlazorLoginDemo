using System.Security.Claims;

namespace Cinturon360.Api.Infrastructure;

public static class RequestContext
{
    public const string CorrelationHeader = "X-Correlation-Id";
    public const string TraceHeader = "X-Trace-Id";

    // HttpContext.Items keys (so filters/controllers can stamp extra info for logging)
    public const string ItemCorrelationId = "c360.correlation_id";
    public const string ItemTraceId = "c360.trace_id";
    public const string ItemClient = "c360.client";     // e.g. "web", "tui", "ios", "android"
    public const string ItemOrgId = "c360.org_id";
    public const string ItemUserId = "c360.user_id";

    public static string GetCorrelationId(HttpContext ctx)
        => (ctx.Items.TryGetValue(ItemCorrelationId, out var v) ? v?.ToString() : null)
           ?? ctx.TraceIdentifier;

    public static string GetTraceId()
        => Guid.NewGuid().ToString();
        //=> Activity.Current?.Id ?? string.Empty;

    public static string? GetClient(HttpContext ctx)
        => ctx.Items.TryGetValue(ItemClient, out var v) ? v?.ToString() : null;

    public static string? GetUserId(HttpContext ctx)
    {
        if (ctx.Items.TryGetValue(ItemUserId, out var v) && v is not null) return v.ToString();

        var user = ctx.User;
        if (user?.Identity?.IsAuthenticated != true) return null;

        return user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub")
            ?? user.FindFirstValue("uid");
    }

    public static string? GetOrgId(HttpContext ctx)
    {
        if (ctx.Items.TryGetValue(ItemOrgId, out var v) && v is not null) return v.ToString();

        var user = ctx.User;
        if (user?.Identity?.IsAuthenticated != true) return null;

        return user.FindFirstValue("org")
            ?? user.FindFirstValue("org_id")
            ?? user.FindFirstValue("tenant")
            ?? user.FindFirstValue("organizationId");
    }
}
