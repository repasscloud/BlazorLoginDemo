using Cinturon360.Domain.Entities.Approval;
using Cinturon360.Domain.Enums.Approval;

namespace Cinturon360.Application.Abstractions.Persistence;

public interface IApprovalRepository
{
    Task<ApprovalRequest?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<ApprovalRequest?> GetActiveForSubjectAsync(string subjectId, CancellationToken ct = default);
    Task<IReadOnlyList<ApprovalRequest>> ListPendingForApproverAsync(string approverUserId, CancellationToken ct = default);
    Task<IReadOnlyList<ApprovalRequest>> ListHistoryForUserAsync(string userId, CancellationToken ct = default);
    Task AddAsync(ApprovalRequest request, CancellationToken ct = default);
    void Update(ApprovalRequest request);

    Task AddDecisionAsync(ApprovalDecision decision, CancellationToken ct = default);
    Task<IReadOnlyList<ApprovalDecision>> GetDecisionsAsync(string approvalRequestId, CancellationToken ct = default);
}
