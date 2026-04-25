using Microsoft.EntityFrameworkCore;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Data.Context;
using Cinturon360.Domain.Entities.Policy;

namespace Cinturon360.Data.Repositories;

internal sealed class TravelPolicyRepository : ITravelPolicyRepository
{
    private readonly AppDbContext _db;
    public TravelPolicyRepository(AppDbContext db) => _db = db;

    public Task<TravelPolicy?> GetByIdAsync(string id, CancellationToken ct)
        => _db.TravelPolicies.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<TravelPolicy?> GetDefaultForOrgAsync(string orgId, CancellationToken ct)
        => _db.TravelPolicies.FirstOrDefaultAsync(x => x.OrgId == orgId && x.IsDefault && x.IsActive, ct);

    public async Task<IReadOnlyList<TravelPolicy>> ListForOrgAsync(string orgId, CancellationToken ct)
        => await _db.TravelPolicies.Where(x => x.OrgId == orgId).ToListAsync(ct);

    public async Task AddAsync(TravelPolicy policy, CancellationToken ct)
        => await _db.TravelPolicies.AddAsync(policy, ct);

    public void Update(TravelPolicy policy)
        => _db.TravelPolicies.Update(policy);

    public async Task<IReadOnlyList<PolicyRule>> GetRulesAsync(string policyId, CancellationToken ct)
        => await _db.PolicyRules.Where(x => x.PolicyId == policyId && x.IsActive).ToListAsync(ct);

    public async Task AddRuleAsync(PolicyRule rule, CancellationToken ct)
        => await _db.PolicyRules.AddAsync(rule, ct);

    public void RemoveRule(PolicyRule rule)
        => _db.PolicyRules.Remove(rule);

    public async Task<IReadOnlyList<PolicyAssignment>> GetAssignmentsAsync(string policyId, CancellationToken ct)
        => await _db.PolicyAssignments.Where(x => x.PolicyId == policyId).ToListAsync(ct);

    public async Task AddAssignmentAsync(PolicyAssignment assignment, CancellationToken ct)
        => await _db.PolicyAssignments.AddAsync(assignment, ct);

    public void RemoveAssignment(PolicyAssignment assignment)
        => _db.PolicyAssignments.Remove(assignment);
}
