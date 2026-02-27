namespace Cinturon360.Shared.Models.Static.Billing;

public enum PaymentStatus : int { PENDING = 0, PROCESSING = 1, AUTHORIZED = 2, PARTIALLY_PAID = 3, PAID = 4, FAILED = 5, OVERDUE = 6, DISPUTED = 7, REFUNDED = 8, PARTIALLY_REFUNDED = 9, CANCELLED = 10, WRITTEN_OFF = 11, CHARGEBACK = 12, VOIDED = 13, SCHEDULED = 14, ON_HOLD = 15 }

public static class PaymentStatuses
{
    public sealed record Status(
        PaymentStatus Type,
        string Code,
        string CodeLower,
        string Label,
        string Description
    );

    private static readonly IReadOnlyList<Status> _all =
    [
        new(PaymentStatus.PENDING,              "PENDING",              "pending",              "Pending",              "Payment has been created but not yet actioned."),
        new(PaymentStatus.PROCESSING,           "PROCESSING",           "processing",           "Processing",           "Payment is currently being processed."),
        new(PaymentStatus.AUTHORIZED,           "AUTHORIZED",           "authorized",           "Authorized",           "Funds have been authorized but not yet captured."),
        new(PaymentStatus.PARTIALLY_PAID,       "PARTIALLY_PAID",       "partially_paid",       "Partially Paid",       "Payment has been partially settled."),
        new(PaymentStatus.PAID,                 "PAID",                 "paid",                 "Paid",                 "Payment has been fully settled."),
        new(PaymentStatus.FAILED,               "FAILED",               "failed",               "Failed",               "Payment attempt failed."),
        new(PaymentStatus.OVERDUE,              "OVERDUE",              "overdue",              "Overdue",              "Payment was not received by the due date."),
        new(PaymentStatus.DISPUTED,             "DISPUTED",             "disputed",             "Disputed",             "Payment is under dispute."),
        new(PaymentStatus.REFUNDED,             "REFUNDED",             "refunded",             "Refunded",             "Payment has been fully refunded."),
        new(PaymentStatus.PARTIALLY_REFUNDED,   "PARTIALLY_REFUNDED",   "partially_refunded",   "Partially Refunded",   "Payment has been partially refunded."),
        new(PaymentStatus.CANCELLED,            "CANCELLED",            "cancelled",            "Cancelled",            "Payment was cancelled before completion."),
        new(PaymentStatus.WRITTEN_OFF,           "WRITTEN_OFF",           "written_off",           "Written Off",           "Payment balance was written off."),
        new(PaymentStatus.CHARGEBACK,            "CHARGEBACK",            "chargeback",            "Chargeback",            "Payment was reversed via chargeback."),
        new(PaymentStatus.VOIDED,                "VOIDED",                "voided",                "Voided",                "Payment authorization was voided."),
        new(PaymentStatus.SCHEDULED,             "SCHEDULED",             "scheduled",             "Scheduled",             "Payment is scheduled for a future date."),
        new(PaymentStatus.ON_HOLD,               "ON_HOLD",               "on_hold",               "On Hold",               "Payment is temporarily on hold.")
    ];

    public static IReadOnlyList<Status> All => _all;

    // UI projection (dropdown-safe)
    public static IReadOnlyList<(string Code, string Label)> DropdownOptions =>
        _all.Select(s => (s.Code, s.Label)).ToList();

    // Enum-safe lookup
    public static Status Get(PaymentStatus type) =>
        _all.First(s => s.Type == type);

    // String-safe lookup (DB / API)
    public static Status? GetByCode(string code) =>
        _all.FirstOrDefault(s =>
            s.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

    // Helpers
    public static string ToCode(PaymentStatus type) =>
        Get(type).Code;

    public static PaymentStatus? ToEnum(string code) =>
        GetByCode(code)?.Type;
}
