namespace Cinturon360.Shared.Models.Static.Billing;

public enum BillingPeriodScope : int { PAYG = 0, MONTHLY = 1, QUARTERLY = 2, BI_ANNUAL = 3, ANNUAL = 4 }

public static class BillingPeriodScopes
{
    public sealed record Period(
        BillingPeriodScope Type, // enum-safe
        string Code,             // "MONTHLY"
        string CodeLower,        // "monthly"
        string Label,            // UI label
        string Description       // helper text / tooltip
    );

    // Single source of truth
    private static readonly IReadOnlyList<Period> _all =
    [
        new(
            BillingPeriodScope.PAYG,
            "PAYG",
            "payg",
            "Pay As You Go",
            "Charges are applied per transaction or usage event."
        ),
        new(
            BillingPeriodScope.MONTHLY,
            "MONTHLY",
            "monthly",
            "Monthly",
            "Billed once per calendar month."
        ),
        new(
            BillingPeriodScope.QUARTERLY,
            "QUARTERLY",
            "quarterly",
            "Quarterly",
            "Billed once every three months."
        ),
        new(
            BillingPeriodScope.BI_ANNUAL,
            "BI_ANNUAL",
            "bi_annual",
            "Bi-Annual",
            "Billed twice per year."
        ),
        new(
            BillingPeriodScope.ANNUAL,
            "ANNUAL",
            "annual",
            "Annual",
            "Billed once per year."
        )
    ];

    public static IReadOnlyList<Period> All => _all;

    // UI projection (dropdown-safe)
    public static IReadOnlyList<(string Code, string Label)> DropdownOptions =>
        _all.Select(p => (p.Code, p.Label)).ToList();

    // Enum-safe lookup
    public static Period Get(BillingPeriodScope type) =>
        _all.First(p => p.Type == type);

    // String-safe lookup (DB / API)
    public static Period? GetByCode(string code) =>
        _all.FirstOrDefault(p =>
            p.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

    // Helpers
    public static string ToCode(BillingPeriodScope type) =>
        Get(type).Code;

    public static BillingPeriodScope? ToEnum(string code) =>
        GetByCode(code)?.Type;
}
