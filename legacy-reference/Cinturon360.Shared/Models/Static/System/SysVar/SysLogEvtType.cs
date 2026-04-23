namespace Cinturon360.Shared.Models.Static.System.SysVar;

/// <summary>
/// Canonical EVT catalogue for <c>C360SystemLog.Evt</c>.
/// 
/// - Upper snake case
/// - Stable meaning over time
/// - No identifiers in EVT (put ids in Ent/EntId/Note/Path)
/// 
/// Storage: keep DB column as string; persist with <c>evt.ToString()</c>.
/// </summary>
public enum SysLogEvtType : int
{
    // -------------------------
    // Generic / fallback
    // -------------------------
    /// <summary>Temporary placeholder event code. Replace with a specific EVT when stabilised.</summary>
    GENERIC = 1,

    // -------------------------
    // UI traceability / intent
    // -------------------------
    /// <summary>User navigated to a route/page (once per navigation).</summary>
    UI_PAGE_OPEN = 1000,
    /// <summary>User left a route/page (optional; only if you can reliably detect).</summary>
    UI_PAGE_LEAVE = 1001,
    /// <summary>User opened a tab/sub-route within a page.</summary>
    UI_TAB_OPEN = 1002,
    /// <summary>User opened a modal/dialog.</summary>
    UI_MODAL_OPEN = 1003,
    /// <summary>User closed a modal/dialog.</summary>
    UI_MODAL_CLOSE = 1004,
    /// <summary>User enabled edit mode for a section or record.</summary>
    UI_EDIT_MODE_ENABLED = 1010,
    /// <summary>User disabled/cancelled edit mode for a section or record.</summary>
    UI_EDIT_MODE_DISABLED = 1011,
    /// <summary>User pressed Save (intent, not outcome).</summary>
    UI_SAVE_PRESSED = 1020,
    /// <summary>User pressed Cancel/Back (intent).</summary>
    UI_CANCEL_PRESSED = 1021,
    /// <summary>User pressed Delete (intent).</summary>
    UI_DELETE_PRESSED = 1022,
    /// <summary>User pressed Submit (intent).</summary>
    UI_SUBMIT_PRESSED = 1023,
    /// <summary>User pressed Approve (intent).</summary>
    UI_APPROVE_PRESSED = 1024,
    /// <summary>User pressed Reject (intent).</summary>
    UI_REJECT_PRESSED = 1025,
    /// <summary>User initiated an export/download.</summary>
    UI_EXPORT_PRESSED = 1030,
    /// <summary>User initiated an import/upload workflow.</summary>
    UI_IMPORT_PRESSED = 1031,
    /// <summary>User submitted a search (not keystrokes).</summary>
    UI_SEARCH_SUBMITTED = 1040,
    /// <summary>User applied a filter (not every change; only apply action).</summary>
    UI_FILTER_APPLIED = 1041,
    /// <summary>User applied a sort.</summary>
    UI_SORT_APPLIED = 1042,
    /// <summary>User changed pagination page/pageSize.</summary>
    UI_PAGINATION_CHANGED = 1043,
    /// <summary>User executed a non-CRUD function (e.g., TestConnection, Reprice).</summary>
    UI_FUNCTION_EXECUTED = 1050,
    /// <summary>UI navigation blocked due to permissions (pair with SEC_PERMISSION_DENY).</summary>
    UI_NAV_DENIED = 1060,
    /// <summary>UI action blocked due to permissions (pair with SEC_PERMISSION_DENY).</summary>
    UI_ACTION_DENIED = 1061,
    /// <summary>UI restored an existing session/state after reconnect.</summary>
    UI_SESSION_RESUMED = 1070,
    /// <summary>UI lost session/connectivity (Blazor circuit disconnect etc.).</summary>
    UI_SESSION_LOST = 1071,

