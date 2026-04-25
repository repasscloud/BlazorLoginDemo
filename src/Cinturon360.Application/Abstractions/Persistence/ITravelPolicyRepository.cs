using Cinturon360.Domain.Entities.Policy;

namespace Cinturon360.Application.Abstractions.Persistence;

public interface ITravelPolicyRepository
{
    Task<TravelPolicy?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<TravelPolicy?> GetDefaultForOrgAsync(string orgId, CancellationToken ct = default);
    Task<IReadOnlyList<TravelPolicy>> ListForOrgAsync(string orgId, CancellationToken ct = default);
    Task AddAsync(TravelPolicy policy, CancellationToken ct = default);
    void Update(TravelPolicy policy);

    Task<IReadOnlyList<PolicyRule>> GetRulesAsync(string policyId, CancellationToken ct = default);
    Task AddRuleAsync(PolicyRule rule, CancellationToken ct = default);
    void RemoveRule(PolicyRule rule);

    Task<IReadOnlyList<PolicyAssignment>> GetAssignmentsAsync(string policyId, CancellationToken ct = default);
    Task AddAssignmentAsync(PolicyAssignment assignment, CancellationToken ct = default);
    void RemoveAssignment(PolicyAssignment assignment);
}
