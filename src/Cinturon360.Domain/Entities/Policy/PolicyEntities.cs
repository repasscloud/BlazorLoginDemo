using Cinturon360.Domain.Common.Base;
using Cinturon360.Domain.Enums.Policy;

namespace Cinturon360.Domain.Entities.Policy;

/// <summary>
/// A travel policy belonging to an organisation.
/// Contains a set of rules that control what bookings are allowed.
/// </summary>
public sealed class TravelPolicy : SoftDeletableEntity
{
    public string OrgId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; } = true;

    private TravelPolicy() { }

    public static TravelPolicy Create(string id, string orgId, string name, string? description = null, bool isDefault = false)
        => new()
        {
            Id = id,
            OrgId = orgId,
            Name = name,
            Description = description,
            IsDefault = isDefault,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void Update(string name, string? description) { Name = name; Description = description; UpdatedAt = DateTimeOffset.UtcNow; }
    public void SetDefault(bool isDefault) { IsDefault = isDefault; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Deactivate() { IsActive = false; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Reactivate() { IsActive = true; UpdatedAt = DateTimeOffset.UtcNow; }
}

/// <summary>
/// A single rule within a travel policy (e.g. MaxFare, CabinClassRestriction).
/// </summary>
public sealed class PolicyRule : Entity
{
    public string PolicyId { get; private set; } = string.Empty;
    public PolicyRuleType RuleType { get; private set; }
    public PolicyViolationAction ViolationAction { get; private set; }

    // Generic value store for the rule (e.g. max fare amount, cabin class name)
    public string? ValueString { get; private set; }
    public decimal? ValueDecimal { get; private set; }
    public int? ValueInt { get; private set; }

    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private PolicyRule() { }

    public static PolicyRule Create(
        string id,
        string policyId,
        PolicyRuleType ruleType,
        PolicyViolationAction violationAction,
        string? valueString = null,
        decimal? valueDecimal = null,
        int? valueInt = null,
        string? description = null)
        => new()
        {
            Id = id,
            PolicyId = policyId,
            RuleType = ruleType,
            ViolationAction = violationAction,
            ValueString = valueString,
            ValueDecimal = valueDecimal,
            ValueInt = valueInt,
            Description = description,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void Deactivate() { IsActive = false; UpdatedAt = DateTimeOffset.UtcNow; }
}

/// <summary>
/// Assigns a travel policy to an org, user, or role (scoped assignment).
/// </summary>
public sealed class PolicyAssignment : Entity
{
    public string PolicyId { get; private set; } = string.Empty;
    public PolicyAssignmentTarget TargetType { get; private set; }
    public string TargetId { get; private set; } = string.Empty;  // OrgId, UserId, or RoleId

    private PolicyAssignment() { }

    public static PolicyAssignment Create(string id, string policyId, PolicyAssignmentTarget targetType, string targetId)
        => new()
        {
            Id = id,
            PolicyId = policyId,
            TargetType = targetType,
            TargetId = targetId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
}