    // -------------------------
    // API lifecycle
    // -------------------------
    /// <summary>API request received (optional; prefer END if you log once).</summary>
    API_REQ_START = 2000,
    /// <summary>API request completed (standard).</summary>
    API_REQ_END = 2001,
    /// <summary>Unhandled exception during request processing.</summary>
    API_REQ_ERR = 2002,
    /// <summary>Request validation failed (400/422).</summary>
    API_VALIDATION_FAIL = 2010,
    /// <summary>Missing auth credential (401).</summary>
    API_AUTH_MISSING = 2020,
    /// <summary>Invalid auth credential (401).</summary>
    API_AUTH_INVALID = 2021,
    /// <summary>Authenticated but not authorised (403).</summary>
    API_PERMISSION_DENY = 2022,
    /// <summary>Request rate-limited (429).</summary>
    API_RATE_LIMIT = 2023,
    /// <summary>Concurrency / conflict outcome (409).</summary>
    API_CONFLICT = 2024,
    /// <summary>Endpoint/entity not found (404).</summary>
    API_NOT_FOUND = 2025,
    /// <summary>Webhook payload received (Stripe/Duffel/etc.).</summary>
    API_WEBHOOK_RECEIVED = 2030,
    /// <summary>Webhook verified/signature OK.</summary>
    API_WEBHOOK_VERIFIED = 2031,
    /// <summary>Webhook rejected (bad signature / replay / deny).</summary>
    API_WEBHOOK_REJECTED = 2032,
    /// <summary>Idempotency key replay detected/handled.</summary>
    API_IDEMPOTENCY_REPLAY = 2033,

    /// <summary>HTTP 200 OK response sent.</summary>
    API_HTTP_200_OK = 2100,

    /// <summary>HTTP 201 Created response sent.</summary>
    API_HTTP_201_CREATED = 2101,

    /// <summary>HTTP 204 No Content response sent.</summary>
    API_HTTP_204_NO_CONTENT = 2102,

    /// <summary>HTTP 400 Bad Request response sent.</summary>
    API_HTTP_400_BAD_REQUEST = 2110,

    /// <summary>HTTP 401 Unauthorized response sent.</summary>
    API_HTTP_401_UNAUTHORIZED = 2120,

    /// <summary>HTTP 403 Forbidden response sent.</summary>
    API_HTTP_403_FORBIDDEN = 2121,

    /// <summary>HTTP 404 Not Found response sent.</summary>
    API_HTTP_404_NOT_FOUND = 2122,

    /// <summary>HTTP 409 Conflict response sent.</summary>
    API_HTTP_409_CONFLICT = 2123,

    /// <summary>HTTP 422 Unprocessable Entity response sent.</summary>
    API_HTTP_422_UNPROCESSABLE_ENTITY = 2130,

    /// <summary>HTTP 429 Too Many Requests response sent.</summary>
    API_HTTP_429_TOO_MANY_REQUESTS = 2140,

    /// <summary>HTTP 500 Internal Server Error response sent.</summary>
    API_HTTP_500_INTERNAL_SERVER_ERROR = 2150,

    /// <summary>HTTP 503 Service Unavailable response sent.</summary>
    API_HTTP_503_SERVICE_UNAVAILABLE = 2151,

    /// <summary>HTTP 504 Gateway Timeout response sent.</summary>
    API_HTTP_504_GATEWAY_TIMEOUT = 2152,

    /// <summary>HTTP response sent outside 2xx range (generic).</summary>
    API_HTTP_NO_2XX = 2199,
    // -------------------------
    // Security / access control
    // -------------------------
    /// <summary>Login attempt.</summary>
    SEC_LOGIN_ATTEMPT = 3000,
    /// <summary>Login succeeded.</summary>
    SEC_LOGIN_OK = 3001,
    /// <summary>Login failed.</summary>
    SEC_LOGIN_FAIL = 3002,
    /// <summary>Logout.</summary>
    SEC_LOGOUT = 3003,
    /// <summary>Session established.</summary>
    SEC_SESSION_START = 3010,
    /// <summary>Session ended/expired.</summary>
    SEC_SESSION_END = 3011,
    /// <summary>MFA challenge issued.</summary>
    SEC_MFA_CHALLENGE = 3020,
    /// <summary>MFA succeeded.</summary>
    SEC_MFA_OK = 3021,
    /// <summary>MFA failed.</summary>
    SEC_MFA_FAIL = 3022,
    /// <summary>Password reset requested.</summary>
    SEC_PASSWORD_RESET_REQUEST = 3030,
    /// <summary>Password reset succeeded.</summary>
    SEC_PASSWORD_RESET_OK = 3031,
    /// <summary>Password reset failed.</summary>
    SEC_PASSWORD_RESET_FAIL = 3032,
    /// <summary>Authorisation denied at policy/guard level.</summary>
    SEC_PERMISSION_DENY = 3040,
    /// <summary>Privileged user started impersonation.</summary>
    SEC_IMPERSONATION_START = 3050,
    /// <summary>Privileged user ended impersonation.</summary>
    SEC_IMPERSONATION_END = 3051,
    /// <summary>Role granted to principal.</summary>
    SEC_ROLE_ASSIGNED = 3060,
    /// <summary>Role revoked from principal.</summary>
    SEC_ROLE_REVOKED = 3061,
    /// <summary>API key created/issued.</summary>
    SEC_API_KEY_CREATED = 3070,
    /// <summary>API key revoked.</summary>
    SEC_API_KEY_REVOKED = 3071,
    /// <summary>Call denied by IP allowlist.</summary>
    SEC_IP_ALLOWLIST_DENY = 3080,
    /// <summary>Audit data exported (admin action).</summary>
    SEC_AUDIT_EXPORT = 3090,

