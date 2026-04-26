using Cinturon360.Contracts.Billing;

namespace Cinturon360.Web.Services.ApiClients;

public sealed class BillingApiClient : ApiClientBase
{
    public BillingApiClient(HttpClient http, ILogger<BillingApiClient> logger)
        : base(http, logger) { }

    public Task<ApiResult<OrgLicenseResponse>> GetLicenseAsync(string orgId, CancellationToken ct = default)
        => GetAsync<OrgLicenseResponse>($"api/v1/organisations/{orgId}/billing/license", ct);

    public Task<ApiResult<InvoiceListResponse>> ListInvoicesAsync(string orgId, CancellationToken ct = default)
        => GetAsync<InvoiceListResponse>($"api/v1/organisations/{orgId}/billing/invoices", ct);

    public Task<ApiResult<InvoiceResponse>> CreateInvoiceAsync(string orgId, CreateInvoiceRequest request, CancellationToken ct = default)
        => PostAsync<InvoiceResponse>($"api/v1/organisations/{orgId}/billing/invoices", request, ct);

    public Task<ApiResult<bool>> RecordPaymentAsync(string orgId, string invoiceId, RecordPaymentRequest request, CancellationToken ct = default)
        => PostAsync<bool>($"api/v1/organisations/{orgId}/billing/invoices/{invoiceId}/payment", request, ct);

    public Task<ApiResult<bool>> CreditPrepaidBalanceAsync(string orgId, CreditPrepaidBalanceRequest request, CancellationToken ct = default)
        => PostAsync<bool>($"api/v1/organisations/{orgId}/billing/prepaid/credit", request, ct);
}
