using Cinturon360.Domain.Entities.Approval;

namespace Cinturon360.Application.Features.Approvals.Models;

public sealed record ApprovalDetailModel(
    ApprovalRequest Approval,
    IReadOnlyList<ApprovalDecision> Decisions);