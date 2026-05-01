using MediatR;
using Microsoft.Extensions.Logging;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Application.Abstractions.Services;
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

public sealed class ApproveHandler(
    IApprovalRepository repo,
    IUserRepository userRepo,
    IEmailService email,
    IUnitOfWork uow,
    ILogger<ApproveHandler> logger) : IRequestHandler<ApproveCommand, Result>
{
    public async Task<Result> Handle(ApproveCommand request, CancellationToken ct)
    {
        var approval = await repo.GetByIdAsync(request.ApprovalRequestId, ct);
        if (approval is null) return Result.Failure(ApprovalErrors.NotFound);
        if (approval.Status != ApprovalStatus.Pending) return Result.Failure(ApprovalErrors.AlreadyDone);

        var decisionId = IdGenerator.New(IdPrefix.ApprovalDecision);
        var decision = ApprovalDecision.Create(decisionId, request.ApprovalRequestId,
            approval.CurrentLevel, request.ApproverUserId, ApprovalStatus.Approved, request.Comments);

        await repo.AddDecisionAsync(decision, ct);
        approval.AdvanceLevel();
        repo.Update(approval);
        await uow.SaveChangesAsync(ct);

        // Notify the requester that their request was approved
        var requester = await userRepo.GetByIdAsync(approval.RequestedByUserId, ct);
        if (requester is not null)
        {
            try
            {
                await email.SendAsync(
                    requester.Email,
                    requester.FullName,
                    $"Approval Request Approved — {approval.SubjectId}",
                    $"<p>Hi {requester.FirstName},</p>" +
                    $"<p>Your approval request for <strong>{approval.SubjectId}</strong> has been <strong>approved</strong>.</p>" +
                    (request.Comments is not null ? $"<p>Comments: {request.Comments}</p>" : string.Empty) +
                    "<p>Thank you,<br/>Cinturon360</p>",
                    ct: ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send approval decision email for {ApprovalId}", approval.Id);
            }
        }

        return Result.Success();
    }
}

// ── Reject ────────────────────────────────────────────────────────────────
public sealed record RejectCommand(
    string ApprovalRequestId,
    string ApproverUserId,
    string? Reason) : IRequest<Result>;

public sealed class RejectHandler(
    IApprovalRepository repo,
    IUserRepository userRepo,
    IEmailService email,
    IUnitOfWork uow,
    ILogger<RejectHandler> logger) : IRequestHandler<RejectCommand, Result>
{
    public async Task<Result> Handle(RejectCommand request, CancellationToken ct)
    {
        var approval = await repo.GetByIdAsync(request.ApprovalRequestId, ct);
        if (approval is null) return Result.Failure(ApprovalErrors.NotFound);
        if (approval.Status != ApprovalStatus.Pending) return Result.Failure(ApprovalErrors.AlreadyDone);

        var decisionId = IdGenerator.New(IdPrefix.ApprovalDecision);
        var decision = ApprovalDecision.Create(decisionId, request.ApprovalRequestId,
            approval.CurrentLevel, request.ApproverUserId, ApprovalStatus.Rejected, request.Reason);

        await repo.AddDecisionAsync(decision, ct);
        approval.Reject(request.Reason);
        repo.Update(approval);
        await uow.SaveChangesAsync(ct);

        // Notify the requester that their request was rejected
        var requester = await userRepo.GetByIdAsync(approval.RequestedByUserId, ct);
        if (requester is not null)
        {
            try
            {
                await email.SendAsync(
                    requester.Email,
                    requester.FullName,
                    $"Approval Request Rejected — {approval.SubjectId}",
                    $"<p>Hi {requester.FirstName},</p>" +
                    $"<p>Your approval request for <strong>{approval.SubjectId}</strong> has been <strong>rejected</strong>.</p>" +
                    (request.Reason is not null ? $"<p>Reason: {request.Reason}</p>" : string.Empty) +
                    "<p>Thank you,<br/>Cinturon360</p>",
                    ct: ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send rejection email for {ApprovalId}", approval.Id);
            }
        }

        return Result.Success();
    }
}