    // -------------------------
    // Data layer
    // -------------------------
    /// <summary>DB transaction began (optional).</summary>
    DATA_TX_BEGIN = 4000,
    /// <summary>DB transaction committed (optional).</summary>
    DATA_TX_COMMIT = 4001,
    /// <summary>DB transaction rolled back (optional).</summary>
    DATA_TX_ROLLBACK = 4002,
    /// <summary>Entity/collection created in database (standard for high-value writes).</summary>
    DATA_CREATE = 4005,
    /// <summary>Entity/collection read from database (standard for high-value reads).</summary>
    DATA_READ = 4006,
    /// <summary>Entity/changes saved successfully (high-value writes only).</summary>
    DATA_UPDATE = 4007,
    /// <summary>Entity/collection deleted from database.</summary>
    DATA_DELETE = 4008,
    /// <summary>Entity/changes read error.</summary>
    DATA_READ_ERR = 4009,

    /// <summary>Entity/collection create failed.</summary>
    DATA_CREATE_ERR = 4010,

    /// <summary>Entity/changes update failed.</summary>
    DATA_UPDATE_ERR = 4011,

    /// <summary>Entity/changes saved successfully.</summary>
    DATA_SAVE_OK = 4010,
    /// <summary>Entity/changes save failed.</summary>
    DATA_SAVE_ERR = 4011,
    /// <summary>Optimistic concurrency conflict.</summary>
    DATA_CONCURRENCY_CONFLICT = 4020,
    /// <summary>Constraint violation / integrity error.</summary>
    DATA_INTEGRITY_VIOLATION = 4021,
    /// <summary>Slow query detected (budget exceeded).</summary>
    DATA_QUERY_SLOW = 4030,
    /// <summary>Migration start.</summary>
    DATA_MIGRATION_START = 4040,
    /// <summary>Migration end.</summary>
    DATA_MIGRATION_END = 4041,
    /// <summary>Outbox message enqueued.</summary>
    DATA_OUTBOX_ENQUEUED = 4050,
    /// <summary>Outbox message dispatched.</summary>
    DATA_OUTBOX_DISPATCHED = 4051,

    // -------------------------
    // Integrations / providers
    // -------------------------
    /// <summary>Outbound provider/API call started (standard).</summary>
    INT_CALL_START = 5000,
    /// <summary>Outbound provider/API call ended (standard).</summary>
    INT_CALL_END = 5001,
    /// <summary>Outbound provider/API call failed (non-timeout).</summary>
    INT_ERR = 5002,
    /// <summary>Outbound provider/API call timed out.</summary>
    INT_TIMEOUT = 5003,
    /// <summary>Retry attempt for outbound call.</summary>
    INT_RETRY = 5004,
    /// <summary>Provider rate limit encountered.</summary>
    INT_RATE_LIMIT = 5005,
    /// <summary>Provider auth token refreshed/renewed.</summary>
    INT_AUTH_REFRESH = 5010,
    /// <summary>Provider payload mapping/deserialisation failed.</summary>
    INT_AUTH_FAIL = 5011,
    /// <summary>Provider response payload mapping/serialisation failed.</summary>
    INT_OAUTH_TOKEN_FAIL = 5012,
    /// <summary>Provider integration setup/configuration failed.</summary>
    INT_MAP_FAIL = 5020,
    /// <summary>Provider schema mismatch/contract drift detected.</summary>
    INT_SCHEMA_MISMATCH = 5021,
    /// <summary>Circuit breaker opened for provider.</summary>
    INT_CIRCUIT_OPEN = 5030,
    /// <summary>Circuit breaker half-open for provider.</summary>
    INT_CIRCUIT_HALFOPEN = 5031,
    /// <summary>Circuit breaker closed (recovered).</summary>
    INT_CIRCUIT_CLOSED = 5032,

