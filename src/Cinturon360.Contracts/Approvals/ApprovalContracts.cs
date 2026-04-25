namespace Cinturon360.Contracts.Approvals;

public sealed record SubmitForApprovalRequest(
    string OrgId,
    int SubjectType,
    string SubjectId,
    string RequestedByUserId,
    int TotalLevels,
    string? Notes);

public sealed record ApprovalDecisionRequest(
    string ApproverUserId,
    string? Comments);

public sealed record ApprovalRequestResponse(
    string Id,
    string OrgId,
    int SubjectType,
    string SubjectId,
    string RequestedByUserId,
    int Status,
    int TotalLevels,
    int CurrentLevel,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ResolvedAt);
