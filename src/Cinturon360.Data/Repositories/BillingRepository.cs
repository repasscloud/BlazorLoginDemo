using Microsoft.EntityFrameworkCore;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Data.Context;
using Cinturon360.Domain.Entities.Billing;

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
}
