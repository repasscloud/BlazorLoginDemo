using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Application.Abstractions.Services;
using Cinturon360.Common.IdGeneration;
using Cinturon360.Common.Results;
using Cinturon360.Domain.Entities.Billing;
using Cinturon360.Domain.Enums.Billing;

namespace Cinturon360.Application.Features.Billing.Commands;

public sealed record ConfigureStripeProviderConnectionCommand(
    string OwnerOrganisationId,
    string DisplayName,
    bool IsLiveMode,
    string SecretKey,
    string PublishableKey,
    bool AllowChildOrgBilling,
    bool AllowClientCheckout,
    bool AllowMonthlyInvoiceCollection,
    ProviderUsageScope UsageScope) : IRequest<Result<ConfigureStripeProviderConnectionResult>>;

public sealed record ConfigureStripeProviderConnectionResult(
    string ConnectionId,
    string WebhookPath,
    string SecretBundleReference,
    bool IsVerified);

public sealed class ConfigureStripeProviderConnectionHandler(
    IOrganisationRepository organisationRepository,
    IBillingRepository billingRepository,
    IPaymentProviderGateway paymentProviderGateway,
    ISecretStore secretStore,
    IConfiguration configuration,
    ILogger<ConfigureStripeProviderConnectionHandler> logger,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ConfigureStripeProviderConnectionCommand, Result<ConfigureStripeProviderConnectionResult>>
{
    private static readonly string[] StripeDefaultWebhookEvents =
    [
        "checkout.session.completed",
        "setup_intent.succeeded",
        "payment_method.attached",
        "payment_intent.succeeded",
        "payment_intent.payment_failed",
        "invoice.payment_succeeded",
        "invoice.payment_failed",
        "invoice.finalized",
        "charge.refunded",
        "charge.dispute.created"
    ];

    public async Task<Result<ConfigureStripeProviderConnectionResult>> Handle(ConfigureStripeProviderConnectionCommand request, CancellationToken ct)
    {
        var ownerOrg = await organisationRepository.GetByIdAsync(request.OwnerOrganisationId, ct);
        if (ownerOrg is null)
            return Result.Failure<ConfigureStripeProviderConnectionResult>(new("billing.owner_org_not_found", "Owner organisation was not found."));

        var connectionId = IdGenerator.New(IdPrefix.ProviderConnection);
        var mode = request.IsLiveMode ? "live" : "test";
        var orgKey = request.OwnerOrganisationId.Replace("_", "-").ToLowerInvariant();
        var bundleRef = $"payment-providers--{orgKey}--stripe--{mode}";

        await secretStore.SetSecretAsync($"{bundleRef}--secret-key", request.SecretKey, null, ct);
        await secretStore.SetSecretAsync($"{bundleRef}--publishable-key", request.PublishableKey, null, ct);

        var webhookPath = $"/api/v1/webhooks/payment-providers/stripe/{connectionId}";

        var publicApiBaseUrl = ResolveApiBaseUrl(configuration);
        if (string.IsNullOrWhiteSpace(publicApiBaseUrl))
            return Result.Failure<ConfigureStripeProviderConnectionResult>(new("billing.api_base_url_missing", "Public API base URL is required to auto-provision Stripe webhook endpoint."));

        var providerContext = new PaymentProviderContext(
            connectionId,
            request.OwnerOrganisationId,
            PaymentProviderType.Stripe,
            request.IsLiveMode,
            bundleRef,
            request.SecretKey,
            request.PublishableKey,
            WebhookSecret: null);

        var verifyResult = await paymentProviderGateway.VerifyConnectionAsync(providerContext, ct);
        if (!verifyResult.Success)
        {
            logger.LogWarning("Stripe provider connection verification failed for owner organisation {OwnerOrganisationId}: {Error}", request.OwnerOrganisationId, verifyResult.Error);
            return Result.Failure<ConfigureStripeProviderConnectionResult>(new(
                "billing.provider_connection_verify_failed",
                verifyResult.Error ?? "Stripe account verification failed."));
        }

        var webhookRegistration = await paymentProviderGateway.RegisterWebhookEndpointAsync(
            providerContext,
            $"{publicApiBaseUrl.TrimEnd('/')}{webhookPath}",
            StripeDefaultWebhookEvents,
            ct);

        if (!webhookRegistration.Success
            || string.IsNullOrWhiteSpace(webhookRegistration.WebhookEndpointId)
            || string.IsNullOrWhiteSpace(webhookRegistration.WebhookSigningSecret))
        {
            logger.LogWarning("Stripe webhook endpoint registration failed for owner organisation {OwnerOrganisationId}: {Error}", request.OwnerOrganisationId, webhookRegistration.Error);
            return Result.Failure<ConfigureStripeProviderConnectionResult>(new(
                "billing.webhook_registration_failed",
                webhookRegistration.Error ?? "Stripe webhook endpoint registration failed."));
        }

        var webhookSecretReference = $"{bundleRef}--webhook-secret";
        await secretStore.SetSecretAsync(webhookSecretReference, webhookRegistration.WebhookSigningSecret, null, ct);

        var connection = PaymentProviderConnection.Create(
            connectionId,
            request.OwnerOrganisationId,
            PaymentProviderType.Stripe,
            request.DisplayName,
            request.IsLiveMode,
            bundleRef,
            request.UsageScope,
            request.AllowChildOrgBilling,
            request.AllowClientCheckout,
            request.AllowMonthlyInvoiceCollection);

        connection.SetWebhook(webhookRegistration.WebhookEndpointId, webhookSecretReference);
        connection.MarkVerified(
            providerAccountId: verifyResult.ProviderAccountId,
            providerAccountName: verifyResult.ProviderAccountName ?? request.DisplayName);

        await billingRepository.AddProviderConnectionAsync(connection, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new ConfigureStripeProviderConnectionResult(
            connectionId,
            webhookPath,
            bundleRef,
            IsVerified: true));
    }

    private static string? ResolveApiBaseUrl(IConfiguration configuration)
        => configuration["Api:PublicBaseUrl"]
           ?? configuration["Api:BaseUrl"]
           ?? configuration["PublicApi:BaseUrl"]
           ?? configuration["PublicApiBaseUrl"];
}

public sealed record CreateBillingRelationshipCommand(
    string SellerOrganisationId,
    string BuyerOrganisationId,
    string PaymentProviderConnectionId,
    BillingMode BillingMode,
    BillingFrequency BillingFrequency,
    CollectionMode CollectionMode,
    ChargeTreatment ChargeTreatment,
    string CurrencyCode,
    int PaymentTermsDays,
    bool AutoCollectPayment,
    bool GenerateInvoices,
    bool RequiresPaymentSetup,
    CommercialRiskOwner CommercialRiskOwner) : IRequest<Result<string>>;

public sealed class CreateBillingRelationshipHandler(
    IOrganisationRepository organisationRepository,
    IBillingRepository billingRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateBillingRelationshipCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CreateBillingRelationshipCommand request, CancellationToken ct)
    {
        var seller = await organisationRepository.GetByIdAsync(request.SellerOrganisationId, ct);
        var buyer = await organisationRepository.GetByIdAsync(request.BuyerOrganisationId, ct);

        if (seller is null || buyer is null)
            return Result.Failure<string>(new("billing.organisation_not_found", "Seller or buyer organisation was not found."));

        var connection = await billingRepository.GetProviderConnectionByIdAsync(request.PaymentProviderConnectionId, ct);
        if (connection is null || !connection.IsEnabled || connection.ProviderType != PaymentProviderType.Stripe)
            return Result.Failure<string>(new("billing.provider_connection_not_found", "Provider connection was not found."));

        if (connection.OwnerOrganisationId != request.SellerOrganisationId)
            return Result.Failure<string>(new("billing.provider_connection_owner_mismatch", "Provider connection is not owned by seller organisation."));

        var existing = await billingRepository.GetBillingRelationshipAsync(request.SellerOrganisationId, request.BuyerOrganisationId, ct);
        if (existing is not null)
            return Result.Success(existing.Id);

        var relationship = BillingRelationship.Create(
            IdGenerator.New(IdPrefix.BillingRelationship),
            request.SellerOrganisationId,
            request.BuyerOrganisationId,
            request.PaymentProviderConnectionId,
            request.BillingMode,
            request.BillingFrequency,
            request.CollectionMode,
            request.ChargeTreatment,
            request.CurrencyCode,
            request.PaymentTermsDays,
            request.AutoCollectPayment,
            request.GenerateInvoices,
            request.RequiresPaymentSetup,
            request.CommercialRiskOwner);

        await billingRepository.AddBillingRelationshipAsync(relationship, ct);

        var profile = await billingRepository.GetOrganisationBillingProfileAsync(request.BuyerOrganisationId, ct);
        if (profile is null)
        {
            profile = OrganisationBillingProfile.Create(
                IdGenerator.New(IdPrefix.BillingProfile),
                request.BuyerOrganisationId,
                request.SellerOrganisationId,
                request.PaymentProviderConnectionId,
                request.BillingMode,
                request.BillingFrequency,
                request.CollectionMode,
                request.ChargeTreatment,
                request.CurrencyCode);

            await billingRepository.AddOrganisationBillingProfileAsync(profile, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(relationship.Id);
    }
}

public sealed record CreateBillingSetupLinkCommand(
    string BillingRelationshipId,
    string RecipientEmail,
    string RecipientName,
    string ReturnUrl,
    string CancelUrl,
    PaymentMethodPurpose Purpose,
    bool SendEmail) : IRequest<Result<CreateBillingSetupLinkResult>>;

public sealed record CreateBillingSetupLinkResult(string CheckoutUrl, string ProviderCustomerId);

public sealed class CreateBillingSetupLinkHandler(
    IOrganisationRepository organisationRepository,
    IBillingRepository billingRepository,
    IPaymentProviderGateway paymentProviderGateway,
    ISecretStore secretStore,
    IEmailService emailService,
    IUnitOfWork unitOfWork,
    ILogger<CreateBillingSetupLinkHandler> logger)
    : IRequestHandler<CreateBillingSetupLinkCommand, Result<CreateBillingSetupLinkResult>>
{
    public async Task<Result<CreateBillingSetupLinkResult>> Handle(CreateBillingSetupLinkCommand request, CancellationToken ct)
    {
        var relationship = await billingRepository.GetBillingRelationshipByIdAsync(request.BillingRelationshipId, ct);
        if (relationship is null)
            return Result.Failure<CreateBillingSetupLinkResult>(new("billing.relationship_not_found", "Billing relationship was not found."));

        if (relationship.PaymentProviderConnectionId is null)
            return Result.Failure<CreateBillingSetupLinkResult>(new("billing.provider_connection_missing", "Billing relationship has no provider connection."));

        var connection = await billingRepository.GetProviderConnectionByIdAsync(relationship.PaymentProviderConnectionId, ct);
        if (connection is null)
            return Result.Failure<CreateBillingSetupLinkResult>(new("billing.provider_connection_not_found", "Provider connection was not found."));

        var buyer = await organisationRepository.GetByIdAsync(relationship.BuyerOrganisationId, ct);
        if (buyer is null)
            return Result.Failure<CreateBillingSetupLinkResult>(new("billing.buyer_org_not_found", "Buyer organisation was not found."));

        var context = await BillingProviderCommandHelpers.BuildContextAsync(connection, secretStore, ct);

        var providerCustomer = await billingRepository.GetProviderCustomerAsync(connection.Id, relationship.BuyerOrganisationId, ct);
        if (providerCustomer is null)
        {
            var customerResult = await paymentProviderGateway.CreateCustomerAsync(
                context,
                new CreateProviderCustomerRequest(
                    relationship.BuyerOrganisationId,
                    request.RecipientEmail,
                    request.RecipientName),
                ct);

            if (!customerResult.Success || string.IsNullOrWhiteSpace(customerResult.ProviderCustomerId))
                return Result.Failure<CreateBillingSetupLinkResult>(new("billing.provider_customer_create_failed", customerResult.Error ?? "Failed to create provider customer."));

            providerCustomer = ProviderCustomer.Create(
                IdGenerator.New(IdPrefix.ProviderCustomer),
                connection.Id,
                relationship.BuyerOrganisationId,
                customerResult.ProviderCustomerId);

            await billingRepository.AddProviderCustomerAsync(providerCustomer, ct);
        }

        var setupResult = await paymentProviderGateway.CreatePaymentMethodSetupAsync(
            context,
            new CreatePaymentMethodSetupRequest(
                providerCustomer.ProviderCustomerId,
                request.ReturnUrl,
                request.CancelUrl,
                relationship.Id,
                relationship.SellerOrganisationId,
                relationship.BuyerOrganisationId,
                request.Purpose.ToString()),
            ct);

        if (!setupResult.Success || string.IsNullOrWhiteSpace(setupResult.CheckoutUrl))
            return Result.Failure<CreateBillingSetupLinkResult>(new("billing.setup_link_create_failed", setupResult.Error ?? "Failed to create setup link."));

        if (request.SendEmail)
        {
            try
            {
                var subject = "Complete your billing setup";
                var html = $"<p>Hello {request.RecipientName},</p><p>Use the secure Stripe link below to set up your payment method.</p><p><a href=\"{setupResult.CheckoutUrl}\">Complete Billing Setup</a></p>";
                await emailService.SendAsync(request.RecipientEmail, request.RecipientName, subject, html, ct: ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Billing setup email failed for relationship {BillingRelationshipId}", relationship.Id);
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(new CreateBillingSetupLinkResult(setupResult.CheckoutUrl, providerCustomer.Id));
    }
}

public sealed record InitiateRelationshipTopUpCommand(
    string BillingRelationshipId,
    decimal Amount,
    string CurrencyCode,
    string BillingEmail,
    string BillingName) : IRequest<Result<InitiateTopUpResult>>;

public sealed class InitiateRelationshipTopUpHandler(
    IBillingRepository billingRepository,
    ISecretStore secretStore,
    IPaymentProviderGateway paymentProviderGateway,
    IUnitOfWork unitOfWork)
    : IRequestHandler<InitiateRelationshipTopUpCommand, Result<InitiateTopUpResult>>
{
    public async Task<Result<InitiateTopUpResult>> Handle(InitiateRelationshipTopUpCommand request, CancellationToken ct)
    {
        if (request.Amount <= 0)
            return Result.Failure<InitiateTopUpResult>(new("billing.invalid_amount", "Amount must be greater than zero."));

        var relationship = await billingRepository.GetBillingRelationshipByIdAsync(request.BillingRelationshipId, ct);
        if (relationship is null)
            return Result.Failure<InitiateTopUpResult>(new("billing.relationship_not_found", "Billing relationship was not found."));

        if (relationship.PaymentProviderConnectionId is null)
            return Result.Failure<InitiateTopUpResult>(new("billing.provider_connection_missing", "Billing relationship has no provider connection."));

        var connection = await billingRepository.GetProviderConnectionByIdAsync(relationship.PaymentProviderConnectionId, ct);
        if (connection is null)
            return Result.Failure<InitiateTopUpResult>(new("billing.provider_connection_not_found", "Provider connection was not found."));

        var context = await BillingProviderCommandHelpers.BuildContextAsync(connection, secretStore, ct);

        var providerCustomer = await billingRepository.GetProviderCustomerAsync(connection.Id, relationship.BuyerOrganisationId, ct);
        if (providerCustomer is null)
        {
            var customerResult = await paymentProviderGateway.CreateCustomerAsync(
                context,
                new CreateProviderCustomerRequest(relationship.BuyerOrganisationId, request.BillingEmail, request.BillingName),
                ct);

            if (!customerResult.Success || string.IsNullOrWhiteSpace(customerResult.ProviderCustomerId))
                return Result.Failure<InitiateTopUpResult>(new("billing.provider_customer_create_failed", customerResult.Error ?? "Failed to create provider customer."));

            providerCustomer = ProviderCustomer.Create(
                IdGenerator.New(IdPrefix.ProviderCustomer),
                connection.Id,
                relationship.BuyerOrganisationId,
                customerResult.ProviderCustomerId);

            await billingRepository.AddProviderCustomerAsync(providerCustomer, ct);
        }

        var paymentResult = await paymentProviderGateway.CreatePaymentAsync(
            context,
            new CreateProviderPaymentRequest(
                providerCustomer.ProviderCustomerId,
                ProviderPaymentMethodId: null,
                request.Amount,
                request.CurrencyCode,
                $"Prepaid top-up for relationship {relationship.Id}",
                Confirm: false,
                OffSession: false,
                ManualCapture: false,
                new Dictionary<string, string>
                {
                    ["paymentType"] = "prepaid_topup",
                    ["billingRelationshipId"] = relationship.Id,
                    ["sellerOrganisationId"] = relationship.SellerOrganisationId,
                    ["buyerOrganisationId"] = relationship.BuyerOrganisationId,
                    ["connectionId"] = connection.Id
                }),
            ct);

        if (!paymentResult.Success || string.IsNullOrWhiteSpace(paymentResult.ProviderPaymentId) || string.IsNullOrWhiteSpace(paymentResult.ClientSecret))
            return Result.Failure<InitiateTopUpResult>(new("billing.payment_intent_create_failed", paymentResult.Error ?? "Failed to create payment intent."));

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(new InitiateTopUpResult(paymentResult.ProviderPaymentId, paymentResult.ClientSecret));
    }
}

public sealed record ConfirmRelationshipTopUpCommand(
    string BillingRelationshipId,
    string BuyerOrganisationId,
    decimal Amount,
    string CurrencyCode,
    string ProviderPaymentIntentId) : IRequest<Result>;

public sealed class ConfirmRelationshipTopUpHandler(
    IBillingRepository billingRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ConfirmRelationshipTopUpCommand, Result>
{
    public async Task<Result> Handle(ConfirmRelationshipTopUpCommand request, CancellationToken ct)
    {
        var existingPayment = await billingRepository.GetPaymentByStripePaymentIntentIdAsync(request.ProviderPaymentIntentId, ct);
        if (existingPayment is not null)
            return Result.Success();

        var balance = await billingRepository.GetPrepaidBalanceAsync(request.BuyerOrganisationId, ct);
        if (balance is null)
        {
            balance = PrepaidBalance.Create(IdGenerator.New(IdPrefix.PrepaidBalance), request.BuyerOrganisationId, request.CurrencyCode);
            balance.Credit(request.Amount);
            await billingRepository.AddPrepaidBalanceAsync(balance, ct);
        }
        else
        {
            balance.Credit(request.Amount);
            billingRepository.UpdatePrepaidBalance(balance);
        }

        var payment = Payment.Create(
            IdGenerator.New(IdPrefix.Payment),
            request.BuyerOrganisationId,
            request.Amount,
            request.CurrencyCode,
            PaymentMethod.Card,
            invoiceId: null,
            stripePaymentIntentId: request.ProviderPaymentIntentId);

        await billingRepository.AddPaymentAsync(payment, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}

internal static class ProviderContextFactory
{
    public static async Task<PaymentProviderContext> BuildContextAsync(
        PaymentProviderConnection connection,
        ISecretStore secretStore,
        CancellationToken ct)
    {
        var secretKey = await secretStore.GetSecretValueAsync($"{connection.SecretBundleReference}--secret-key", ct);
        var publishableKey = await secretStore.GetSecretValueAsync($"{connection.SecretBundleReference}--publishable-key", ct);

        string? webhookSecret = null;
        try
        {
            webhookSecret = await secretStore.GetSecretValueAsync(connection.WebhookSecretReference ?? $"{connection.SecretBundleReference}--webhook-secret", ct);
        }
        catch
        {
            // Webhook secret may not exist yet during initial onboarding.
        }

        return new PaymentProviderContext(
            connection.Id,
            connection.OwnerOrganisationId,
            connection.ProviderType,
            connection.IsLiveMode,
            connection.SecretBundleReference,
            secretKey,
            publishableKey,
            webhookSecret);
    }
}

internal static class BillingProviderCommandHelpers
{
    public static Task<PaymentProviderContext> BuildContextAsync(
        PaymentProviderConnection connection,
        ISecretStore secretStore,
        CancellationToken ct)
        => ProviderContextFactory.BuildContextAsync(connection, secretStore, ct);
}

// ─── Q19: Set Primary Provider Connection ───────────────────────────────────

/// <summary>
/// Marks one provider connection as IsPrimary for the org/provider/environment/scope combination.
/// Unsets IsPrimary on any other connections that would conflict (Q19).
/// </summary>
public sealed record SetPrimaryProviderConnectionCommand(
    string ConnectionId,
    string OwnerOrganisationId) : IRequest<Result>;

public sealed class SetPrimaryProviderConnectionHandler(
    IBillingRepository billingRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SetPrimaryProviderConnectionCommand, Result>
{
    public async Task<Result> Handle(SetPrimaryProviderConnectionCommand request, CancellationToken ct)
    {
        var connection = await billingRepository.GetProviderConnectionByIdAsync(request.ConnectionId, ct);
        if (connection is null)
            return Result.Failure(new Error("billing.provider_connection_not_found", "Provider connection not found."));
        if (connection.OwnerOrganisationId != request.OwnerOrganisationId)
            return Result.Failure(new Error("billing.provider_connection_not_found", "Provider connection not found."));
        if (connection.Status != ProviderConnectionStatus.Verified)
            return Result.Failure(new Error("billing.provider_connection_not_verified", "Only verified connections can be set as primary."));

        // Clear IsPrimary on any existing primary for same org/provider/liveMode/scope
        var existing = await billingRepository.GetPrimaryProviderConnectionAsync(
            request.OwnerOrganisationId, connection.ProviderType, connection.IsLiveMode, connection.UsageScope, ct);
        if (existing is not null && existing.Id != request.ConnectionId)
        {
            existing.SetPrimary(false);
            billingRepository.UpdateProviderConnection(existing);
        }

        connection.SetPrimary(true);
        billingRepository.UpdateProviderConnection(connection);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ─── Q16: License Agreement Commands ────────────────────────────────────────

public sealed record CreateLicenseAgreementCommand(
    string SellerOrgId,
    string BuyerOrgId,
    BillingModel BillingModel,
    BillingPeriod BillingPeriod,
    CollectionMode CollectionMode,
    int PaymentTermsDays,
    string AccessPackageCode,
    DateOnly EffectiveFrom,
    string CurrencyCode = "AUD",
    decimal? CreditLimitAmount = null,
    DateOnly? EffectiveTo = null,
    bool RequirePaymentBeforeTicketing = false) : IRequest<Result<string>>;

public sealed class CreateLicenseAgreementHandler(
    IBillingRepository billingRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateLicenseAgreementCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CreateLicenseAgreementCommand request, CancellationToken ct)
    {
        var id = IdGenerator.New(IdPrefix.LicenseAgreement);
        var agreement = LicenseAgreement.Create(
            id,
            request.SellerOrgId,
            request.BuyerOrgId,
            request.BillingModel,
            request.BillingPeriod,
            request.CollectionMode,
            request.PaymentTermsDays,
            request.AccessPackageCode,
            request.EffectiveFrom,
            request.CurrencyCode,
            request.CreditLimitAmount,
            request.EffectiveTo,
            request.RequirePaymentBeforeTicketing);

        await billingRepository.AddLicenseAgreementAsync(agreement, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(id);
    }
}

public sealed record ActivateLicenseAgreementCommand(string LicenseAgreementId) : IRequest<Result>;

public sealed class ActivateLicenseAgreementHandler(
    IBillingRepository billingRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ActivateLicenseAgreementCommand, Result>
{
    public async Task<Result> Handle(ActivateLicenseAgreementCommand request, CancellationToken ct)
    {
        var agreement = await billingRepository.GetLicenseAgreementByIdAsync(request.LicenseAgreementId, ct);
        if (agreement is null)
            return Result.Failure(new Error("billing.license_agreement_not_found", "License agreement not found."));
        if (agreement.Status != LicenseAgreementStatus.Draft)
            return Result.Failure(new Error("billing.license_agreement_not_draft", "Only draft agreements can be activated."));

        agreement.Activate();
        billingRepository.UpdateLicenseAgreement(agreement);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public sealed record SupersedeLicenseAgreementCommand(
    string OldLicenseAgreementId,
    string NewLicenseAgreementId) : IRequest<Result>;

public sealed class SupersedeLicenseAgreementHandler(
    IBillingRepository billingRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SupersedeLicenseAgreementCommand, Result>
{
    public async Task<Result> Handle(SupersedeLicenseAgreementCommand request, CancellationToken ct)
    {
        var old = await billingRepository.GetLicenseAgreementByIdAsync(request.OldLicenseAgreementId, ct);
        if (old is null)
            return Result.Failure(new Error("billing.license_agreement_not_found", "License agreement not found."));
        var replacement = await billingRepository.GetLicenseAgreementByIdAsync(request.NewLicenseAgreementId, ct);
        if (replacement is null)
            return Result.Failure(new Error("billing.license_agreement_not_found", "Replacement license agreement not found."));

        old.SupersedeWith(request.NewLicenseAgreementId);
        billingRepository.UpdateLicenseAgreement(old);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
