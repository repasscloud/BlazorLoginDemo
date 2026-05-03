using Cinturon360.Domain.Entities.Billing;
using Cinturon360.Domain.Enums.Billing;

namespace Cinturon360.Application.Abstractions.Persistence;

public interface IBillingRepository
{
    Task<OrgLicense?> GetActiveLicenseAsync(string orgId, CancellationToken ct = default);
    Task<OrgBillingConfig?> GetBillingConfigAsync(string orgId, CancellationToken ct = default);
    Task AddLicenseAsync(OrgLicense license, CancellationToken ct = default);
    void UpdateLicense(OrgLicense license);
    Task AddBillingConfigAsync(OrgBillingConfig config, CancellationToken ct = default);
    void UpdateBillingConfig(OrgBillingConfig config);

    Task<Invoice?> GetInvoiceByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<Invoice>> ListInvoicesAsync(string orgId, int page, int pageSize, CancellationToken ct = default);
    Task AddInvoiceAsync(Invoice invoice, CancellationToken ct = default);
    void UpdateInvoice(Invoice invoice);

    Task<Payment?> GetPaymentByIdAsync(string id, CancellationToken ct = default);
    Task AddPaymentAsync(Payment payment, CancellationToken ct = default);
    void UpdatePayment(Payment payment);

    Task<PrepaidBalance?> GetPrepaidBalanceAsync(string orgId, CancellationToken ct = default);
    Task AddPrepaidBalanceAsync(PrepaidBalance balance, CancellationToken ct = default);
    void UpdatePrepaidBalance(PrepaidBalance balance);

    Task<Payment?> GetPaymentByStripePaymentIntentIdAsync(string stripePaymentIntentId, CancellationToken ct = default);

    Task<PaymentProviderConnection?> GetProviderConnectionByIdAsync(string id, CancellationToken ct = default);
    Task<PaymentProviderConnection?> GetPrimaryProviderConnectionAsync(string ownerOrgId, CancellationToken ct = default);
    Task<PaymentProviderConnection?> GetPrimaryProviderConnectionAsync(string ownerOrgId, PaymentProviderType providerType, bool isLiveMode, ProviderUsageScope usageScope, CancellationToken ct = default);
    Task AddProviderConnectionAsync(PaymentProviderConnection connection, CancellationToken ct = default);
    void UpdateProviderConnection(PaymentProviderConnection connection);

    Task<BillingRelationship?> GetBillingRelationshipByIdAsync(string id, CancellationToken ct = default);
    Task<BillingRelationship?> GetBillingRelationshipAsync(string sellerOrgId, string buyerOrgId, CancellationToken ct = default);
    Task AddBillingRelationshipAsync(BillingRelationship relationship, CancellationToken ct = default);
    void UpdateBillingRelationship(BillingRelationship relationship);

    Task<ProviderCustomer?> GetProviderCustomerAsync(string paymentProviderConnectionId, string buyerOrgId, CancellationToken ct = default);
    Task<ProviderCustomer?> GetProviderCustomerByIdAsync(string id, CancellationToken ct = default);
    Task<ProviderCustomer?> GetProviderCustomerByProviderIdAsync(string paymentProviderConnectionId, string providerCustomerId, CancellationToken ct = default);
    Task AddProviderCustomerAsync(ProviderCustomer providerCustomer, CancellationToken ct = default);
    void UpdateProviderCustomer(ProviderCustomer providerCustomer);

    Task<IReadOnlyList<ProviderPaymentMethod>> ListProviderPaymentMethodsAsync(string providerCustomerId, CancellationToken ct = default);
    Task<ProviderPaymentMethod?> GetProviderPaymentMethodAsync(string providerCustomerId, string providerPaymentMethodId, CancellationToken ct = default);
    Task AddProviderPaymentMethodAsync(ProviderPaymentMethod paymentMethod, CancellationToken ct = default);
    void UpdateProviderPaymentMethod(ProviderPaymentMethod paymentMethod);

    Task<OrganisationBillingProfile?> GetOrganisationBillingProfileAsync(string organisationId, CancellationToken ct = default);
    Task AddOrganisationBillingProfileAsync(OrganisationBillingProfile profile, CancellationToken ct = default);
    void UpdateOrganisationBillingProfile(OrganisationBillingProfile profile);

    Task<ProviderWebhookEvent?> GetProviderWebhookEventAsync(string paymentProviderConnectionId, string providerEventId, CancellationToken ct = default);
    Task AddProviderWebhookEventAsync(ProviderWebhookEvent webhookEvent, CancellationToken ct = default);
    void UpdateProviderWebhookEvent(ProviderWebhookEvent webhookEvent);

    // ── Q16: License Agreements ──────────────────────────────────────────
    Task<LicenseAgreement?> GetLicenseAgreementByIdAsync(string id, CancellationToken ct = default);
    Task AddLicenseAgreementAsync(LicenseAgreement agreement, CancellationToken ct = default);
    void UpdateLicenseAgreement(LicenseAgreement agreement);
}
