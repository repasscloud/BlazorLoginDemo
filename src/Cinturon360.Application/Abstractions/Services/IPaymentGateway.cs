namespace Cinturon360.Application.Abstractions.Services;

/// <summary>
/// Abstraction for Stripe payment processing.
/// </summary>
public interface IPaymentGateway
{
    Task<CreateCustomerResult> CreateCustomerAsync(
        string orgId,
        string email,
        string name,
        CancellationToken ct = default);

    Task<CreatePaymentIntentResult> CreatePaymentIntentAsync(
        string customerId,
        decimal amount,
        string currencyCode,
        string? description = null,
        CancellationToken ct = default);

    Task<bool> RefundAsync(string chargeId, decimal? amount = null, CancellationToken ct = default);

    /// <summary>
    /// Constructs and validates a Stripe webhook event from the raw request body and Stripe-Signature header.
    /// Returns null when signature verification fails or the webhook secret is not configured.
    /// </summary>
    StripeWebhookEvent? ParseWebhookEvent(string rawBody, string stripeSignatureHeader);
}

public sealed record CreateCustomerResult(bool Success, string? CustomerId, string? Error);
public sealed record CreatePaymentIntentResult(bool Success, string? PaymentIntentId, string? ClientSecret, string? Error);

/// <summary>Simplified webhook event DTO — avoids coupling the Application layer to the Stripe SDK.</summary>
public sealed record StripeWebhookEvent(string EventType, string? RelatedObjectId, decimal? Amount, string? Currency, string? OrgId);
