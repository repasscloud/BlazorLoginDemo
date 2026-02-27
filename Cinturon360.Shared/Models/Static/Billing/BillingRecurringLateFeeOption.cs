namespace Cinturon360.Shared.Models.Static.Billing;

public enum RecurringLateFeeOption : int { NONE = 0, DAILY = 1, WEEKLY = 2 }

public static class RecurringLateFeeOptions
{
    public sealed record LateFeeOption(
        RecurringLateFeeOption Type,
        string Code,
        string CodeLower,
        string Label,
        string Description
    );

    private static readonly IReadOnlyList<LateFeeOption> _all =
    [
        new(
            RecurringLateFeeOption.NONE,
            "NONE",
            "none",
            "None",
            "No recurring late fees are applied."
        ),
        new(
            RecurringLateFeeOption.DAILY,
            "DAILY",
            "daily",
            "Daily",
            "A late fee is applied for each day the payment remains overdue."
        ),
        new(
            RecurringLateFeeOption.WEEKLY,
            "WEEKLY",
            "weekly",
            "Weekly",
            "A late fee is applied once per week while the payment remains overdue."
        )
    ];

    public static IReadOnlyList<LateFeeOption> All => _all;

    // UI projection (dropdown-safe)
    public static IReadOnlyList<(string Code, string Label)> DropdownOptions =>
        _all.Select(o => (o.Code, o.Label)).ToList();

    // Enum-safe lookup
    public static LateFeeOption Get(RecurringLateFeeOption type) =>
        _all.First(o => o.Type == type);

    // String-safe lookup (DB / API)
    public static LateFeeOption? GetByCode(string code) =>
        _all.FirstOrDefault(o =>
            o.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

    // Helpers
    public static string ToCode(RecurringLateFeeOption type) =>
        Get(type).Code;

    public static RecurringLateFeeOption? ToEnum(string code) =>
        GetByCode(code)?.Type;
}
