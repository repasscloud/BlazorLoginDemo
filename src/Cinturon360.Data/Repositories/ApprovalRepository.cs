using Microsoft.EntityFrameworkCore;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Data.Context;
using Cinturon360.Domain.Entities.Approval;
using Cinturon360.Domain.Enums.Approval;

namespace Cinturon360.Data.Repositories;

internal sealed class ApprovalRepository : IApprovalRepository
{
    private readonly AppDbContext _db;
    public ApprovalRepository(AppDbContext db) => _db = db;

    public Task<ApprovalRequest?> GetByIdAsync(string id, CancellationToken ct)
        => _db.ApprovalRequests.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<ApprovalRequest?> GetActiveForSubjectAsync(string subjectId, CancellationToken ct)
        => _db.ApprovalRequests
            .FirstOrDefaultAsync(x => x.SubjectId == subjectId && x.Status == ApprovalStatus.Pending, ct);

    public async Task<IReadOnlyList<ApprovalRequest>> ListPendingForApproverAsync(string approverUserId, CancellationToken ct)
    {
        // Pending requests from the approver's org — the specific routing is handled at the app layer
        return await _db.ApprovalRequests
            .Where(x => x.Status == ApprovalStatus.Pending)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ApprovalRequest request, CancellationToken ct)
        => await _db.ApprovalRequests.AddAsync(request, ct);

    public void Update(ApprovalRequest request)
        => _db.ApprovalRequests.Update(request);

    public async Task AddDecisionAsync(ApprovalDecision decision, CancellationToken ct)
        => await _db.ApprovalDecisions.AddAsync(decision, ct);

    public async Task<IReadOnlyList<ApprovalDecision>> GetDecisionsAsync(string approvalRequestId, CancellationToken ct)
        => await _db.ApprovalDecisions
            .Where(x => x.ApprovalRequestId == approvalRequestId)
            .OrderBy(x => x.Level)
            .ToListAsync(ct);
}
