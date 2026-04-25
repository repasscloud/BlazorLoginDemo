using Cinturon360.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace Cinturon360.Infrastructure.Payments;

/// <summary>
/// Stub Stripe payment gateway implementation.
/// TODO: Replace with real Stripe SDK calls when secret key is configured.
/// </summary>
internal sealed class StripePaymentGateway : IPaymentGateway
{
    private readonly ILogger<StripePaymentGateway> _logger;

    public StripePaymentGateway(ILogger<StripePaymentGateway> logger)
    {
        _logger = logger;
    }

    public Task<CreateCustomerResult> CreateCustomerAsync(
        string orgId,
        string email,
        string name,
        CancellationToken ct = default)
    {
        _logger.LogInformation("[StripeStub] CreateCustomer for org {OrgId}", orgId);
        return Task.FromResult(new CreateCustomerResult(true, $"cus_stub_{orgId}", null));
    }

    public Task<CreatePaymentIntentResult> CreatePaymentIntentAsync(
        string customerId,
        decimal amount,
        string currencyCode,
        string? description = null,
        CancellationToken ct = default)
    {
        _logger.LogInformation("[StripeStub] CreatePaymentIntent {Amount} {Currency} for {Customer}", amount, currencyCode, customerId);
        return Task.FromResult(new CreatePaymentIntentResult(true, $"pi_stub_{Guid.NewGuid():N}", "seti_stub_secret", null));
    }

    public Task<bool> RefundAsync(string chargeId, decimal? amount = null, CancellationToken ct = default)
    {
        _logger.LogInformation("[StripeStub] Refund for charge {ChargeId}", chargeId);
        return Task.FromResult(true);
    }
}
