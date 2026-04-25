using MediatR;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Common.IdGeneration;
using Cinturon360.Common.Results;
using Cinturon360.Domain.Entities.Approval;
using Cinturon360.Domain.Enums.Approval;

namespace Cinturon360.Application.Features.Approvals.Commands;

// ── Errors ─────────────────────────────────────────────────────────────────
public static class ApprovalErrors
{
    public static readonly Error NotFound    = new("approval.not_found",    "Approval request not found.");
    public static readonly Error AlreadyDone = new("approval.already_done", "Approval request has already been resolved.");
}

// ── Submit for approval ────────────────────────────────────────────────────
public sealed record SubmitForApprovalCommand(
    string OrgId,
    ApprovalSubjectType SubjectType,
    string SubjectId,
    string RequestedByUserId,
    int TotalLevels,
    string? Notes) : IRequest<Result<string>>;

public sealed class SubmitForApprovalHandler : IRequestHandler<SubmitForApprovalCommand, Result<string>>
{
    private readonly IApprovalRepository _repo;
    private readonly IUnitOfWork _uow;

    public SubmitForApprovalHandler(IApprovalRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result<string>> Handle(SubmitForApprovalCommand request, CancellationToken ct)
    {
        var id = IdGenerator.New(IdPrefix.Approval);
        var approval = ApprovalRequest.Create(id, request.OrgId, request.SubjectType,
            request.SubjectId, request.RequestedByUserId, request.TotalLevels, request.Notes);

        await _repo.AddAsync(approval, ct);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(id);
    }
}

// ── Approve ────────────────────────────────────────────────────────────────
public sealed record ApproveCommand(
    string ApprovalRequestId,
    string ApproverUserId,
    string? Comments) : IRequest<Result>;

public sealed class ApproveHandler : IRequestHandler<ApproveCommand, Result>
{
    private readonly IApprovalRepository _repo;
    private readonly IUnitOfWork _uow;

    public ApproveHandler(IApprovalRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result> Handle(ApproveCommand request, CancellationToken ct)
    {
        var approval = await _repo.GetByIdAsync(request.ApprovalRequestId, ct);
        if (approval is null) return Result.Failure(ApprovalErrors.NotFound);
        if (approval.Status != ApprovalStatus.Pending) return Result.Failure(ApprovalErrors.AlreadyDone);

        var decisionId = IdGenerator.New(IdPrefix.ApprovalDecision);
        var decision = ApprovalDecision.Create(decisionId, request.ApprovalRequestId,
            approval.CurrentLevel, request.ApproverUserId, ApprovalStatus.Approved, request.Comments);

        await _repo.AddDecisionAsync(decision, ct);
        approval.AdvanceLevel();
        _repo.Update(approval);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Reject ────────────────────────────────────────────────────────────────
public sealed record RejectCommand(
    string ApprovalRequestId,
    string ApproverUserId,
    string? Reason) : IRequest<Result>;

public sealed class RejectHandler : IRequestHandler<RejectCommand, Result>
{
    private readonly IApprovalRepository _repo;
    private readonly IUnitOfWork _uow;

    public RejectHandler(IApprovalRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result> Handle(RejectCommand request, CancellationToken ct)
    {
        var approval = await _repo.GetByIdAsync(request.ApprovalRequestId, ct);
        if (approval is null) return Result.Failure(ApprovalErrors.NotFound);
        if (approval.Status != ApprovalStatus.Pending) return Result.Failure(ApprovalErrors.AlreadyDone);

        var decisionId = IdGenerator.New(IdPrefix.ApprovalDecision);
        var decision = ApprovalDecision.Create(decisionId, request.ApprovalRequestId,
            approval.CurrentLevel, request.ApproverUserId, ApprovalStatus.Rejected, request.Reason);

        await _repo.AddDecisionAsync(decision, ct);
        approval.Reject(request.Reason);
        _repo.Update(approval);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
