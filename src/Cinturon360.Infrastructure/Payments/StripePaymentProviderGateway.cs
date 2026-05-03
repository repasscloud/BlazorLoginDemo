using Cinturon360.Application.Abstractions.Services;
using Cinturon360.Domain.Enums.Billing;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace Cinturon360.Infrastructure.Payments;

internal sealed class StripePaymentProviderGateway(ILogger<StripePaymentProviderGateway> logger) : IPaymentProviderGateway
{
    public PaymentProviderType ProviderType => PaymentProviderType.Stripe;

    public async Task<ProviderConnectionVerificationResult> VerifyConnectionAsync(
        PaymentProviderContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            var accountService = new AccountService();
            var account = await accountService.GetAsync(
                id: "self",
                options: null,
                requestOptions: new RequestOptions { ApiKey = context.SecretKey },
                cancellationToken: cancellationToken);

            return new ProviderConnectionVerificationResult(
                Success: true,
                ProviderAccountId: account.Id,
                ProviderAccountName: account.Settings?.Dashboard?.DisplayName ?? account.BusinessProfile?.Name,
                Error: null);
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe provider verification failed for connection {ConnectionId}", context.PaymentProviderConnectionId);
            return new ProviderConnectionVerificationResult(false, null, null, ex.Message);
        }
    }

    public async Task<ProviderWebhookRegistrationResult> RegisterWebhookEndpointAsync(
        PaymentProviderContext context,
        string webhookUrl,
        IReadOnlyList<string> enabledEvents,
        CancellationToken cancellationToken)
    {
        try
        {
            var webhookService = new WebhookEndpointService();
            var endpoint = await webhookService.CreateAsync(
                new WebhookEndpointCreateOptions
                {
                    Url = webhookUrl,
                    EnabledEvents = enabledEvents.ToList()
                },
                new RequestOptions { ApiKey = context.SecretKey },
                cancellationToken);

            return new ProviderWebhookRegistrationResult(
                Success: true,
                WebhookEndpointId: endpoint.Id,
                WebhookSigningSecret: endpoint.Secret,
                Error: null);
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe webhook endpoint registration failed for connection {ConnectionId}", context.PaymentProviderConnectionId);
            return new ProviderWebhookRegistrationResult(false, null, null, ex.Message);
        }
    }

    public async Task<CreateProviderCustomerResult> CreateCustomerAsync(
        PaymentProviderContext context,
        CreateProviderCustomerRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var service = new CustomerService();
            var customer = await service.CreateAsync(
                new CustomerCreateOptions
                {
                    Email = request.Email,
                    Name = request.Name,
                    Metadata = new Dictionary<string, string>
                    {
                        ["buyerOrganisationId"] = request.BuyerOrganisationId,
                        ["connectionId"] = context.PaymentProviderConnectionId
                    }
                },
                new RequestOptions { ApiKey = context.SecretKey },
                cancellationToken);

            return new CreateProviderCustomerResult(true, customer.Id, null);
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe customer creation failed for buyer {BuyerOrgId}", request.BuyerOrganisationId);
            return new CreateProviderCustomerResult(false, null, ex.Message);
        }
    }

    public async Task<CreatePaymentMethodSetupResult> CreatePaymentMethodSetupAsync(
        PaymentProviderContext context,
        CreatePaymentMethodSetupRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var service = new SessionService();
            var session = await service.CreateAsync(
                new SessionCreateOptions
                {
                    Mode = "setup",
                    Customer = request.ProviderCustomerId,
                    SuccessUrl = request.ReturnUrl,
                    CancelUrl = request.CancelUrl,
                    PaymentMethodTypes = ["card"],
                    Metadata = new Dictionary<string, string>
                    {
                        ["billingRelationshipId"] = request.BillingRelationshipId,
                        ["sellerOrganisationId"] = request.SellerOrganisationId,
                        ["buyerOrganisationId"] = request.BuyerOrganisationId,
                        ["purpose"] = request.Purpose,
                        ["connectionId"] = context.PaymentProviderConnectionId
                    }
                },
                new RequestOptions { ApiKey = context.SecretKey },
                cancellationToken);

            return new CreatePaymentMethodSetupResult(true, session.Url, null);
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe setup session creation failed for customer {CustomerId}", request.ProviderCustomerId);
            return new CreatePaymentMethodSetupResult(false, null, ex.Message);
        }
    }

    public async Task<IReadOnlyList<ProviderPaymentMethodSnapshot>> ListPaymentMethodsAsync(
        PaymentProviderContext context,
        string providerCustomerId,
        CancellationToken cancellationToken)
    {
        var service = new PaymentMethodService();
        var methods = await service.ListAsync(
            new PaymentMethodListOptions
            {
                Customer = providerCustomerId,
                Type = "card"
            },
            new RequestOptions { ApiKey = context.SecretKey },
            cancellationToken);

        return methods
            .Select(m => new ProviderPaymentMethodSnapshot(
                m.Id,
                BuildDisplayName(m.Card?.Brand, m.Card?.Last4, m.Card?.ExpMonth, m.Card?.ExpYear),
                m.Card?.Brand,
                m.Card?.Last4,
                ToNullableInt(m.Card?.ExpMonth),
                ToNullableInt(m.Card?.ExpYear),
                m.BillingDetails?.Name,
                m.Card?.Fingerprint,
                true))
            .ToList();
    }

    public async Task<CreatePaymentResult> CreatePaymentAsync(
        PaymentProviderContext context,
        CreateProviderPaymentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(
                new PaymentIntentCreateOptions
                {
                    Customer = request.ProviderCustomerId,
                    PaymentMethod = request.ProviderPaymentMethodId,
                    Amount = ToMinorUnits(request.Amount),
                    Currency = request.CurrencyCode.ToLowerInvariant(),
                    Confirm = request.Confirm,
                    OffSession = request.OffSession,
                    CaptureMethod = request.ManualCapture ? "manual" : "automatic",
                    Description = request.Description,
                    AutomaticPaymentMethods = request.ProviderPaymentMethodId is null
                        ? new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true }
                        : null,
                    Metadata = request.Metadata.ToDictionary(x => x.Key, x => x.Value)
                },
                new RequestOptions { ApiKey = context.SecretKey },
                cancellationToken);

            return new CreatePaymentResult(true, intent.Id, intent.ClientSecret, intent.Status, null);
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe payment creation failed for customer {CustomerId}", request.ProviderCustomerId);
            return new CreatePaymentResult(false, null, null, null, ex.Message);
        }
    }

    public ParsedProviderWebhookEvent? ParseWebhookEvent(string rawBody, string signatureHeader, string webhookSecret)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(rawBody, signatureHeader, webhookSecret);

            var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string? providerObjectId = null;
            decimal? amount = null;
            string? currency = null;
            string? providerCustomerId = null;

            if (stripeEvent.Data.Object is PaymentIntent paymentIntent)
            {
                providerObjectId = paymentIntent.Id;
                amount = paymentIntent.Amount / 100m;
                currency = paymentIntent.Currency;
                providerCustomerId = paymentIntent.CustomerId;
                CopyMetadata(paymentIntent.Metadata, metadata);
            }
            else if (stripeEvent.Data.Object is Session session)
            {
                providerObjectId = session.Id;
                providerCustomerId = session.CustomerId;
                CopyMetadata(session.Metadata, metadata);
            }
            else if (stripeEvent.Data.Object is SetupIntent setupIntent)
            {
                providerObjectId = setupIntent.Id;
                providerCustomerId = setupIntent.CustomerId;
                CopyMetadata(setupIntent.Metadata, metadata);
            }
            else if (stripeEvent.Data.Object is Invoice invoice)
            {
                providerObjectId = invoice.Id;
                amount = invoice.AmountDue / 100m;
                currency = invoice.Currency;
                providerCustomerId = invoice.CustomerId;
                CopyMetadata(invoice.Metadata, metadata);
            }

            return new ParsedProviderWebhookEvent(
                stripeEvent.Id,
                stripeEvent.Type,
                providerObjectId,
                amount,
                currency,
                providerCustomerId,
                metadata,
                rawBody);
        }
        catch (StripeException ex)
        {
            logger.LogWarning(ex, "Stripe webhook signature verification failed");
            return null;
        }
    }

    private static string BuildDisplayName(string? brand, string? last4, long? month, long? year)
    {
        var brandText = string.IsNullOrWhiteSpace(brand) ? "Card" : brand;
        var last4Text = string.IsNullOrWhiteSpace(last4) ? "****" : last4;
        var expiry = month.HasValue && year.HasValue ? $" exp {month:00}/{year}" : string.Empty;
        return $"{brandText} ending {last4Text}{expiry}";
    }

    private static void CopyMetadata(IDictionary<string, string>? source, IDictionary<string, string> destination)
    {
        if (source is null)
            return;

        foreach (var entry in source)
            destination[entry.Key] = entry.Value;
    }

    private static long ToMinorUnits(decimal amount)
    {
        var rounded = Math.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);
        return Convert.ToInt64(rounded);
    }

    private static int? ToNullableInt(long? value)
        => value.HasValue ? Convert.ToInt32(value.Value) : null;
}
