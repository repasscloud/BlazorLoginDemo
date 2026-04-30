namespace Cinturon360.Common.IdGeneration;

/// <summary>
/// Domain entity type prefixes for human-readable IDs.
/// Format: {prefix}_{NanoId}  e.g. usr_abc123xyz
/// </summary>
public static class IdPrefix
{
    // ── Identity ──────────────────────────────────────────────────────────
    public const string User             = "usr";
    public const string Organization     = "org";
    public const string Role             = "rol";
    public const string RoleAssignment   = "rla";
    public const string ServiceAccount   = "svc";
    public const string ApiToken         = "tok";
    public const string Session          = "ses";
    public const string AuditEvent       = "aud";

    // ── Traveller profile ─────────────────────────────────────────────────
    public const string TravellerProfile = "tvl";
    public const string LoyaltyProgram   = "loy";
    public const string EmergencyContact = "ec";
    public const string Address          = "adr";
    public const string Preferences      = "prf";

    // ── Geography ─────────────────────────────────────────────────────────
    public const string Country          = "cty";
    public const string City             = "cit";
    public const string Airport          = "apt";

    // ── Bookings / Quotes ─────────────────────────────────────────────────
    public const string Booking          = "bkg";
    public const string BookingItem      = "bki";
    public const string Quote            = "qte";
    public const string Ticket           = "tkt";

    // ── Policy ────────────────────────────────────────────────────────────
    public const string Policy           = "pol";
    public const string PolicyRule       = "plr";
    public const string PolicyAssignment = "pla";

    // ── Approvals ─────────────────────────────────────────────────────────
    public const string Approval         = "apr";
    public const string ApprovalDecision = "apd";

    // ── Billing ───────────────────────────────────────────────────────────
    public const string License          = "lic";
    public const string BillingConfig    = "bcf";
    public const string Invoice          = "inv";
    public const string Payment          = "pay";
    public const string PrepaidBalance   = "ppb";

    // ── Documents / Jobs ──────────────────────────────────────────────────
    public const string Document         = "doc";
    public const string Job              = "job";

    // ── Exchange Rates ────────────────────────────────────────────────────
    public const string ExchangeRate     = "fxr";

    // ── Support Ticketing ─────────────────────────────────────────────────
    public const string SupportTicket    = "stk";
    public const string TicketComment    = "tkc";
    public const string TicketEscalation = "tke";
    public const string TicketAttachment = "tka";
}
