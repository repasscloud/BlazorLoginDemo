namespace Cinturon360.Shared.Models.Static.Billing;

public enum ServiceFeeType : int { NONE = 0, MARKUP_ONLY = 1, PER_ITEM_FEE_ONLY = 2, MARKUP_AND_PER_ITEM_FEE = 3 }

public static class ServiceFeeTypes
{
    public sealed record FeeType(
        ServiceFeeType Type, // enum-safe
        string Code,         // "MARKUP_ONLY"
        string CodeLower,    // "markup_only"
        string Label,        // UI label
        string Description  // helper text / tooltip
    );

    // Single source of truth
    private static readonly IReadOnlyList<FeeType> _all =
    [
        new(
            ServiceFeeType.NONE,
            "NONE",
            "none",
            "None",
            "No markup or per-item service fee applied."
        ),
        new(
            ServiceFeeType.MARKUP_ONLY,
            "MARKUP_ONLY",
            "markup_only",
            "Markup Only",
            "Applies a percentage markup without any per-item fee."
        ),
        new(
            ServiceFeeType.PER_ITEM_FEE_ONLY,
            "PER_ITEM_FEE_ONLY",
            "per_item_fee_only",
            "Per Item Fee Only",
            "Applies a fixed fee per item without any markup."
        ),
        new(
            ServiceFeeType.MARKUP_AND_PER_ITEM_FEE,
            "MARKUP_AND_PER_ITEM_FEE",
            "markup_and_per_item_fee",
            "Markup & Per Item Fee",
            "Applies both a percentage markup and a fixed per-item fee."
        )
    ];

    public static IReadOnlyList<FeeType> All => _all;

    // UI projection (dropdown-safe)
    public static IReadOnlyList<(string Code, string Label)> DropdownOptions =>
        _all.Select(f => (f.Code, f.Label)).ToList();

    // Enum-safe lookup
    public static FeeType Get(ServiceFeeType type) =>
        _all.First(f => f.Type == type);

    // String-safe lookup (DB / API)
    public static FeeType? GetByCode(string code) =>
        _all.FirstOrDefault(f =>
            f.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

    // Helpers
    public static string ToCode(ServiceFeeType type) =>
        Get(type).Code;

    public static ServiceFeeType? ToEnum(string code) =>
        GetByCode(code)?.Type;
}