    // -------------------------
    // Workflows (multi-step)
    // -------------------------
    /// <summary>Workflow started (multi-step business flow).</summary>
    WF_FLOW_START = 6000,
    /// <summary>Workflow step started.</summary>
    WF_STEP_START = 6001,
    /// <summary>Workflow step ended.</summary>
    WF_STEP_END = 6002,
    /// <summary>Workflow ended successfully.</summary>
    WF_FLOW_END = 6003,
    /// <summary>Workflow failed.</summary>
    WF_FLOW_FAIL = 6004,
    /// <summary>Workflow step executed (generic; for when you don't want start/end).</summary>
    WF_FLOW_STEP = 6005,
    /// <summary>Workflow warning.</summary>
    WF_FLOW_WARN = 6006,
    /// <summary>Workflow compensation started.</summary>
    WF_COMPENSATE_START = 6010,
    /// <summary>Workflow compensation ended.</summary>
    WF_COMPENSATE_END = 6011,
    /// <summary>Workflow compensation failed.</summary>
    WF_COMPENSATE_FAIL = 6012,

    // -------------------------
    // Background jobs / schedulers
    // -------------------------
    /// <summary>Background job started (standard).</summary>
    AUTO_JOB_START = 7000,
    /// <summary>Background job completed (standard).</summary>
    AUTO_JOB_END = 7001,
    /// <summary>Background job failed.</summary>
    AUTO_JOB_FAIL = 7002,
    /// <summary>Background job retrying.</summary>
    AUTO_JOB_RETRY = 7003,
    /// <summary>Background job cancelled.</summary>
    AUTO_JOB_CANCEL = 7004,
    /// <summary>Scheduler tick/dispatch.</summary>
    AUTO_SCHEDULE_TICK = 7010,
    /// <summary>Message moved to dead-letter.</summary>
    AUTO_DEADLETTER = 7020,

    // -------------------------
    // System / host lifecycle
    // -------------------------
    /// <summary>Service startup complete.</summary>
    SYS_STARTUP = 8000,
    /// <summary>Service shutdown.</summary>
    SYS_SHUTDOWN = 8001,
    /// <summary>Health check OK.</summary>
    SYS_HEALTH_OK = 8010,
    /// <summary>Health check FAIL.</summary>
    SYS_HEALTH_FAIL = 8011,
    /// <summary>Configuration loaded/validated.</summary>
    SYS_CONFIG_LOADED = 8020,
    /// <summary>Configuration reloaded at runtime.</summary>
    SYS_CONFIG_RELOAD = 8021,
    /// <summary>Feature flag state changed.</summary>
    SYS_FEATUREFLAG_CHANGED = 8030,
    /// <summary>Cache warmed.</summary>
    SYS_CACHE_WARM = 8040,
    /// <summary>Cache evicted/cleared.</summary>
    SYS_CACHE_EVICT = 8041,
    /// <summary>Rate limit config reloaded.</summary>
    SYS_RATE_LIMIT_CONFIG_RELOAD = 8050,
    /// <summary>Application version/build info logged (once at start).</summary>
    SYS_DEPLOY_VERSION = 8060,

