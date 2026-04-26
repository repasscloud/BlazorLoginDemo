namespace Cinturon360.Contracts.Approvals;

/// <summary>Request to approve an approval request.</summary>
public sealed record ApproveRequest(string ApproverUserId, string? Comments = null);

/// <summary>Request to reject an approval request.</summary>
public sealed record RejectRequest(string ApproverUserId, string? Comments = null);

/// <summary>List wrapper for pending approvals.</summary>
public sealed record PendingApprovalsResponse(IReadOnlyList<ApprovalRequestResponse> Items);
