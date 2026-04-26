namespace Cinturon360.Contracts.Billing;

/// <summary>List wrapper for invoices.</summary>
public sealed record InvoiceListResponse(
    IReadOnlyList<InvoiceResponse> Items,
    int Total,
    int Page,
    int PageSize);
