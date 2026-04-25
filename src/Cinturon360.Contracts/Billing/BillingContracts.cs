namespace Cinturon360.Contracts.Billing;

public sealed record CreateInvoiceRequest(
    string InvoiceNumber,
    decimal Subtotal,
    decimal Tax,
    string CurrencyCode,
    DateOnly IssuedOn,
    DateOnly DueOn,
    string? StripeInvoiceId);

public sealed record RecordPaymentRequest(
    decimal Amount,
    string CurrencyCode,
    int Method,
    string? InvoiceId,
    string? StripePaymentIntentId);

public sealed record CreditPrepaidBalanceRequest(
    decimal Amount,
    string CurrencyCode);

public sealed record OrgLicenseResponse(
    string Id,
    string OrgId,
    int LicenseType,
    int BillingCycle,
    int MaxUsers,
    int MaxBookingsPerMonth,
    DateOnly StartsOn,
    DateOnly? ExpiresOn,
    bool IsActive);

public sealed record InvoiceResponse(
    string Id,
    string OrgId,
    string InvoiceNumber,
    int Status,
    decimal SubtotalAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    string CurrencyCode,
    DateOnly IssuedOn,
    DateOnly DueOn,
    DateTimeOffset? PaidAt);
