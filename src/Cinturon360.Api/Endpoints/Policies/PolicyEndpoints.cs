using MediatR;
using Microsoft.AspNetCore.Mvc;
using Cinturon360.Application.Features.Policies.Commands;
using Cinturon360.Application.Features.Policies.Queries;
using Cinturon360.Contracts.Common.Errors;
using Cinturon360.Contracts.Common.Results;
using Cinturon360.Contracts.Policies;
using Cinturon360.Domain.Entities.Policy;
using Cinturon360.Domain.Enums.Policy;

namespace Cinturon360.Api.Endpoints.Policies;

public static class PolicyEndpoints
{
    public static IEndpointRouteBuilder MapPolicyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/organisations/{orgId}/policies").WithTags("Policies").RequireAuthorization();

        group.MapGet("/", ListForOrg)
            .WithName("ListOrgPolicies")
            .Produces<ApiResponse<IEnumerable<TravelPolicyResponse>>>(200);

        group.MapGet("/{policyId}", GetById)
            .WithName("GetTravelPolicy")
            .Produces<ApiResponse<TravelPolicyResponse>>(200)
            .Produces<ApiResponse<object>>(404);

        group.MapPost("/", Create)
            .WithName("CreateTravelPolicy")
            .Produces<ApiResponse<string>>(201)
            .Produces<ApiResponse<object>>(400);

        group.MapPost("/{policyId}/rules", AddRule)
            .WithName("AddPolicyRule")
            .Produces<ApiResponse<string>>(201)
            .Produces<ApiResponse<object>>(400);

        group.MapPost("/{policyId}/assignments", Assign)
            .WithName("AssignPolicy")
            .Produces<ApiResponse<string>>(201)
            .Produces<ApiResponse<object>>(400);

        return app;
    }

    private static async Task<IResult> ListForOrg(string orgId, ISender mediator)
    {
        var result = await mediator.Send(new ListOrgPoliciesQuery(orgId));
        if (result.IsFailure)
            return Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));

        return Results.Ok(ApiResponse.Ok(result.Value.Select(MapPolicy)));
    }

    private static async Task<IResult> GetById(string policyId, ISender mediator)
    {
        var result = await mediator.Send(new GetTravelPolicyQuery(policyId));
        if (result.IsFailure)
            return Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));

        return result.Value is null
            ? Results.NotFound(ApiResponse.Fail(new ApiError("policy.not_found", "Travel policy not found.")))
            : Results.Ok(ApiResponse.Ok(MapPolicy(result.Value)));
    }

    private static async Task<IResult> Create(string orgId, [FromBody] CreateTravelPolicyRequest request, ISender mediator)
    {
        var result = await mediator.Send(new CreateTravelPolicyCommand(orgId, request.Name, request.Description, request.IsDefault));

        return result.IsSuccess
            ? Results.Created($"/api/v1/organisations/{orgId}/policies/{result.Value}", ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> AddRule(string policyId, [FromBody] AddPolicyRuleRequest request, ISender mediator)
    {
        if (!Enum.IsDefined(typeof(PolicyRuleType), request.RuleType))
            return Results.BadRequest(ApiResponse.Fail(new ApiError("policy.invalid_rule_type", "Invalid policy rule type.")));

        if (!Enum.IsDefined(typeof(PolicyViolationAction), request.ViolationAction))
            return Results.BadRequest(ApiResponse.Fail(new ApiError("policy.invalid_violation_action", "Invalid policy violation action.")));

        var result = await mediator.Send(new AddPolicyRuleCommand(
            policyId,
            (PolicyRuleType)request.RuleType,
            (PolicyViolationAction)request.ViolationAction,
            request.ValueString,
            request.ValueDecimal,
            request.ValueInt,
            request.Description));

        return result.IsSuccess
            ? Results.Created($"/api/v1/policies/{policyId}/rules/{result.Value}", ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> Assign(string policyId, [FromBody] AssignPolicyRequest request, ISender mediator)
    {
        if (!Enum.IsDefined(typeof(PolicyAssignmentTarget), request.TargetType))
            return Results.BadRequest(ApiResponse.Fail(new ApiError("policy.invalid_assignment_target", "Invalid policy assignment target.")));

        var result = await mediator.Send(new AssignPolicyCommand(policyId, (PolicyAssignmentTarget)request.TargetType, request.TargetId));

        return result.IsSuccess
            ? Results.Created($"/api/v1/policies/{policyId}/assignments/{result.Value}", ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static TravelPolicyResponse MapPolicy(TravelPolicy p) => new(
        p.Id,
        p.OrgId,
        p.Name,
        p.Description,
        p.IsDefault,
        p.IsActive,
        p.CreatedAt);
}
