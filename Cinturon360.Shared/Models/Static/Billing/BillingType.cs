namespace Cinturon360.Shared.Models.Static.Billing;

public enum BillingType : int { PREPAID = 0, POSTPAID = 1 }

public static class BillingTypes
{
    public sealed record TypeDef(
        BillingType Type,
        string Code,
        string CodeLower,
        string Label,
        string Description
    );

    private static readonly IReadOnlyList<TypeDef> _all =
    [
        new(
            BillingType.PREPAID,
            "PREPAID",
            "prepaid",
            "Prepaid",
            "Charges are collected in advance before services are consumed."
        ),
        new(
            BillingType.POSTPAID,
            "POSTPAID",
            "postpaid",
            "Postpaid",
            "Charges are billed after services have been consumed."
        )
    ];

    public static IReadOnlyList<TypeDef> All => _all;

    // UI projection
    public static IReadOnlyList<(string Code, string Label)> DropdownOptions =>
        _all.Select(t => (t.Code, t.Label)).ToList();

    public static TypeDef Get(BillingType type) =>
        _all.First(t => t.Type == type);

    public static TypeDef? GetByCode(string code) =>
        _all.FirstOrDefault(t =>
            t.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

    public static string ToCode(BillingType type) =>
        Get(type).Code;

    public static BillingType? ToEnum(string code) =>
        GetByCode(code)?.Type;
}
