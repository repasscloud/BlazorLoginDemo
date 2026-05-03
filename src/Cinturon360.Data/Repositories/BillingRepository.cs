using Microsoft.EntityFrameworkCore;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Data.Context;
using Cinturon360.Domain.Entities.Billing;
using Cinturon360.Domain.Enums.Billing;

namespace Cinturon360.Data.Repositories;

internal sealed class BillingRepository : IBillingRepository
{
    private readonly AppDbContext _db;
    public BillingRepository(AppDbContext db) => _db = db;

    public Task<OrgLicense?> GetActiveLicenseAsync(string orgId, CancellationToken ct)
        => _db.OrgLicenses.FirstOrDefaultAsync(x => x.OrgId == orgId && x.IsActive, ct);

    public Task<OrgBillingConfig?> GetBillingConfigAsync(string orgId, CancellationToken ct)
        => _db.OrgBillingConfigs.FirstOrDefaultAsync(x => x.OrgId == orgId, ct);

    public async Task AddLicenseAsync(OrgLicense license, CancellationToken ct)
        => await _db.OrgLicenses.AddAsync(license, ct);

    public void UpdateLicense(OrgLicense license)
        => _db.OrgLicenses.Update(license);

    public async Task AddBillingConfigAsync(OrgBillingConfig config, CancellationToken ct)
        => await _db.OrgBillingConfigs.AddAsync(config, ct);

    public void UpdateBillingConfig(OrgBillingConfig config)
        => _db.OrgBillingConfigs.Update(config);

    public Task<Invoice?> GetInvoiceByIdAsync(string id, CancellationToken ct)
        => _db.Invoices.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Invoice>> ListInvoicesAsync(string orgId, int page, int pageSize, CancellationToken ct)
        => await _db.Invoices
            .Where(x => x.OrgId == orgId)
            .OrderByDescending(x => x.IssuedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task AddInvoiceAsync(Invoice invoice, CancellationToken ct)
        => await _db.Invoices.AddAsync(invoice, ct);

    public void UpdateInvoice(Invoice invoice)
        => _db.Invoices.Update(invoice);

    public Task<Payment?> GetPaymentByIdAsync(string id, CancellationToken ct)
        => _db.Payments.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Payment?> GetPaymentByStripePaymentIntentIdAsync(string stripePaymentIntentId, CancellationToken ct)
        => _db.Payments.FirstOrDefaultAsync(x => x.StripePaymentIntentId == stripePaymentIntentId, ct);

    public async Task AddPaymentAsync(Payment payment, CancellationToken ct)
        => await _db.Payments.AddAsync(payment, ct);

    public void UpdatePayment(Payment payment)
        => _db.Payments.Update(payment);

    public Task<PrepaidBalance?> GetPrepaidBalanceAsync(string orgId, CancellationToken ct)
        => _db.PrepaidBalances.FirstOrDefaultAsync(x => x.OrgId == orgId, ct);

    public async Task AddPrepaidBalanceAsync(PrepaidBalance balance, CancellationToken ct)
        => await _db.PrepaidBalances.AddAsync(balance, ct);

    public void UpdatePrepaidBalance(PrepaidBalance balance)
        => _db.PrepaidBalances.Update(balance);

    public Task<PaymentProviderConnection?> GetProviderConnectionByIdAsync(string id, CancellationToken ct)
        => _db.PaymentProviderConnections.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<PaymentProviderConnection?> GetPrimaryProviderConnectionAsync(string ownerOrgId, CancellationToken ct)
        => _db.PaymentProviderConnections
            .FirstOrDefaultAsync(
                x => x.OwnerOrganisationId == ownerOrgId
                     && x.ProviderType == PaymentProviderType.Stripe
                     && x.IsEnabled
                     && x.IsPrimary,
                ct);

    public Task<PaymentProviderConnection?> GetPrimaryProviderConnectionAsync(string ownerOrgId, PaymentProviderType providerType, bool isLiveMode, ProviderUsageScope usageScope, CancellationToken ct)
        => _db.PaymentProviderConnections
            .FirstOrDefaultAsync(
                x => x.OwnerOrganisationId == ownerOrgId
                     && x.ProviderType == providerType
                     && x.IsLiveMode == isLiveMode
                     && x.UsageScope == usageScope
                     && x.IsEnabled
                     && x.IsPrimary,
                ct);

    public async Task AddProviderConnectionAsync(PaymentProviderConnection connection, CancellationToken ct)
        => await _db.PaymentProviderConnections.AddAsync(connection, ct);

    public void UpdateProviderConnection(PaymentProviderConnection connection)
        => _db.PaymentProviderConnections.Update(connection);

    public Task<BillingRelationship?> GetBillingRelationshipByIdAsync(string id, CancellationToken ct)
        => _db.BillingRelationships.FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);

