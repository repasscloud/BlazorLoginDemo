using Cinturon360.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace Cinturon360.Infrastructure.Payments;

/// <summary>
/// Stripe payment gateway implementation.
/// Falls back to stub mode when Stripe is not configured.
/// </summary>
internal sealed class StripePaymentGateway : IPaymentGateway
{
    private readonly StripeSettings _settings;
    private readonly ILogger<StripePaymentGateway> _logger;

    public StripePaymentGateway(
        IOptions<StripeSettings> settings,
        ILogger<StripePaymentGateway> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        if (!string.IsNullOrWhiteSpace(_settings.SecretKey))
        {
            StripeConfiguration.ApiKey = _settings.SecretKey;
        }
    }

    public async Task<CreateCustomerResult> CreateCustomerAsync(
        string orgId,
        string email,
        string name,
        CancellationToken ct = default)
    {
        if (!IsConfigured())
        {
            _logger.LogWarning("Stripe not configured. Using stub customer for org {OrgId}", orgId);
            return new CreateCustomerResult(true, $"cus_stub_{orgId}", null);
        }

        try
        {
            var service = new CustomerService();
            var customer = await service.CreateAsync(new CustomerCreateOptions
            {
                Email = email,
                Name = name,
                Metadata = new Dictionary<string, string>
                {
                    ["orgId"] = orgId
                }
            }, cancellationToken: ct);

            return new CreateCustomerResult(true, customer.Id, null);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe customer creation failed for org {OrgId}", orgId);
            return new CreateCustomerResult(false, null, ex.Message);
        }
    }

    public async Task<CreatePaymentIntentResult> CreatePaymentIntentAsync(
        string customerId,
        decimal amount,
        string currencyCode,
        string? description = null,
        CancellationToken ct = default)
    {
        if (amount <= 0)
            return new CreatePaymentIntentResult(false, null, null, "Amount must be greater than zero.");

        if (!IsConfigured())
        {
            _logger.LogWarning("Stripe not configured. Using stub payment intent for customer {Customer}", customerId);
            return new CreatePaymentIntentResult(true, $"pi_stub_{Guid.NewGuid():N}", "seti_stub_secret", null);
        }

        try
        {
            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(new PaymentIntentCreateOptions
            {
                Customer = customerId,
                Amount = ToMinorUnits(amount),
                Currency = currencyCode.ToLowerInvariant(),
                Description = description,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                }
            }, cancellationToken: ct);

            return new CreatePaymentIntentResult(true, intent.Id, intent.ClientSecret, null);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe payment intent creation failed for customer {Customer}", customerId);
            return new CreatePaymentIntentResult(false, null, null, ex.Message);
        }
    }

    public async Task<bool> RefundAsync(string chargeId, decimal? amount = null, CancellationToken ct = default)
    {
        if (!IsConfigured())
        {
            _logger.LogWarning("Stripe not configured. Using stub refund for charge {ChargeId}", chargeId);
            return true;
        }

        try
        {
            var service = new RefundService();
            var options = new RefundCreateOptions
            {
                Charge = chargeId,
                Amount = amount.HasValue ? ToMinorUnits(amount.Value) : null
            };

            var refund = await service.CreateAsync(options, cancellationToken: ct);
            return refund.Status is "succeeded" or "pending";
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe refund failed for charge {ChargeId}", chargeId);
            return false;
        }
    }

    private bool IsConfigured() => !string.IsNullOrWhiteSpace(_settings.SecretKey);

    private static long ToMinorUnits(decimal amount)
    {
        var rounded = Math.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);
        return Convert.ToInt64(rounded);
    }
}

public sealed class StripeSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
}
