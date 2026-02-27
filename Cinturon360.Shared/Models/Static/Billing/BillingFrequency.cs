namespace Cinturon360.Shared.Models.Static.Billing;

public enum BillingFrequency : int { PAYG = 0, MONTHLY = 30, QUARTERLY = 90, BI_ANNUALLY = 182, ANNUALLY = 365 }

public static class BillingFrequencies
{
    public sealed record Frequency(
        BillingFrequency Type, // enum-safe
        string Code,           // "MONTHLY"
        string CodeLower,      // "monthly"
        string Label,          // UI label
        int Days,              // cadence in days
        string Description     // helper text / tooltip
    );

    // Single source of truth
    private static readonly IReadOnlyList<Frequency> _all =
    [
        new(
            BillingFrequency.PAYG,
            "PAYG",
            "payg",
            "Pay As You Go",
            0,
            "Charges are applied per transaction or usage event."
        ),
        new(
            BillingFrequency.MONTHLY,
            "MONTHLY",
            "monthly",
            "Monthly",
            30,
            "Billed once per calendar month."
        ),
        new(
            BillingFrequency.QUARTERLY,
            "QUARTERLY",
            "quarterly",
            "Quarterly",
            90,
            "Billed once every three months."
        ),
        new(
            BillingFrequency.BI_ANNUALLY,
            "BI_ANNUALLY",
            "bi_annually",
            "Bi-Annually",
            182,
            "Billed twice per year."
        ),
        new(
            BillingFrequency.ANNUALLY,
            "ANNUALLY",
            "annually",
            "Annually",
            365,
            "Billed once per year."
        )
    ];

    public static IReadOnlyList<Frequency> All => _all;

    // UI projection (dropdown-safe)
    public static IReadOnlyList<(string Code, string Label)> DropdownOptions =>
        _all.Select(f => (f.Code, f.Label)).ToList();

    // Enum-safe lookup
    public static Frequency Get(BillingFrequency type) =>
        _all.First(f => f.Type == type);

    // String-safe lookup (DB / API)
    public static Frequency? GetByCode(string code) =>
        _all.FirstOrDefault(f =>
            f.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

    // Helpers
    public static string ToCode(BillingFrequency type) =>
        Get(type).Code;

    public static BillingFrequency? ToEnum(string code) =>
        GetByCode(code)?.Type;
}