    public Task<BillingRelationship?> GetBillingRelationshipAsync(string sellerOrgId, string buyerOrgId, CancellationToken ct)
        => _db.BillingRelationships
            .FirstOrDefaultAsync(x => x.SellerOrganisationId == sellerOrgId && x.BuyerOrganisationId == buyerOrgId && x.IsActive, ct);

    public async Task AddBillingRelationshipAsync(BillingRelationship relationship, CancellationToken ct)
        => await _db.BillingRelationships.AddAsync(relationship, ct);

    public void UpdateBillingRelationship(BillingRelationship relationship)
        => _db.BillingRelationships.Update(relationship);

    public Task<ProviderCustomer?> GetProviderCustomerAsync(string paymentProviderConnectionId, string buyerOrgId, CancellationToken ct)
        => _db.ProviderCustomers
            .FirstOrDefaultAsync(x => x.PaymentProviderConnectionId == paymentProviderConnectionId && x.BuyerOrganisationId == buyerOrgId, ct);

    public Task<ProviderCustomer?> GetProviderCustomerByIdAsync(string id, CancellationToken ct)
        => _db.ProviderCustomers.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<ProviderCustomer?> GetProviderCustomerByProviderIdAsync(string paymentProviderConnectionId, string providerCustomerId, CancellationToken ct)
        => _db.ProviderCustomers
            .FirstOrDefaultAsync(x => x.PaymentProviderConnectionId == paymentProviderConnectionId && x.ProviderCustomerId == providerCustomerId, ct);

    public async Task AddProviderCustomerAsync(ProviderCustomer providerCustomer, CancellationToken ct)
        => await _db.ProviderCustomers.AddAsync(providerCustomer, ct);

    public void UpdateProviderCustomer(ProviderCustomer providerCustomer)
        => _db.ProviderCustomers.Update(providerCustomer);

    public async Task<IReadOnlyList<ProviderPaymentMethod>> ListProviderPaymentMethodsAsync(string providerCustomerId, CancellationToken ct)
        => await _db.ProviderPaymentMethods
            .Where(x => x.ProviderCustomerId == providerCustomerId && x.IsEnabled)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(ct);

    public Task<ProviderPaymentMethod?> GetProviderPaymentMethodAsync(string providerCustomerId, string providerPaymentMethodId, CancellationToken ct)
        => _db.ProviderPaymentMethods
            .FirstOrDefaultAsync(x => x.ProviderCustomerId == providerCustomerId && x.ProviderPaymentMethodId == providerPaymentMethodId, ct);

    public async Task AddProviderPaymentMethodAsync(ProviderPaymentMethod paymentMethod, CancellationToken ct)
        => await _db.ProviderPaymentMethods.AddAsync(paymentMethod, ct);

    public void UpdateProviderPaymentMethod(ProviderPaymentMethod paymentMethod)
        => _db.ProviderPaymentMethods.Update(paymentMethod);

    public Task<OrganisationBillingProfile?> GetOrganisationBillingProfileAsync(string organisationId, CancellationToken ct)
        => _db.OrganisationBillingProfiles.FirstOrDefaultAsync(x => x.OrganisationId == organisationId, ct);

    public async Task AddOrganisationBillingProfileAsync(OrganisationBillingProfile profile, CancellationToken ct)
        => await _db.OrganisationBillingProfiles.AddAsync(profile, ct);

    public void UpdateOrganisationBillingProfile(OrganisationBillingProfile profile)
        => _db.OrganisationBillingProfiles.Update(profile);

    public Task<ProviderWebhookEvent?> GetProviderWebhookEventAsync(string paymentProviderConnectionId, string providerEventId, CancellationToken ct)
        => _db.ProviderWebhookEvents
            .FirstOrDefaultAsync(x => x.PaymentProviderConnectionId == paymentProviderConnectionId && x.ProviderEventId == providerEventId, ct);

    public async Task AddProviderWebhookEventAsync(ProviderWebhookEvent webhookEvent, CancellationToken ct)
        => await _db.ProviderWebhookEvents.AddAsync(webhookEvent, ct);

    public void UpdateProviderWebhookEvent(ProviderWebhookEvent webhookEvent)
        => _db.ProviderWebhookEvents.Update(webhookEvent);

    // ── Q16: License Agreements ──────────────────────────────────────────

    public Task<LicenseAgreement?> GetLicenseAgreementByIdAsync(string id, CancellationToken ct)
        => _db.LicenseAgreements.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddLicenseAgreementAsync(LicenseAgreement agreement, CancellationToken ct)
        => await _db.LicenseAgreements.AddAsync(agreement, ct);

    public void UpdateLicenseAgreement(LicenseAgreement agreement)
        => _db.LicenseAgreements.Update(agreement);
}
