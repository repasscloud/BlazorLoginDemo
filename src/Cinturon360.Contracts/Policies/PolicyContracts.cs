namespace Cinturon360.Contracts.Policies;

public sealed record CreateTravelPolicyRequest(
    string Name,
    string? Description,
    bool IsDefault);

public sealed record AddPolicyRuleRequest(
    int RuleType,
    int ViolationAction,
    string? ValueString,
    decimal? ValueDecimal,
    int? ValueInt,
    string? Description);

public sealed record AssignPolicyRequest(
    int TargetType,
    string TargetId);

public sealed record TravelPolicyResponse(
    string Id,
    string OrgId,
    string Name,
    string? Description,
    bool IsDefault,
    bool IsActive,
    DateTimeOffset CreatedAt);
