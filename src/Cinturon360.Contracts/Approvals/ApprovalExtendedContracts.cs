namespace Cinturon360.Contracts.Approvals;

/// <summary>Request to approve an approval request.</summary>
public sealed record ApproveRequest(string ApproverUserId, string? Comments = null);

/// <summary>Request to reject an approval request.</summary>
public sealed record RejectRequest(string ApproverUserId, string? Comments = null);

/// <summary>List wrapper for pending approvals.</summary>
public sealed record PendingApprovalsResponse(IReadOnlyList<ApprovalRequestResponse> Items);

/// <summary>List wrapper for approval history.</summary>
public sealed record ApprovalHistoryResponse(IReadOnlyList<ApprovalRequestResponse> Items);

/// <summary>Decision history for a single approval request.</summary>
public sealed record ApprovalDecisionResponse(
	string Id,
	int Level,
	string ApproverUserId,
	int Decision,
	string? Comments,
	DateTimeOffset CreatedAt);

/// <summary>Detailed approval response including decision history.</summary>
public sealed record ApprovalDetailResponse(
	ApprovalRequestResponse Approval,
	IReadOnlyList<ApprovalDecisionResponse> Decisions);