    // -------------------------
    // Domain: identity & tenancy
    // -------------------------
    /// <summary>Organisation created.</summary>
    ORG_CREATE = 10000,
    /// <summary>Organisation updated.</summary>
    ORG_UPDATE = 10001,
    /// <summary>Organisation disabled/suspended.</summary>
    ORG_DISABLE = 10002,
    /// <summary>Organisation enabled/unsuspended.</summary>
    ORG_ENABLE = 10003,
    /// <summary>Organisation domain added.</summary>
    ORG_DOMAIN_ADD = 10010,
    /// <summary>Organisation domain removed.</summary>
    ORG_DOMAIN_REMOVE = 10011,
    /// <summary>Licence issued/assigned to organisation.</summary>
    ORG_LICENSE_ISSUE = 10020,
    /// <summary>Licence revoked from organisation.</summary>
    ORG_LICENSE_REVOKE = 10021,
    /// <summary>User created.</summary>
    USER_CREATE = 10100,
    /// <summary>User updated.</summary>
    USER_UPDATE = 10101,
    /// <summary>User disabled.</summary>
    USER_DISABLE = 10102,
    /// <summary>User enabled.</summary>
    USER_ENABLE = 10103,
    /// <summary>User invite sent.</summary>
    USER_INVITE_SENT = 10110,
    /// <summary>User invite accepted.</summary>
    USER_INVITE_ACCEPTED = 10111,
    /// <summary>User changed password.</summary>
    USER_PASSWORD_CHANGED = 10120,
    /// <summary>User changed email/identity.</summary>
    USER_EMAIL_CHANGED = 10121,

    // -------------------------
    // Domain: policy & approval
    // -------------------------
    /// <summary>Travel policy created.</summary>
    TRAVEL_POLICY_CREATE = 11000,
    /// <summary>Travel policy updated.</summary>
    TRAVEL_POLICY_UPDATE = 11001,
    /// <summary>Travel policy published/activated.</summary>
    TRAVEL_POLICY_PUBLISH = 11002,
    /// <summary>Travel policy archived/retired.</summary>
    TRAVEL_POLICY_ARCHIVE = 11003,
    /// <summary>Expense policy created.</summary>
    EXPENSE_POLICY_CREATE = 11010,
    /// <summary>Expense policy updated.</summary>
    EXPENSE_POLICY_UPDATE = 11011,
    /// <summary>Expense policy published/activated.</summary>
    EXPENSE_POLICY_PUBLISH = 11012,
    /// <summary>Expense policy archived/retired.</summary>
    EXPENSE_POLICY_ARCHIVE = 11013,
    /// <summary>Policy evaluation passed.</summary>
    POLICY_EVAL_PASS = 11020,
    /// <summary>Policy evaluation failed (out-of-policy).</summary>
    POLICY_EVAL_FAIL = 11021,
    /// <summary>Approval requested (pre-booking / exception).</summary>
    APPROVAL_REQUESTED = 11030,
    /// <summary>Approval granted.</summary>
    APPROVAL_GRANTED = 11031,
    /// <summary>Approval rejected.</summary>
    APPROVAL_REJECTED = 11032,
    /// <summary>Approval expired.</summary>
    APPROVAL_EXPIRED = 11033,

    // -------------------------
    // Domain: shopping & quoting
    // -------------------------
    /// <summary>Flight/hotel/car offer search started.</summary>
    OFFER_SEARCH_START = 12000,
    /// <summary>Offer search completed.</summary>
    OFFER_SEARCH_END = 12001,
    /// <summary>Offer search failed.</summary>
    OFFER_SEARCH_ERR = 12002,
    /// <summary>User/system selected an offer for quoting.</summary>
    OFFER_SELECTED = 12010,
    /// <summary>Quote created.</summary>
    QUOTE_CREATE = 12020,
    /// <summary>Quote updated (non-pricing).</summary>
    QUOTE_UPDATE = 12021,
    /// <summary>Quote reprice started.</summary>
    QUOTE_REPRICE_START = 12022,
    /// <summary>Quote reprice completed.</summary>
    QUOTE_REPRICE_END = 12023,
    /// <summary>Quote reprice failed.</summary>
    QUOTE_REPRICE_ERR = 12024,
    /// <summary>Quote expired/invalidated.</summary>
    QUOTE_EXPIRE = 12030,

    // -------------------------
    // Domain: booking lifecycle
    // -------------------------
    /// <summary>Booking create started.</summary>
    BOOKING_CREATE_START = 13000,
    /// <summary>Booking create completed.</summary>
    BOOKING_CREATE_END = 13001,
    /// <summary>Booking create failed.</summary>
    BOOKING_CREATE_ERR = 13002,
    /// <summary>Booking confirmation/commit started.</summary>
    BOOKING_CONFIRM_START = 13010,
    /// <summary>Booking confirmation/commit completed.</summary>
    BOOKING_CONFIRM_END = 13011,
    /// <summary>Booking confirmation/commit failed.</summary>
    BOOKING_CONFIRM_ERR = 13012,
    /// <summary>Booking cancellation started.</summary>
    BOOKING_CANCEL_START = 13020,
    /// <summary>Booking cancellation completed.</summary>
    BOOKING_CANCEL_END = 13021,
    /// <summary>Booking cancellation failed.</summary>
    BOOKING_CANCEL_ERR = 13022,
    /// <summary>Booking change started.</summary>
    BOOKING_CHANGE_START = 13030,
    /// <summary>Booking change completed.</summary>
    BOOKING_CHANGE_END = 13031,
    /// <summary>Booking change failed.</summary>
    BOOKING_CHANGE_ERR = 13032,

