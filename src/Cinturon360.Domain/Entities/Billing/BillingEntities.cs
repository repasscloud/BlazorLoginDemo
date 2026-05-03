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

/// <summary>Seller-owned payment provider connection, e.g. a TMC Stripe account.</summary>
public sealed class PaymentProviderConnection : Entity
{
    public string OwnerOrganisationId { get; private set; } = string.Empty;
    public PaymentProviderType ProviderType { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public bool IsLiveMode { get; private set; }
    public bool IsEnabled { get; private set; }
    public ProviderConnectionStatus Status { get; private set; }
    public string SecretBundleReference { get; private set; } = string.Empty;
    public string? ProviderAccountId { get; private set; }
    public string? ProviderAccountName { get; private set; }
    public string? WebhookEndpointId { get; private set; }
    public string? WebhookSecretReference { get; private set; }
    public ProviderUsageScope UsageScope { get; private set; }
    public bool AllowChildOrgBilling { get; private set; }
    public bool AllowClientCheckout { get; private set; }
    public bool AllowMonthlyInvoiceCollection { get; private set; }
    /// <summary>True if this is the primary connection for org-scoped webhook routing (Q19).</summary>
    public bool IsPrimary { get; private set; }
    public DateTimeOffset? VerifiedAt { get; private set; }
    public DateTimeOffset? WebhookProvisionedAtUtc { get; private set; }
    public DateTimeOffset? LastRotatedAt { get; private set; }
    public DateTimeOffset? LastUsedAt { get; private set; }
    public DateTimeOffset? LastWebhookReceivedAt { get; private set; }
    public DateTimeOffset? DisabledAt { get; private set; }

    private PaymentProviderConnection() { }

    public static PaymentProviderConnection Create(
        string id,
        string ownerOrganisationId,
        PaymentProviderType providerType,
        string displayName,
        bool isLiveMode,
        string secretBundleReference,
        ProviderUsageScope usageScope,
        bool allowChildOrgBilling,
        bool allowClientCheckout,
        bool allowMonthlyInvoiceCollection)
        => new()
        {
            Id = id,
            OwnerOrganisationId = ownerOrganisationId,
            ProviderType = providerType,
            DisplayName = displayName,
            IsLiveMode = isLiveMode,
            IsEnabled = true,
            Status = ProviderConnectionStatus.PendingVerification,
            SecretBundleReference = secretBundleReference,
            UsageScope = usageScope,
            AllowChildOrgBilling = allowChildOrgBilling,
            AllowClientCheckout = allowClientCheckout,
            AllowMonthlyInvoiceCollection = allowMonthlyInvoiceCollection,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void MarkVerified(string? providerAccountId, string? providerAccountName)
    {
        ProviderAccountId = providerAccountId;
        ProviderAccountName = providerAccountName;
        Status = ProviderConnectionStatus.Verified;
        VerifiedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetWebhook(string? endpointId, string? secretReference)
    {
        WebhookEndpointId = endpointId;
        WebhookSecretReference = secretReference;
        WebhookProvisionedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkUsed() { LastUsedAt = DateTimeOffset.UtcNow; UpdatedAt = DateTimeOffset.UtcNow; }
    public void MarkWebhookReceived() { LastWebhookReceivedAt = DateTimeOffset.UtcNow; UpdatedAt = DateTimeOffset.UtcNow; }

    public void Disable()
    {
        IsEnabled = false;
        Status = ProviderConnectionStatus.Disabled;
        DisabledAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

/// <summary>Commercial billing relationship between seller and buyer organisations.</summary>
public sealed class BillingRelationship : Entity
{
    public string SellerOrganisationId { get; private set; } = string.Empty;
    public string BuyerOrganisationId { get; private set; } = string.Empty;
    public string? OrgLicenseId { get; private set; }
    public string? PaymentProviderConnectionId { get; private set; }
    public BillingMode BillingMode { get; private set; }
    public BillingFrequency BillingFrequency { get; private set; }
    public CollectionMode CollectionMode { get; private set; }
    public ChargeTreatment ChargeTreatment { get; private set; }
    public bool GenerateInvoices { get; private set; }
    public bool AutoCollectPayment { get; private set; }
    public bool RequiresPaymentSetup { get; private set; }
    public bool IsBillingSetupComplete { get; private set; }
    public string CurrencyCode { get; private set; } = "USD";
    public int PaymentTermsDays { get; private set; }
    public decimal? CreditLimitAmount { get; private set; }
    public decimal? MaxSingleBookingAmount { get; private set; }
    public bool BlockBookingsWhenOverdue { get; private set; }
    public bool RequirePaymentBeforeTicketing { get; private set; }
    public CommercialRiskOwner CommercialRiskOwner { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public bool IsActive { get; private set; }

    private BillingRelationship() { }

    public static BillingRelationship Create(
        string id,
        string sellerOrganisationId,
        string buyerOrganisationId,
        string? paymentProviderConnectionId,
        BillingMode billingMode,
        BillingFrequency billingFrequency,
        CollectionMode collectionMode,
        ChargeTreatment chargeTreatment,
        string currencyCode,
        int paymentTermsDays,
        bool autoCollectPayment,
        bool generateInvoices,
        bool requiresPaymentSetup,
        CommercialRiskOwner commercialRiskOwner)
        => new()
        {
            Id = id,
            SellerOrganisationId = sellerOrganisationId,
            BuyerOrganisationId = buyerOrganisationId,
            PaymentProviderConnectionId = paymentProviderConnectionId,
            BillingMode = billingMode,
            BillingFrequency = billingFrequency,
            CollectionMode = collectionMode,
            ChargeTreatment = chargeTreatment,
            CurrencyCode = currencyCode,
            PaymentTermsDays = paymentTermsDays,
            AutoCollectPayment = autoCollectPayment,
            GenerateInvoices = generateInvoices,
            RequiresPaymentSetup = requiresPaymentSetup,
            CommercialRiskOwner = commercialRiskOwner,
            EffectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void MarkSetupComplete()
    {
        IsBillingSetupComplete = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

/// <summary>Buyer org as customer inside a seller-owned provider account.</summary>
public sealed class ProviderCustomer : Entity
{
    public string PaymentProviderConnectionId { get; private set; } = string.Empty;
    public string BuyerOrganisationId { get; private set; } = string.Empty;
    public string ProviderCustomerId { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }
    public bool HasUsablePaymentMethod { get; private set; }
    public DateTimeOffset? SetupCompletedAt { get; private set; }
    public DateTimeOffset? LastPaymentSucceededAt { get; private set; }
    public DateTimeOffset? LastPaymentFailedAt { get; private set; }

    private ProviderCustomer() { }

    public static ProviderCustomer Create(
        string id,
        string paymentProviderConnectionId,
        string buyerOrganisationId,
        string providerCustomerId)
        => new()
        {
            Id = id,
            PaymentProviderConnectionId = paymentProviderConnectionId,
            BuyerOrganisationId = buyerOrganisationId,
            ProviderCustomerId = providerCustomerId,
            IsEnabled = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void MarkSetupComplete(bool hasUsablePaymentMethod)
    {
        HasUsablePaymentMethod = hasUsablePaymentMethod;
        SetupCompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

/// <summary>Saved provider payment method reference for billing and travel policies.</summary>
public sealed class ProviderPaymentMethod : Entity
{
    public string ProviderCustomerId { get; private set; } = string.Empty;
    public PaymentProviderType ProviderType { get; private set; }
    public string ProviderPaymentMethodId { get; private set; } = string.Empty;
    public string? ProviderSetupIntentId { get; private set; }
    public PaymentMethodPurpose Purpose { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public string? Brand { get; private set; }
    public string? Last4 { get; private set; }
    public int? ExpiryMonth { get; private set; }
    public int? ExpiryYear { get; private set; }
    public string? CardholderName { get; private set; }
    public string? Fingerprint { get; private set; }
    public string? BillingCountry { get; private set; }
    public string? BillingEmail { get; private set; }
    public bool IsDefaultForAccountFees { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTimeOffset AddedAt { get; private set; }
    public DateTimeOffset? VerifiedAt { get; private set; }
    public DateTimeOffset? DisabledAt { get; private set; }

    private ProviderPaymentMethod() { }

    public static ProviderPaymentMethod Create(
        string id,
        string providerCustomerId,
        PaymentProviderType providerType,
        string providerPaymentMethodId,
        PaymentMethodPurpose purpose,
        string displayName,
        string? brand,
        string? last4,
        int? expiryMonth,
        int? expiryYear,
        string? cardholderName,
        string? fingerprint)
        => new()
        {
            Id = id,
            ProviderCustomerId = providerCustomerId,
            ProviderType = providerType,
            ProviderPaymentMethodId = providerPaymentMethodId,
            Purpose = purpose,
            DisplayName = displayName,
            Brand = brand,
            Last4 = last4,
            ExpiryMonth = expiryMonth,
            ExpiryYear = expiryYear,
            CardholderName = cardholderName,
            Fingerprint = fingerprint,
            IsEnabled = true,
            AddedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void SyncDisplay(string displayName, string? brand, string? last4, int? expiryMonth, int? expiryYear)
    {
        DisplayName = displayName;
        Brand = brand;
        Last4 = last4;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        VerifiedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

/// <summary>Default organisation-level billing profile used by policy resolution.</summary>
public sealed class OrganisationBillingProfile : Entity
{
    public string OrganisationId { get; private set; } = string.Empty;
    public string SellerOrganisationId { get; private set; } = string.Empty;
    public string PaymentProviderConnectionId { get; private set; } = string.Empty;
    public string? DefaultProviderCustomerId { get; private set; }
    public string? DefaultProviderPaymentMethodId { get; private set; }
    public BillingMode DefaultBillingMode { get; private set; }
    public BillingFrequency DefaultBillingFrequency { get; private set; }
    public CollectionMode DefaultCollectionMode { get; private set; }
    public ChargeTreatment DefaultChargeTreatment { get; private set; }
    public string CurrencyCode { get; private set; } = "USD";
    public bool IsBillingSetupComplete { get; private set; }

    private OrganisationBillingProfile() { }

    public static OrganisationBillingProfile Create(
        string id,
        string organisationId,
        string sellerOrganisationId,
        string paymentProviderConnectionId,
        BillingMode billingMode,
        BillingFrequency billingFrequency,
        CollectionMode collectionMode,
        ChargeTreatment chargeTreatment,
        string currencyCode)
        => new()
        {
            Id = id,
            OrganisationId = organisationId,
            SellerOrganisationId = sellerOrganisationId,
            PaymentProviderConnectionId = paymentProviderConnectionId,
            DefaultBillingMode = billingMode,
            DefaultBillingFrequency = billingFrequency,
            DefaultCollectionMode = collectionMode,
            DefaultChargeTreatment = chargeTreatment,
            CurrencyCode = currencyCode,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void SetDefaultPaymentMethod(string? providerCustomerId, string? providerPaymentMethodId)
    {
        DefaultProviderCustomerId = providerCustomerId;
        DefaultProviderPaymentMethodId = providerPaymentMethodId;
        IsBillingSetupComplete = !string.IsNullOrWhiteSpace(providerCustomerId)
            && !string.IsNullOrWhiteSpace(providerPaymentMethodId);
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Shared billing rule for any policy domain (travel, expense, etc.).
/// Uses PolicyType discriminator to identify domain (Q18).
/// </summary>
public sealed class PolicyBillingRule : Entity
{
    public string OrganisationId { get; private set; } = string.Empty;
    /// <summary>The domain of the policy (Travel, Expense, etc.).</summary>
    public PolicyType PolicyType { get; private set; }
    /// <summary>The ID of the policy within its domain (TravelPolicyId, ExpensePolicyId, etc.).</summary>
    public string PolicyId { get; private set; } = string.Empty;
    public BillingResolutionMode ResolutionMode { get; private set; }
    public string? ProviderPaymentMethodId { get; private set; }
    public string? PaymentProviderConnectionId { get; private set; }
    public string? ProviderCustomerId { get; private set; }
    public BillingMode? BillingModeOverride { get; private set; }
    public CollectionMode? CollectionModeOverride { get; private set; }
    public bool RequirePaymentBeforeExecution { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }

    private PolicyBillingRule() { }

    public static PolicyBillingRule Create(
        string id,
        string organisationId,
        PolicyType policyType,
        string policyId,
        BillingResolutionMode resolutionMode,
        string? providerPaymentMethodId = null,
        string? paymentProviderConnectionId = null,
        string? providerCustomerId = null)
        => new()
        {
            Id = id,
            OrganisationId = organisationId,
            PolicyType = policyType,
            PolicyId = policyId,
            ResolutionMode = resolutionMode,
            ProviderPaymentMethodId = providerPaymentMethodId,
            PaymentProviderConnectionId = paymentProviderConnectionId,
            ProviderCustomerId = providerCustomerId,
            IsEnabled = true,
            EffectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void SetEnabled(bool enabled) { IsEnabled = enabled; UpdatedAt = DateTimeOffset.UtcNow; }
    public void SetEffectiveTo(DateOnly effectiveTo) { EffectiveTo = effectiveTo; UpdatedAt = DateTimeOffset.UtcNow; }
}

/// <summary>Provider webhook event persistence for idempotency and audit.</summary>
public sealed class ProviderWebhookEvent : Entity
{
    public string PaymentProviderConnectionId { get; private set; } = string.Empty;
    public PaymentProviderType ProviderType { get; private set; }
    public string ProviderEventId { get; private set; } = string.Empty;
    public string EventType { get; private set; } = string.Empty;
    public string RawPayloadJson { get; private set; } = string.Empty;
    public WebhookProcessingStatus Status { get; private set; }
    public DateTimeOffset ReceivedAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    private ProviderWebhookEvent() { }

    public static ProviderWebhookEvent Create(
        string id,
        string paymentProviderConnectionId,
        PaymentProviderType providerType,
        string providerEventId,
        string eventType,
        string rawPayloadJson)
        => new()
        {
            Id = id,
            PaymentProviderConnectionId = paymentProviderConnectionId,
            ProviderType = providerType,
            ProviderEventId = providerEventId,
            EventType = eventType,
            RawPayloadJson = rawPayloadJson,
            Status = WebhookProcessingStatus.Pending,
            ReceivedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void MarkProcessed()
    {
        Status = WebhookProcessingStatus.Processed;
        ProcessedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string error)
    {
        Status = WebhookProcessingStatus.Failed;
        ErrorMessage = error;
        ProcessedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

// ─── Q16: License Agreement & Entitlement ───────────────────────────────────

/// <summary>
/// Seller-owned commercial contract defining billing, access, entitlement,
/// credit, and collection rules between a seller and buyer org (Q16).
/// This is the source of truth for billing execution.
/// </summary>
public sealed class LicenseAgreement : Entity
{
    public string SellerOrgId { get; private set; } = string.Empty;
    public string BuyerOrgId { get; private set; } = string.Empty;
    public BillingModel BillingModel { get; private set; }
    public BillingPeriod BillingPeriod { get; private set; }
    public CollectionMode CollectionMode { get; private set; }
    public int PaymentTermsDays { get; private set; }
    public decimal? CreditLimitAmount { get; private set; }
    public string CurrencyCode { get; private set; } = "AUD";
    public bool RequirePaymentBeforeTicketing { get; private set; }
    public string AccessPackageCode { get; private set; } = string.Empty;
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public LicenseAgreementStatus Status { get; private set; } = LicenseAgreementStatus.Draft;
    public int VersionNumber { get; private set; } = 1;
    public string? PreviousLicenseAgreementId { get; private set; }
    public string? SupersededByLicenseAgreementId { get; private set; }

    private LicenseAgreement() { }

    public static LicenseAgreement Create(
        string id,
        string sellerOrgId,
        string buyerOrgId,
        BillingModel billingModel,
        BillingPeriod billingPeriod,
        CollectionMode collectionMode,
        int paymentTermsDays,
        string accessPackageCode,
        DateOnly effectiveFrom,
        string currencyCode = "AUD",
        decimal? creditLimitAmount = null,
        DateOnly? effectiveTo = null,
        bool requirePaymentBeforeTicketing = false)
        => new()
        {
            Id = id,
            SellerOrgId = sellerOrgId,
            BuyerOrgId = buyerOrgId,
            BillingModel = billingModel,
            BillingPeriod = billingPeriod,
            CollectionMode = collectionMode,
            PaymentTermsDays = paymentTermsDays,
            AccessPackageCode = accessPackageCode,
            EffectiveFrom = effectiveFrom,
            CurrencyCode = currencyCode,
            CreditLimitAmount = creditLimitAmount,
            EffectiveTo = effectiveTo,
            RequirePaymentBeforeTicketing = requirePaymentBeforeTicketing,
            Status = LicenseAgreementStatus.Draft,
            VersionNumber = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void Activate()
    {
        Status = LicenseAgreementStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Suspend()
    {
        Status = LicenseAgreementStatus.Suspended;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SupersedeWith(string newAgreementId)
    {
        SupersededByLicenseAgreementId = newAgreementId;
        Status = LicenseAgreementStatus.Superseded;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

/// <summary>An individual entitlement line within a LicenseAgreement (Q16).</summary>
public sealed class LicenseAgreementEntitlement : Entity
{
    public string LicenseAgreementId { get; private set; } = string.Empty;
    public EntitlementType Type { get; private set; }
    public EntitlementValueKind ValueKind { get; private set; }
    public bool? BooleanValue { get; private set; }
    public decimal? NumericValue { get; private set; }
    public string? TextValue { get; private set; }
    public bool IsUnlimited { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? Notes { get; private set; }

    private LicenseAgreementEntitlement() { }

    public static LicenseAgreementEntitlement CreateBoolean(
        string id, string licenseAgreementId, EntitlementType type, bool value, DateOnly effectiveFrom)
        => new()
        {
            Id = id, LicenseAgreementId = licenseAgreementId, Type = type,
            ValueKind = EntitlementValueKind.Boolean, BooleanValue = value,
            EffectiveFrom = effectiveFrom, IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow
        };

    public static LicenseAgreementEntitlement CreateQuantity(
        string id, string licenseAgreementId, EntitlementType type, decimal quantity, DateOnly effectiveFrom, bool isUnlimited = false)
        => new()
        {
            Id = id, LicenseAgreementId = licenseAgreementId, Type = type,
            ValueKind = EntitlementValueKind.Quantity, NumericValue = quantity, IsUnlimited = isUnlimited,
            EffectiveFrom = effectiveFrom, IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow
        };

    public void Deactivate() { IsActive = false; UpdatedAt = DateTimeOffset.UtcNow; }
}

/// <summary>Collection policy rules attached to a LicenseAgreement (Q16).</summary>
public sealed class LicenseCollectionPolicy : Entity
{
    public string LicenseAgreementId { get; private set; } = string.Empty;
    public int PaymentTermsDays { get; private set; }
    public int GracePeriodDays { get; private set; }
    public bool BlockBookingsWhenOverdue { get; private set; }
    public bool RequirePaymentBeforeTicketing { get; private set; }
    public CollectionActionAfterGrace ActionAfterGrace { get; private set; }

    private LicenseCollectionPolicy() { }

    public static LicenseCollectionPolicy Create(
        string id,
        string licenseAgreementId,
        int paymentTermsDays,
        int gracePeriodDays,
        bool blockBookingsWhenOverdue,
        bool requirePaymentBeforeTicketing,
        CollectionActionAfterGrace actionAfterGrace)
        => new()
        {
            Id = id,
            LicenseAgreementId = licenseAgreementId,
            PaymentTermsDays = paymentTermsDays,
            GracePeriodDays = gracePeriodDays,
            BlockBookingsWhenOverdue = blockBookingsWhenOverdue,
            RequirePaymentBeforeTicketing = requirePaymentBeforeTicketing,
            ActionAfterGrace = actionAfterGrace,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
}

// ─── Q16: Billing Account ───────────────────────────────────────────────────

/// <summary>Billing account linking an org to a LicenseAgreement with runtime status (Q16).</summary>
public sealed class BillingAccount : Entity
{
    public string OwnerOrgId { get; private set; } = string.Empty;
    public string? LicenseAgreementId { get; private set; }
    public BillingModel BillingModel { get; private set; }
    public BillingAccountStatus AccountStatus { get; private set; } = BillingAccountStatus.Active;
    public string CurrencyCode { get; private set; } = "AUD";

    private BillingAccount() { }

    public static BillingAccount Create(
        string id,
        string ownerOrgId,
        BillingModel billingModel,
        string currencyCode,
        string? licenseAgreementId = null)
        => new()
        {
            Id = id,
            OwnerOrgId = ownerOrgId,
            BillingModel = billingModel,
            CurrencyCode = currencyCode,
            LicenseAgreementId = licenseAgreementId,
            AccountStatus = BillingAccountStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void Suspend() { AccountStatus = BillingAccountStatus.Suspended; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Activate() { AccountStatus = BillingAccountStatus.Active; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Close() { AccountStatus = BillingAccountStatus.Closed; UpdatedAt = DateTimeOffset.UtcNow; }
}

// ─── Q16: Accounting Stub Entities ─────────────────────────────────────────
// Stubs only — tables created, no application logic yet.

/// <summary>Billing ledger entry for prepaid/postpaid accounting (Q16 stub).</summary>
public sealed class BillingLedgerEntry : Entity
{
    public string BillingAccountId { get; private set; } = string.Empty;
    public string? LicenseAgreementId { get; private set; }
    public string? RelatedEntityId { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public decimal Amount { get; private set; }
    public string CurrencyCode { get; private set; } = "AUD";
    public string Description { get; private set; } = string.Empty;
    public DateTimeOffset EntryDateUtc { get; private set; }

    private BillingLedgerEntry() { }

    public static BillingLedgerEntry Create(
        string id,
        string billingAccountId,
        decimal amount,
        string currencyCode,
        string description,
        string? relatedEntityId = null,
        string? relatedEntityType = null,
        string? licenseAgreementId = null)
        => new()
        {
            Id = id,
            BillingAccountId = billingAccountId,
            LicenseAgreementId = licenseAgreementId,
            Amount = amount,
            CurrencyCode = currencyCode,
            Description = description,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            EntryDateUtc = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
}

/// <summary>Invoice issued under a LicenseAgreement (Q16 stub).</summary>
public sealed class BillingInvoice : Entity
{
    public string SellerOrgId { get; private set; } = string.Empty;
    public string BuyerOrgId { get; private set; } = string.Empty;
    public string? LicenseAgreementId { get; private set; }
    public string InvoiceNumber { get; private set; } = string.Empty;
    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;
    public decimal SubtotalAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string CurrencyCode { get; private set; } = "AUD";
    public DateOnly IssuedOn { get; private set; }
    public DateOnly DueOn { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }
    public string? Notes { get; private set; }

    private BillingInvoice() { }

    public static BillingInvoice Create(
        string id,
        string sellerOrgId,
        string buyerOrgId,
        string invoiceNumber,
        decimal subtotal,
        decimal tax,
        string currencyCode,
        DateOnly issuedOn,
        DateOnly dueOn,
        string? licenseAgreementId = null)
        => new()
        {
            Id = id,
            SellerOrgId = sellerOrgId,
            BuyerOrgId = buyerOrgId,
            LicenseAgreementId = licenseAgreementId,
            InvoiceNumber = invoiceNumber,
            SubtotalAmount = subtotal,
            TaxAmount = tax,
            TotalAmount = subtotal + tax,
            CurrencyCode = currencyCode,
            IssuedOn = issuedOn,
            DueOn = dueOn,
            Status = InvoiceStatus.Draft,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void Send() { Status = InvoiceStatus.Sent; UpdatedAt = DateTimeOffset.UtcNow; }
    public void MarkPaid() { Status = InvoiceStatus.Paid; PaidAt = DateTimeOffset.UtcNow; UpdatedAt = DateTimeOffset.UtcNow; }
}

/// <summary>Individual line item within a BillingInvoice (Q16 stub).</summary>
public sealed class BillingInvoiceLine : Entity
{
    public string BillingInvoiceId { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal UnitAmount { get; private set; }
    public int Quantity { get; private set; } = 1;
    public decimal TaxAmount { get; private set; }
    public decimal LineTotal { get; private set; }
    public string? RelatedEntityId { get; private set; }
    public string? RelatedEntityType { get; private set; }

    private BillingInvoiceLine() { }

    public static BillingInvoiceLine Create(
        string id,
        string billingInvoiceId,
        string description,
        decimal unitAmount,
        int quantity,
        decimal taxAmount,
        string? relatedEntityId = null,
        string? relatedEntityType = null)
        => new()
        {
            Id = id,
            BillingInvoiceId = billingInvoiceId,
            Description = description,
            UnitAmount = unitAmount,
            Quantity = quantity,
            TaxAmount = taxAmount,
            LineTotal = (unitAmount * quantity) + taxAmount,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
}

/// <summary>Payment attempt against a BillingInvoice or top-up (Q16 stub).</summary>
public sealed class PaymentAttempt : Entity
{
    public string? BillingInvoiceId { get; private set; }
    public string? BillingAccountId { get; private set; }
    public string? PaymentProviderConnectionId { get; private set; }
    public string? ProviderPaymentIntentId { get; private set; }
    public decimal Amount { get; private set; }
    public string CurrencyCode { get; private set; } = "AUD";
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public string? FailureReason { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }

    private PaymentAttempt() { }

    public static PaymentAttempt Create(
        string id,
        decimal amount,
        string currencyCode,
        string? billingInvoiceId = null,
        string? billingAccountId = null,
        string? paymentProviderConnectionId = null,
        string? providerPaymentIntentId = null)
        => new()
        {
            Id = id,
            BillingInvoiceId = billingInvoiceId,
            BillingAccountId = billingAccountId,
            PaymentProviderConnectionId = paymentProviderConnectionId,
            ProviderPaymentIntentId = providerPaymentIntentId,
            Amount = amount,
            CurrencyCode = currencyCode,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void MarkSucceeded() { Status = PaymentStatus.Succeeded; ProcessedAt = DateTimeOffset.UtcNow; UpdatedAt = DateTimeOffset.UtcNow; }
    public void MarkFailed(string reason) { Status = PaymentStatus.Failed; FailureReason = reason; ProcessedAt = DateTimeOffset.UtcNow; UpdatedAt = DateTimeOffset.UtcNow; }
}

/// <summary>Double-entry journal entry header (Q16 stub — DR/CR accounting support).</summary>
public sealed class JournalEntry : Entity
{
    public string? LicenseAgreementId { get; private set; }
    public string? BillingInvoiceId { get; private set; }
    public string? PaymentAttemptId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateOnly EntryDate { get; private set; }

    private JournalEntry() { }

    public static JournalEntry Create(
        string id,
        string description,
        DateOnly entryDate,
        string? licenseAgreementId = null,
        string? billingInvoiceId = null,
        string? paymentAttemptId = null)
        => new()
        {
            Id = id,
            Description = description,
            EntryDate = entryDate,
            LicenseAgreementId = licenseAgreementId,
            BillingInvoiceId = billingInvoiceId,
            PaymentAttemptId = paymentAttemptId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
}

/// <summary>Individual debit or credit line within a JournalEntry (Q16 stub).</summary>
public sealed class JournalLine : Entity
{
    public string JournalEntryId { get; private set; } = string.Empty;
    public string AccountCode { get; private set; } = string.Empty;
    public string AccountName { get; private set; } = string.Empty;
    public decimal DebitAmount { get; private set; }
    public decimal CreditAmount { get; private set; }
    public string CurrencyCode { get; private set; } = "AUD";

    private JournalLine() { }

    public static JournalLine CreateDebit(
        string id, string journalEntryId, string accountCode, string accountName, decimal amount, string currencyCode)
        => new()
        {
            Id = id,
            JournalEntryId = journalEntryId,
            AccountCode = accountCode,
            AccountName = accountName,
            DebitAmount = amount,
            CreditAmount = 0,
            CurrencyCode = currencyCode,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public static JournalLine CreateCredit(
        string id, string journalEntryId, string accountCode, string accountName, decimal amount, string currencyCode)
        => new()
        {
            Id = id,
            JournalEntryId = journalEntryId,
            AccountCode = accountCode,
            AccountName = accountName,
            DebitAmount = 0,
            CreditAmount = amount,
            CurrencyCode = currencyCode,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
}
