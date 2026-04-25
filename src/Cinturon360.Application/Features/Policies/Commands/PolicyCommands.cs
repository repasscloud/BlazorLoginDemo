using MediatR;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Common.IdGeneration;
using Cinturon360.Common.Results;
using Cinturon360.Domain.Entities.Policy;
using Cinturon360.Domain.Enums.Policy;

namespace Cinturon360.Application.Features.Policies.Commands;

// ── Errors ─────────────────────────────────────────────────────────────────
public static class PolicyErrors
{
    public static readonly Error NotFound     = new("policy.not_found",     "Travel policy not found.");
    public static readonly Error RuleNotFound = new("policy.rule_not_found","Policy rule not found.");
}

// ── Create policy ─────────────────────────────────────────────────────────
public sealed record CreateTravelPolicyCommand(
    string OrgId,
    string Name,
    string? Description,
    bool IsDefault) : IRequest<Result<string>>;

public sealed class CreateTravelPolicyHandler : IRequestHandler<CreateTravelPolicyCommand, Result<string>>
{
    private readonly ITravelPolicyRepository _repo;
    private readonly IUnitOfWork _uow;

    public CreateTravelPolicyHandler(ITravelPolicyRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result<string>> Handle(CreateTravelPolicyCommand request, CancellationToken ct)
    {
        var id = IdGenerator.New(IdPrefix.Policy);
        var policy = TravelPolicy.Create(id, request.OrgId, request.Name, request.Description, request.IsDefault);
        await _repo.AddAsync(policy, ct);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(id);
    }
}

// ── Add policy rule ───────────────────────────────────────────────────────
public sealed record AddPolicyRuleCommand(
    string PolicyId,
    PolicyRuleType RuleType,
    PolicyViolationAction ViolationAction,
    string? ValueString,
    decimal? ValueDecimal,
    int? ValueInt,
    string? Description) : IRequest<Result<string>>;

public sealed class AddPolicyRuleHandler : IRequestHandler<AddPolicyRuleCommand, Result<string>>
{
    private readonly ITravelPolicyRepository _repo;
    private readonly IUnitOfWork _uow;

    public AddPolicyRuleHandler(ITravelPolicyRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result<string>> Handle(AddPolicyRuleCommand request, CancellationToken ct)
    {
        var policy = await _repo.GetByIdAsync(request.PolicyId, ct);
        if (policy is null) return Result.Failure<string>(PolicyErrors.NotFound);

        var ruleId = IdGenerator.New(IdPrefix.PolicyRule);
        var rule = PolicyRule.Create(ruleId, request.PolicyId, request.RuleType,
            request.ViolationAction, request.ValueString, request.ValueDecimal,
            request.ValueInt, request.Description);

        await _repo.AddRuleAsync(rule, ct);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(ruleId);
    }
}

// ── Assign policy ──────────────────────────────────────────────────────────
public sealed record AssignPolicyCommand(
    string PolicyId,
    PolicyAssignmentTarget TargetType,
    string TargetId) : IRequest<Result<string>>;

public sealed class AssignPolicyHandler : IRequestHandler<AssignPolicyCommand, Result<string>>
{
    private readonly ITravelPolicyRepository _repo;
    private readonly IUnitOfWork _uow;

    public AssignPolicyHandler(ITravelPolicyRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result<string>> Handle(AssignPolicyCommand request, CancellationToken ct)
    {
        var policy = await _repo.GetByIdAsync(request.PolicyId, ct);
        if (policy is null) return Result.Failure<string>(PolicyErrors.NotFound);

        var assignmentId = IdGenerator.New(IdPrefix.PolicyAssignment);
        var assignment = PolicyAssignment.Create(assignmentId, request.PolicyId, request.TargetType, request.TargetId);

        await _repo.AddAssignmentAsync(assignment, ct);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(assignmentId);
    }
}
