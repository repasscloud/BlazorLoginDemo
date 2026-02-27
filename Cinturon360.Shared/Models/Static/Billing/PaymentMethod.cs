namespace Cinturon360.Shared.Models.Static.Billing;

public enum PaymentMethod : int { INVOICE = 0, PAYPAL = 1, STRIPE = 2, BANK_TRANSFER_AU = 3, BANK_TRANSFER_NZ = 4, CREDIT_CARD = 5, APPLE_PAY = 6, GOOGLE_PAY = 7 }

public static class PaymentMethods
{
    public sealed record Method(
        PaymentMethod Type,
        string Code,
        string CodeLower,
        string Label,
        string Description
    );

    // Single source of truth
    private static readonly IReadOnlyList<Method> _all =
    [
        new(
            PaymentMethod.INVOICE,
            "INVOICE",
            "invoice",
            "Invoice",
            "Payment is settled via issued invoice."
        ),
        new(
            PaymentMethod.PAYPAL,
            "PAYPAL",
            "paypal",
            "PayPal",
            "Payment is processed through PayPal."
        ),
        new(
            PaymentMethod.STRIPE,
            "STRIPE",
            "stripe",
            "Stripe",
            "Payment is processed via Stripe."
        ),
        new(
            PaymentMethod.BANK_TRANSFER_AU,
            "BANK_TRANSFER_AU",
            "bank_transfer_au",
            "Bank Transfer (AU)",
            "Australian domestic bank transfer."
        ),
        new(
            PaymentMethod.BANK_TRANSFER_NZ,
            "BANK_TRANSFER_NZ",
            "bank_transfer_nz",
            "Bank Transfer (NZ)",
            "New Zealand domestic bank transfer."
        ),
        new(
            PaymentMethod.CREDIT_CARD,
            "CREDIT_CARD",
            "credit_card",
            "Credit Card",
            "Payment via card networks (Visa, Mastercard, Amex)."
        ),
        new(
            PaymentMethod.APPLE_PAY,
            "APPLE_PAY",
            "apple_pay",
            "Apple Pay",
            "Payment using Apple Pay wallets."
        ),
        new(
            PaymentMethod.GOOGLE_PAY,
            "GOOGLE_PAY",
            "google_pay",
            "Google Pay",
            "Payment using Google Pay wallets."
        )
    ];

    public static IReadOnlyList<Method> All => _all;

    // UI projection (dropdown-safe)
    public static IReadOnlyList<(string Code, string Label)> DropdownOptions =>
        _all.Select(m => (m.Code, m.Label)).ToList();

    // Enum-safe lookup
    public static Method Get(PaymentMethod type) =>
        _all.First(m => m.Type == type);

    // String-safe lookup (DB / API)
    public static Method? GetByCode(string code) =>
        _all.FirstOrDefault(m =>
            m.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

    // Helpers
    public static string ToCode(PaymentMethod type) =>
        Get(type).Code;

    public static PaymentMethod? ToEnum(string code) =>
        GetByCode(code)?.Type;
}
