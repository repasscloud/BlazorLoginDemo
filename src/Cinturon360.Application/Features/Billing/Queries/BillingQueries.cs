using MediatR;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Common.Results;
using Cinturon360.Domain.Entities.Billing;

namespace Cinturon360.Application.Features.Billing.Queries;

// ── Get active license ────────────────────────────────────────────────────
public sealed record GetOrgLicenseQuery(string OrgId) : IRequest<Result<OrgLicense?>>;

public sealed class GetOrgLicenseHandler : IRequestHandler<GetOrgLicenseQuery, Result<OrgLicense?>>
{
    private readonly IBillingRepository _repo;
    public GetOrgLicenseHandler(IBillingRepository repo) => _repo = repo;

    public async Task<Result<OrgLicense?>> Handle(GetOrgLicenseQuery request, CancellationToken ct)
    {
        var license = await _repo.GetActiveLicenseAsync(request.OrgId, ct);
        return Result.Success(license);
    }
}

// ── List invoices ─────────────────────────────────────────────────────────
public sealed record ListInvoicesQuery(string OrgId, int Page, int PageSize) : IRequest<Result<IReadOnlyList<Invoice>>>;

public sealed class ListInvoicesHandler : IRequestHandler<ListInvoicesQuery, Result<IReadOnlyList<Invoice>>>
{
    private readonly IBillingRepository _repo;
    public ListInvoicesHandler(IBillingRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<Invoice>>> Handle(ListInvoicesQuery request, CancellationToken ct)
    {
        var invoices = await _repo.ListInvoicesAsync(request.OrgId, request.Page, request.PageSize, ct);
        return Result.Success(invoices);
    }
}
