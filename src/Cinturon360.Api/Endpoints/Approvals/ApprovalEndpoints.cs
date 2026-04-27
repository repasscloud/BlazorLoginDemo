using MediatR;
using Microsoft.AspNetCore.Mvc;
using Cinturon360.Application.Features.Approvals.Commands;
using Cinturon360.Application.Features.Approvals.Queries;
using Cinturon360.Contracts.Approvals;
using Cinturon360.Contracts.Common.Errors;
using Cinturon360.Contracts.Common.Results;
using Cinturon360.Domain.Entities.Approval;
using Cinturon360.Domain.Enums.Approval;

namespace Cinturon360.Api.Endpoints.Approvals;

public static class ApprovalEndpoints
{
    public static IEndpointRouteBuilder MapApprovalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/approvals").WithTags("Approvals").RequireAuthorization();

        group.MapGet("/{approvalRequestId}", GetById)
            .WithName("GetApprovalRequest")
            .Produces<ApiResponse<ApprovalDetailResponse>>(200)
            .Produces<ApiResponse<object>>(404);

        group.MapGet("/pending/{approverUserId}", ListPending)
            .WithName("ListPendingApprovals")
            .Produces<ApiResponse<IEnumerable<ApprovalRequestResponse>>>(200);

        group.MapGet("/history/{userId}", ListHistory)
            .WithName("ListApprovalHistory")
            .Produces<ApiResponse<IEnumerable<ApprovalRequestResponse>>>(200);

        group.MapPost("/", Submit)
            .WithName("SubmitForApproval")
            .Produces<ApiResponse<string>>(201)
            .Produces<ApiResponse<object>>(400);

        group.MapPost("/{approvalRequestId}/approve", Approve)
            .WithName("ApproveRequest")
            .Produces<ApiResponse<object>>(204)
            .Produces<ApiResponse<object>>(400);

        group.MapPost("/{approvalRequestId}/reject", Reject)
            .WithName("RejectRequest")
            .Produces<ApiResponse<object>>(204)
            .Produces<ApiResponse<object>>(400);

        return app;
    }

    private static async Task<IResult> GetById(string approvalRequestId, ISender mediator)
    {
        var result = await mediator.Send(new GetApprovalDetailQuery(approvalRequestId));
        if (result.IsFailure)
            return Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));

        return result.Value is null
            ? Results.NotFound(ApiResponse.Fail(new ApiError("approval.not_found", "Approval request not found.")))
            : Results.Ok(ApiResponse.Ok(MapApprovalDetail(result.Value)));
    }

    private static async Task<IResult> ListPending(string approverUserId, ISender mediator)
    {
        var result = await mediator.Send(new ListPendingApprovalsQuery(approverUserId));
        if (result.IsFailure)
            return Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));

        return Results.Ok(ApiResponse.Ok(result.Value.Select(MapApproval)));
    }

    private static async Task<IResult> ListHistory(string userId, ISender mediator)
    {
        var result = await mediator.Send(new ListApprovalHistoryQuery(userId));
        if (result.IsFailure)
            return Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));

        return Results.Ok(ApiResponse.Ok(result.Value.Select(MapApproval)));
    }

    private static async Task<IResult> Submit([FromBody] SubmitForApprovalRequest request, ISender mediator)
    {
        if (!Enum.IsDefined(typeof(ApprovalSubjectType), request.SubjectType))
            return Results.BadRequest(ApiResponse.Fail(new ApiError("approval.invalid_subject_type", "Invalid approval subject type.")));

        var result = await mediator.Send(new SubmitForApprovalCommand(
            request.OrgId,
            (ApprovalSubjectType)request.SubjectType,
            request.SubjectId,
            request.RequestedByUserId,
            request.TotalLevels,
            request.Notes));

        return result.IsSuccess
            ? Results.Created($"/api/v1/approvals/{result.Value}", ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> Approve(string approvalRequestId, [FromBody] ApprovalDecisionRequest request, ISender mediator)
    {
        var result = await mediator.Send(new ApproveCommand(approvalRequestId, request.ApproverUserId, request.Comments));
        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> Reject(string approvalRequestId, [FromBody] ApprovalDecisionRequest request, ISender mediator)
    {
        var result = await mediator.Send(new RejectCommand(approvalRequestId, request.ApproverUserId, request.Comments));
        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static ApprovalRequestResponse MapApproval(ApprovalRequest a) => new(
        a.Id,
        a.OrgId,
        (int)a.SubjectType,
        a.SubjectId,
        a.RequestedByUserId,
        (int)a.Status,
        a.TotalLevels,
        a.CurrentLevel,
        a.Notes,
        a.CreatedAt,
        a.ResolvedAt);

    private static ApprovalDecisionResponse MapDecision(ApprovalDecision decision) => new(
        decision.Id,
        decision.Level,
        decision.ApproverUserId,
        (int)decision.Decision,
        decision.Comments,
        decision.CreatedAt);

    private static ApprovalDetailResponse MapApprovalDetail(Cinturon360.Application.Features.Approvals.Models.ApprovalDetailModel detail) => new(
        MapApproval(detail.Approval),
        detail.Decisions.Select(MapDecision).ToList());
}