    // -------------------------
    // Domain: ticketing lifecycle
    // -------------------------
    /// <summary>Ticket issuance started.</summary>
    TICKET_ISSUE_START = 14000,
    /// <summary>Ticket issuance completed.</summary>
    TICKET_ISSUE_END = 14001,
    /// <summary>Ticket issuance failed.</summary>
    TICKET_ISSUE_ERR = 14002,
    /// <summary>Ticket void started.</summary>
    TICKET_VOID_START = 14010,
    /// <summary>Ticket void completed.</summary>
    TICKET_VOID_END = 14011,
    /// <summary>Ticket void failed.</summary>
    TICKET_VOID_ERR = 14012,
    /// <summary>Ticket refund started.</summary>
    TICKET_REFUND_START = 14020,
    /// <summary>Ticket refund completed.</summary>
    TICKET_REFUND_END = 14021,
    /// <summary>Ticket refund failed.</summary>
    TICKET_REFUND_ERR = 14022,
    /// <summary>Ticket reissue started.</summary>
    TICKET_REISSUE_START = 14030,
    /// <summary>Ticket reissue completed.</summary>
    TICKET_REISSUE_END = 14031,
    /// <summary>Ticket reissue failed.</summary>
    TICKET_REISSUE_ERR = 14032,

    // -------------------------
    // Domain: payments & billing
    // -------------------------
    /// <summary>Payment intent created (Stripe or internal abstraction).</summary>
    PAYMENT_INTENT_CREATE = 15000,
    /// <summary>Payment authorisation started.</summary>
    PAYMENT_AUTHORISE_START = 15001,
    /// <summary>Payment authorisation completed.</summary>
    PAYMENT_AUTHORISE_END = 15002,
    /// <summary>Payment capture started.</summary>
    PAYMENT_CAPTURE_START = 15010,
    /// <summary>Payment capture completed.</summary>
    PAYMENT_CAPTURE_END = 15011,
    /// <summary>Payment failed (decline/insufficient funds/etc.).</summary>
    PAYMENT_FAILED = 15012,
    /// <summary>Refund started.</summary>
    PAYMENT_REFUND_START = 15020,
    /// <summary>Refund completed.</summary>
    PAYMENT_REFUND_END = 15021,
    /// <summary>Refund failed.</summary>
    PAYMENT_REFUND_ERR = 15022,
    /// <summary>Dispute opened.</summary>
    PAYMENT_DISPUTE_OPENED = 15030,
    /// <summary>Dispute won.</summary>
    PAYMENT_DISPUTE_WON = 15031,
    /// <summary>Dispute lost.</summary>
    PAYMENT_DISPUTE_LOST = 15032,
    /// <summary>Invoice created.</summary>
    INVOICE_CREATE = 15100,
    /// <summary>Invoice finalised.</summary>
    INVOICE_FINALISE = 15101,
    /// <summary>Invoice sent/issued.</summary>
    INVOICE_SENT = 15102,
    /// <summary>Invoice marked paid.</summary>
    INVOICE_PAID = 15103,
    /// <summary>Invoice voided/cancelled.</summary>
    INVOICE_VOID = 15104,
    /// <summary>Credit note created.</summary>
    CREDIT_NOTE_CREATE = 15110,
    /// <summary>Credit note issued.</summary>
    CREDIT_NOTE_ISSUED = 15111,
    /// <summary>Settlement reconciliation started.</summary>
    SETTLEMENT_RECONCILE_START = 15200,
    /// <summary>Settlement reconciliation completed.</summary>
    SETTLEMENT_RECONCILE_END = 15201,
    /// <summary>Settlement reconciliation mismatch detected.</summary>
    SETTLEMENT_RECONCILE_MISMATCH = 15202,

