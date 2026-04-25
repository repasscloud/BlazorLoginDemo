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
}

public sealed record CreateCustomerResult(bool Success, string? CustomerId, string? Error);
public sealed record CreatePaymentIntentResult(bool Success, string? PaymentIntentId, string? ClientSecret, string? Error);
