namespace Cinturon360.Contracts.Billing;

public sealed record ConfigureStripeProviderConnectionRequest(
    string OwnerOrganisationId,
    string DisplayName,
    bool IsLiveMode,
    string SecretKey,
    string PublishableKey,
    bool AllowChildOrgBilling,
    bool AllowClientCheckout,
    bool AllowMonthlyInvoiceCollection,
    int UsageScope = 0);

public sealed record ConfigureStripeProviderConnectionResponse(
    string ConnectionId,
    string WebhookPath,
    string SecretBundleReference,
    bool IsVerified);

public sealed record CreateBillingRelationshipRequest(
    string SellerOrganisationId,
    string BuyerOrganisationId,
    string PaymentProviderConnectionId,
    int BillingMode,
    int BillingFrequency,
    int CollectionMode,
    int ChargeTreatment,
    string CurrencyCode,
    int PaymentTermsDays,
    bool AutoCollectPayment,
    bool GenerateInvoices,
    bool RequiresPaymentSetup,
    int CommercialRiskOwner);

public sealed record CreateBillingRelationshipResponse(string BillingRelationshipId);

public sealed record CreateBillingSetupLinkRequest(
    string RecipientEmail,
    string RecipientName,
    string ReturnUrl,
    string CancelUrl,
    int Purpose,
    bool SendEmail = true);

public sealed record CreateBillingSetupLinkResponse(string CheckoutUrl, string ProviderCustomerId);

public sealed record ProviderPaymentMethodResponse(
    string Id,
    string ProviderPaymentMethodId,
    string DisplayName,
    string? Brand,
    string? Last4,
    int? ExpiryMonth,
    int? ExpiryYear);

public sealed record RelationshipTopUpRequest(
    decimal Amount,
    string CurrencyCode,
    string BillingEmail,
    string BillingName);

public sealed record RelationshipTopUpResponse(string PaymentIntentId, string ClientSecret);

public sealed record SetPrimaryProviderConnectionRequest(string OwnerOrganisationId);