    // -------------------------
    // Domain: notifications
    // -------------------------
    /// <summary>Email notification send started.</summary>
    NOTIF_EMAIL_SEND_START = 16000,
    /// <summary>Email notification send completed.</summary>
    NOTIF_EMAIL_SEND_END = 16001,
    /// <summary>Email notification send failed.</summary>
    NOTIF_EMAIL_SEND_ERR = 16002,
    /// <summary>Outbound webhook dispatch started.</summary>
    NOTIF_WEBHOOK_DISPATCH_START = 16010,
    /// <summary>Outbound webhook dispatch completed.</summary>
    NOTIF_WEBHOOK_DISPATCH_END = 16011,
    /// <summary>Outbound webhook dispatch failed.</summary>
    NOTIF_WEBHOOK_DISPATCH_ERR = 16012,
    /// <summary>In-app notification created.</summary>
    NOTIF_INAPP_CREATE = 16020,

    // -------------------------
    // Domain: reporting & import/export
    // -------------------------
    /// <summary>Report run started.</summary>
    REPORT_RUN_START = 17000,
    /// <summary>Report run completed.</summary>
    REPORT_RUN_END = 17001,
    /// <summary>Report run failed.</summary>
    REPORT_RUN_ERR = 17002,
    /// <summary>Export started.</summary>
    EXPORT_START = 17010,
    /// <summary>Export completed.</summary>
    EXPORT_END = 17011,
    /// <summary>Export failed.</summary>
    EXPORT_ERR = 17012,
    /// <summary>Import started.</summary>
    IMPORT_START = 17020,
    /// <summary>Import completed.</summary>
    IMPORT_END = 17021,
    /// <summary>Import failed.</summary>
    IMPORT_ERR = 17022,


    // -------------------------
    // Queue / messaging lifecycle
    // -------------------------

    /// <summary>Queue/topic/subscription created/provisioned.</summary>
    QUEUE_CREATE = 18000,

    /// <summary>Queue/topic/subscription deleted/deprovisioned.</summary>
    QUEUE_DELETE = 18001,

    /// <summary>Queue/topic/subscription configured/updated (visibility timeout, DLQ settings, etc.).</summary>
    QUEUE_CONFIG_UPDATE = 18002,

    /// <summary>Queue became unavailable / unhealthy.</summary>
    QUEUE_UNAVAILABLE = 18003,

    /// <summary>Queue recovered / became healthy again.</summary>
    QUEUE_AVAILABLE = 18004,

    /// <summary>Message enqueued/published into the queue.</summary>
    QUEUE_MSG_ENQUEUE = 18010,

    /// <summary>Message dequeued/received for processing.</summary>
    QUEUE_MSG_DEQUEUE = 18011,

    /// <summary>Message acknowledged/committed as processed.</summary>
    QUEUE_MSG_ACK = 18012,

    /// <summary>Message processing failed (handler threw / non-transient error).</summary>
    QUEUE_MSG_FAIL = 18013,

    /// <summary>Message abandoned/nacked (will be made visible again).</summary>
    QUEUE_MSG_NACK = 18014,

    /// <summary>Message retry scheduled/attempted (transient error path).</summary>
    QUEUE_MSG_RETRY = 18015,

    /// <summary>Message moved to dead-letter queue (exhausted retries / poison message).</summary>
    QUEUE_MSG_DEADLETTER = 18016,

    /// <summary>Message permanently dropped (only if you explicitly drop).</summary>
    QUEUE_MSG_DROP = 18017,

    /// <summary>Message processing cancelled (shutdown / token cancelled).</summary>
    QUEUE_MSG_CANCEL = 18018,

    /// <summary>Duplicate message detected (idempotency hit).</summary>
    QUEUE_MSG_DUPLICATE = 18019,

}

public static class SysLogEvtTypeExtensions
{
    /// <summary>Returns the persisted EVT code (e.g. "API_REQ_END").</summary>
    public static string Code(this SysLogEvtType evt) => evt.ToString();

    /// <summary>Parse a stored EVT string back to <see cref="SysLogEvtType"/> when possible.</summary>
    public static bool TryParse(string? code, out SysLogEvtType evt)
        => Enum.TryParse(code ?? string.Empty, ignoreCase: false, out evt);
}