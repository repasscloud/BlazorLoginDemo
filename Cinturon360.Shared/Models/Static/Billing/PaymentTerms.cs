namespace Cinturon360.Shared.Models.Static.Billing;

public enum PaymentTerms : int { NET_0 = 0, NET_1 = 1, NET_7 = 7, NET_14 = 14, NET_21 = 21, NET_30 = 30, NET_60 = 60, NET_90 = 90 }

public static class PaymentTermCatalog
{
    public sealed record Term(
        PaymentTerms Type,
        string Code,
        string CodeLower,
        string Label,
        int Days,
        string Description
    );

    private static readonly IReadOnlyList<Term> _all =
    [
        new(PaymentTerms.NET_0,  "NET_0",  "net_0",  "Net 0 Days",  0,  "Payment due immediately."),
        new(PaymentTerms.NET_1,  "NET_1",  "net_1",  "Net 1 Day",   1,  "Payment due one day after invoice."),
        new(PaymentTerms.NET_7,  "NET_7",  "net_7",  "Net 7 Days",  7,  "Payment due seven days after invoice."),
        new(PaymentTerms.NET_14, "NET_14", "net_14", "Net 14 Days", 14, "Payment due fourteen days after invoice."),
        new(PaymentTerms.NET_21, "NET_21", "net_21", "Net 21 Days", 21, "Payment due twenty-one days after invoice."),
        new(PaymentTerms.NET_30, "NET_30", "net_30", "Net 30 Days", 30, "Payment due thirty days after invoice."),
        new(PaymentTerms.NET_60, "NET_60", "net_60", "Net 60 Days", 60, "Payment due sixty days after invoice."),
        new(PaymentTerms.NET_90, "NET_90", "net_90", "Net 90 Days", 90, "Payment due ninety days after invoice.")
    ];

    public static IReadOnlyList<Term> All => _all;

    // UI projection (dropdown-safe)
    public static IReadOnlyList<(string Code, string Label)> DropdownOptions =>
        _all.Select(t => (t.Code, t.Label)).ToList();

    // Enum-safe lookup
    public static Term Get(PaymentTerms type) =>
        _all.First(t => t.Type == type);

    // String-safe lookup
    public static Term? GetByCode(string code) =>
        _all.FirstOrDefault(t =>
            t.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

    // Helpers
    public static string ToCode(PaymentTerms type) =>
        Get(type).Code;

    public static PaymentTerms? ToEnum(string code) =>
        GetByCode(code)?.Type;
}
