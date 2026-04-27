using MediatR;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Application.Features.Approvals.Models;
using Cinturon360.Common.Results;
using Cinturon360.Domain.Entities.Approval;

namespace Cinturon360.Application.Features.Approvals.Queries;

// ── Get by ID ─────────────────────────────────────────────────────────────
public sealed record GetApprovalRequestQuery(string ApprovalRequestId) : IRequest<Result<ApprovalRequest?>>;

public sealed class GetApprovalRequestHandler : IRequestHandler<GetApprovalRequestQuery, Result<ApprovalRequest?>>
{
    private readonly IApprovalRepository _repo;
    public GetApprovalRequestHandler(IApprovalRepository repo) => _repo = repo;

    public async Task<Result<ApprovalRequest?>> Handle(GetApprovalRequestQuery request, CancellationToken ct)
    {
        var approval = await _repo.GetByIdAsync(request.ApprovalRequestId, ct);
        return Result.Success(approval);
    }
}

public sealed record GetApprovalDetailQuery(string ApprovalRequestId) : IRequest<Result<ApprovalDetailModel?>>;

public sealed class GetApprovalDetailHandler : IRequestHandler<GetApprovalDetailQuery, Result<ApprovalDetailModel?>>
{
    private readonly IApprovalRepository _repo;

    public GetApprovalDetailHandler(IApprovalRepository repo) => _repo = repo;

    public async Task<Result<ApprovalDetailModel?>> Handle(GetApprovalDetailQuery request, CancellationToken ct)
    {
        var approval = await _repo.GetByIdAsync(request.ApprovalRequestId, ct);
        if (approval is null)
        {
            return Result.Success<ApprovalDetailModel?>(null);
        }

        var decisions = await _repo.GetDecisionsAsync(request.ApprovalRequestId, ct);
        return Result.Success<ApprovalDetailModel?>(new ApprovalDetailModel(approval, decisions));
    }
}

// ── List pending for approver ──────────────────────────────────────────────
public sealed record ListPendingApprovalsQuery(string ApproverUserId) : IRequest<Result<IReadOnlyList<ApprovalRequest>>>;

public sealed class ListPendingApprovalsHandler : IRequestHandler<ListPendingApprovalsQuery, Result<IReadOnlyList<ApprovalRequest>>>
{
    private readonly IApprovalRepository _repo;
    public ListPendingApprovalsHandler(IApprovalRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<ApprovalRequest>>> Handle(ListPendingApprovalsQuery request, CancellationToken ct)
    {
        var approvals = await _repo.ListPendingForApproverAsync(request.ApproverUserId, ct);
        return Result.Success(approvals);
    }
}

public sealed record ListApprovalHistoryQuery(string UserId) : IRequest<Result<IReadOnlyList<ApprovalRequest>>>;

public sealed class ListApprovalHistoryHandler : IRequestHandler<ListApprovalHistoryQuery, Result<IReadOnlyList<ApprovalRequest>>>
{
    private readonly IApprovalRepository _repo;

    public ListApprovalHistoryHandler(IApprovalRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<ApprovalRequest>>> Handle(ListApprovalHistoryQuery request, CancellationToken ct)
    {
        var approvals = await _repo.ListHistoryForUserAsync(request.UserId, ct);
        return Result.Success(approvals);
    }
}
