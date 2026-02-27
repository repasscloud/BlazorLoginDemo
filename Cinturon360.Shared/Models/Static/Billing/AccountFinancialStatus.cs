namespace Cinturon360.Shared.Models.Static.Billing;

public enum AccountFinancialStatus : int { PENDING = 0, CURRENT = 1, IN_BILLING_PERIOD = 2, OVERDUE = 3, SUSPENDED = 4, TERMINATED = 5 }

public static class AccountFinancialStatuses
{
    public sealed record Status(
        AccountFinancialStatus Type,
        string Code,
        string CodeLower,
        string Label,
        string Description
    );

    private static readonly IReadOnlyList<Status> _all =
    [
        new(
            AccountFinancialStatus.PENDING,
            "PENDING",
            "pending",
            "Pending",
            "Account is pending activation or initial configuration."
        ),
        new(
            AccountFinancialStatus.CURRENT,
            "CURRENT",
            "current",
            "Current",
            "Account is fully up to date with no unpaid invoices."
        ),
        new(
            AccountFinancialStatus.IN_BILLING_PERIOD,
            "IN_BILLING_PERIOD",
            "in_billing_period",
            "In Billing Period",
            "Invoice is unpaid but still within agreed billing terms."
        ),
        new(
            AccountFinancialStatus.OVERDUE,
            "OVERDUE",
            "overdue",
            "Overdue",
            "Invoice is past the due date and payment has not been received."
        ),
        new(
            AccountFinancialStatus.SUSPENDED,
            "SUSPENDED",
            "suspended",
            "Suspended",
            "Account access is restricted due to non-payment."
        ),
        new(
            AccountFinancialStatus.TERMINATED,
            "TERMINATED",
            "terminated",
            "Terminated",
            "Account has been formally terminated and requires reactivation."
        )
    ];

    public static IReadOnlyList<Status> All => _all;

    // UI projection (dropdown-safe)
    public static IReadOnlyList<(string Code, string Label)> DropdownOptions =>
        _all.Select(s => (s.Code, s.Label)).ToList();

    // Enum-safe lookup
    public static Status Get(AccountFinancialStatus type) =>
        _all.First(s => s.Type == type);

    // String-safe lookup (DB / API)
    public static Status? GetByCode(string code) =>
        _all.FirstOrDefault(s =>
            s.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

    // Helpers
    public static string ToCode(AccountFinancialStatus type) =>
        Get(type).Code;

    public static AccountFinancialStatus? ToEnum(string code) =>
        GetByCode(code)?.Type;
}
