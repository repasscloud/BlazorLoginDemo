using Cinturon360.Domain.Common.Base;
using Cinturon360.Domain.Enums.Billing;

namespace Cinturon360.Domain.Entities.Billing;

/// <summary>SaaS license assigned to an organisation.</summary>
public sealed class OrgLicense : Entity
{
    public string OrgId { get; private set; } = string.Empty;
    public LicenseType LicenseType { get; private set; }
    public BillingCycle BillingCycle { get; private set; }
    public int MaxUsers { get; private set; }
    public int MaxBookingsPerMonth { get; private set; }
    public DateOnly StartsOn { get; private set; }
    public DateOnly? ExpiresOn { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Stripe subscription ID for recurring billing
    public string? StripeSubscriptionId { get; private set; }

    private OrgLicense() { }

    public static OrgLicense Create(
        string id,
        string orgId,
        LicenseType licenseType,
        BillingCycle billingCycle,
        int maxUsers,
        int maxBookingsPerMonth,
        DateOnly startsOn,
        DateOnly? expiresOn = null,
        string? stripeSubscriptionId = null)
        => new()
        {
            Id = id,
            OrgId = orgId,
            LicenseType = licenseType,
            BillingCycle = billingCycle,
            MaxUsers = maxUsers,
            MaxBookingsPerMonth = maxBookingsPerMonth,
            StartsOn = startsOn,
            ExpiresOn = expiresOn,
            StripeSubscriptionId = stripeSubscriptionId,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void Deactivate() { IsActive = false; UpdatedAt = DateTimeOffset.UtcNow; }
    public void SetExpiry(DateOnly expiry) { ExpiresOn = expiry; UpdatedAt = DateTimeOffset.UtcNow; }
}

/// <summary>Billing configuration for an organisation (Stripe customer details).</summary>
public sealed class OrgBillingConfig : Entity
{
    public string OrgId { get; private set; } = string.Empty;
    public string? StripeCustomerId { get; private set; }
    public string? BillingEmail { get; private set; }
    public string? BillingName { get; private set; }
    public string CurrencyCode { get; private set; } = "USD";
    public bool VatExempt { get; private set; }
    public string? VatNumber { get; private set; }

    private OrgBillingConfig() { }

    public static OrgBillingConfig Create(
        string id,
        string orgId,
        string? stripeCustomerId = null,
        string? billingEmail = null,
        string? billingName = null,
        string currencyCode = "USD")
        => new()
        {
            Id = id,
            OrgId = orgId,
            StripeCustomerId = stripeCustomerId,
            BillingEmail = billingEmail,
            BillingName = billingName,
            CurrencyCode = currencyCode,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void Update(string? billingEmail, string? billingName, bool vatExempt, string? vatNumber)
    {
        BillingEmail = billingEmail;
        BillingName = billingName;
        VatExempt = vatExempt;
        VatNumber = vatNumber;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetStripeCustomerId(string customerId) { StripeCustomerId = customerId; UpdatedAt = DateTimeOffset.UtcNow; }
}

/// <summary>Invoice issued to an organisation.</summary>
public sealed class Invoice : SoftDeletableEntity
{
    public string OrgId { get; private set; } = string.Empty;
    public string InvoiceNumber { get; private set; } = string.Empty;
    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;
    public decimal SubtotalAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string CurrencyCode { get; private set; } = "USD";
    public DateOnly IssuedOn { get; private set; }
    public DateOnly DueOn { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }
    public string? StripeInvoiceId { get; private set; }
    public string? Notes { get; private set; }

    private Invoice() { }

    public static Invoice Create(
        string id,
        string orgId,
        string invoiceNumber,
        decimal subtotal,
        decimal tax,
        string currencyCode,
        DateOnly issuedOn,
        DateOnly dueOn,
        string? stripeInvoiceId = null)
        => new()
        {
            Id = id,
            OrgId = orgId,
            InvoiceNumber = invoiceNumber,
            SubtotalAmount = subtotal,
            TaxAmount = tax,
            TotalAmount = subtotal + tax,
            CurrencyCode = currencyCode,
            IssuedOn = issuedOn,
            DueOn = dueOn,
            StripeInvoiceId = stripeInvoiceId,
            Status = InvoiceStatus.Draft,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void Send() { Status = InvoiceStatus.Sent; UpdatedAt = DateTimeOffset.UtcNow; }
    public void MarkPaid() { Status = InvoiceStatus.Paid; PaidAt = DateTimeOffset.UtcNow; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Void() { Status = InvoiceStatus.Void; UpdatedAt = DateTimeOffset.UtcNow; }
    public void MarkOverdue() { Status = InvoiceStatus.Overdue; UpdatedAt = DateTimeOffset.UtcNow; }
}

/// <summary>A payment record against an invoice.</summary>
public sealed class Payment : Entity
{
    public string OrgId { get; private set; } = string.Empty;
    public string? InvoiceId { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public PaymentMethod Method { get; private set; }
    public decimal Amount { get; private set; }
    public string CurrencyCode { get; private set; } = "USD";
    public string? StripePaymentIntentId { get; private set; }
    public string? StripeChargeId { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public string? FailureReason { get; private set; }

    private Payment() { }

    public static Payment Create(
        string id,
        string orgId,
        decimal amount,
        string currencyCode,
        PaymentMethod method,
        string? invoiceId = null,
        string? stripePaymentIntentId = null)
        => new()
        {
            Id = id,
            OrgId = orgId,
            InvoiceId = invoiceId,
            Amount = amount,
            CurrencyCode = currencyCode,
            Method = method,
            StripePaymentIntentId = stripePaymentIntentId,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void MarkSucceeded(string? chargeId = null)
    {
        Status = PaymentStatus.Succeeded;
        StripeChargeId = chargeId;
        ProcessedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        Status = PaymentStatus.Failed;
        FailureReason = reason;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

/// <summary>Prepaid credit balance for an organisation. Used to offset invoice amounts.</summary>
public sealed class PrepaidBalance : Entity
{
    public string OrgId { get; private set; } = string.Empty;
    public decimal BalanceAmount { get; private set; }
    public string CurrencyCode { get; private set; } = "USD";

    private PrepaidBalance() { }

    public static PrepaidBalance Create(string id, string orgId, string currencyCode)
        => new() { Id = id, OrgId = orgId, CurrencyCode = currencyCode, BalanceAmount = 0, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };

    public void Credit(decimal amount) { BalanceAmount += amount; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Debit(decimal amount) { BalanceAmount -= amount; UpdatedAt = DateTimeOffset.UtcNow; }
}
