using Cinturon360.Domain.Enums.Billing;

namespace Cinturon360.Application.Abstractions.Services;

public interface IPaymentProviderGateway
{
    PaymentProviderType ProviderType { get; }

    Task<ProviderConnectionVerificationResult> VerifyConnectionAsync(
        PaymentProviderContext context,
        CancellationToken cancellationToken);

    Task<ProviderWebhookRegistrationResult> RegisterWebhookEndpointAsync(
        PaymentProviderContext context,
        string webhookUrl,
        IReadOnlyList<string> enabledEvents,
        CancellationToken cancellationToken);

    Task<CreateProviderCustomerResult> CreateCustomerAsync(
        PaymentProviderContext context,
        CreateProviderCustomerRequest request,
        CancellationToken cancellationToken);

    Task<CreatePaymentMethodSetupResult> CreatePaymentMethodSetupAsync(
        PaymentProviderContext context,
        CreatePaymentMethodSetupRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ProviderPaymentMethodSnapshot>> ListPaymentMethodsAsync(
        PaymentProviderContext context,
        string providerCustomerId,
        CancellationToken cancellationToken);

    Task<CreatePaymentResult> CreatePaymentAsync(
        PaymentProviderContext context,
        CreateProviderPaymentRequest request,
        CancellationToken cancellationToken);

    ParsedProviderWebhookEvent? ParseWebhookEvent(
        string rawBody,
        string signatureHeader,
        string webhookSecret);
}

public sealed record PaymentProviderContext(
    string PaymentProviderConnectionId,
    string OwnerOrganisationId,
    PaymentProviderType ProviderType,
    bool IsLiveMode,
    string SecretBundleReference,
    string SecretKey,
    string PublishableKey,
    string? WebhookSecret);

public sealed record CreateProviderCustomerRequest(string BuyerOrganisationId, string Email, string Name);
public sealed record CreateProviderCustomerResult(bool Success, string? ProviderCustomerId, string? Error);

public sealed record CreatePaymentMethodSetupRequest(
    string ProviderCustomerId,
    string ReturnUrl,
    string CancelUrl,
    string BillingRelationshipId,
    string SellerOrganisationId,
    string BuyerOrganisationId,
    string Purpose);

public sealed record CreatePaymentMethodSetupResult(bool Success, string? CheckoutUrl, string? Error);

public sealed record ProviderPaymentMethodSnapshot(
    string ProviderPaymentMethodId,
    string DisplayName,
    string? Brand,
    string? Last4,
    int? ExpiryMonth,
    int? ExpiryYear,
    string? CardholderName,
    string? Fingerprint,
    bool IsReusable);

public sealed record CreateProviderPaymentRequest(
    string ProviderCustomerId,
    string? ProviderPaymentMethodId,
    decimal Amount,
    string CurrencyCode,
    string Description,
    bool Confirm,
    bool OffSession,
    bool ManualCapture,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record CreatePaymentResult(
    bool Success,
    string? ProviderPaymentId,
    string? ClientSecret,
    string? Status,
    string? Error);

public sealed record ProviderConnectionVerificationResult(
    bool Success,
    string? ProviderAccountId,
    string? ProviderAccountName,
    string? Error);

public sealed record ProviderWebhookRegistrationResult(
    bool Success,
    string? WebhookEndpointId,
    string? WebhookSigningSecret,
    string? Error);

public sealed record ParsedProviderWebhookEvent(
    string ProviderEventId,
    string EventType,
    string? ProviderObjectId,
    decimal? Amount,
    string? Currency,
    string? ProviderCustomerId,
    IReadOnlyDictionary<string, string> Metadata,
    string RawPayloadJson);
