using Cinturon360.Domain.Entities.Billing;

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
}
