namespace Cinturon360.Common.Constants;

/// <summary>
/// Authoritative list of permission codes used in RolePermission and evaluated in the permission engine.
/// Format: {domain}.{action} or {domain}.{sub-domain}.{action}
/// </summary>
public static class PermissionCodes
{
    // ── Bookings ─────────────────────────────────────────────────────────
    public const string BookingsRead      = "bookings.read";
    public const string BookingsCreate    = "bookings.create";
    public const string BookingsManage    = "bookings.manage";
    public const string BookingsCancel    = "bookings.cancel";

    // ── Quotes ───────────────────────────────────────────────────────────
    public const string QuotesRead        = "quotes.read";
    public const string QuotesCreate      = "quotes.create";

    // ── Approvals ────────────────────────────────────────────────────────
    public const string ApprovalsRead     = "approvals.read";
    public const string ApprovalsManage   = "approvals.manage";
    public const string ApprovalsApprove  = "approvals.approve";

    // ── Policy ───────────────────────────────────────────────────────────
    public const string PolicyRead        = "policy.read";
    public const string PolicyManage      = "policy.manage";

    // ── Users ────────────────────────────────────────────────────────────
    public const string UsersRead         = "users.read";
    public const string UsersManage       = "users.manage";
    public const string UsersInvite       = "users.invite";

    // ── Roles ────────────────────────────────────────────────────────────
    public const string RolesRead         = "roles.read";
    public const string RolesManage       = "roles.manage";

    // ── Reports ──────────────────────────────────────────────────────────
    public const string ReportsRead       = "reports.read";
    public const string ReportsFinance    = "reports.finance.read";
    public const string ReportsOperations = "reports.operations.read";

    // ── Billing ──────────────────────────────────────────────────────────
    public const string BillingRead       = "billing.read";
    public const string BillingManage     = "billing.manage";

    // ── Org management ───────────────────────────────────────────────────
    public const string OrgRead           = "org.read";
    public const string OrgManage         = "org.manage";

    // ── Ticketing ────────────────────────────────────────────────────────
    public const string TicketingRead     = "ticketing.read";
    public const string TicketingCreate   = "ticketing.create";
    public const string TicketingManage   = "ticketing.manage";

    // ── Storage ──────────────────────────────────────────────────────────
    public const string StorageRead       = "storage.read";
    public const string StorageUpload     = "storage.upload";

    // ── Platform (sudo only) ─────────────────────────────────────────────
    public const string PlatformAdmin     = "platform.admin";
    public const string PlatformAudit     = "platform.audit";
    public const string PlatformOps       = "platform.ops";
}
