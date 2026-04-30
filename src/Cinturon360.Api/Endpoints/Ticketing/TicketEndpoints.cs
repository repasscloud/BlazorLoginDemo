using MediatR;
using Microsoft.AspNetCore.Mvc;
using Cinturon360.Application.Features.Ticketing.Commands;
using Cinturon360.Application.Features.Ticketing.Queries;
using Cinturon360.Contracts.Common.Errors;
using Cinturon360.Contracts.Common.Results;
using Cinturon360.Contracts.Ticketing;
using Cinturon360.Domain.Entities.Ticketing;
using Cinturon360.Domain.Enums.Ticketing;

namespace Cinturon360.Api.Endpoints.Ticketing;

public static class TicketEndpoints
{
    public static IEndpointRouteBuilder MapTicketEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/tickets").WithTags("Tickets").RequireAuthorization();

        // Any authenticated user
        group.MapGet("/my", ListMine)
            .WithName("ListMyTickets")
            .Produces<ApiResponse<TicketListResponse>>(200);

        group.MapGet("/{ticketId}", GetDetail)
            .WithName("GetTicketDetail")
            .Produces<ApiResponse<TicketDetailResponse>>(200)
            .Produces<ApiResponse<object>>(404);

        group.MapPost("/", Raise)
            .WithName("RaiseTicket")
            .Produces<ApiResponse<string>>(201)
            .Produces<ApiResponse<object>>(400);

        group.MapPost("/{ticketId}/comments", AddComment)
            .WithName("AddTicketComment")
            .Produces<ApiResponse<string>>(201)
            .Produces<ApiResponse<object>>(400);

        group.MapPost("/{ticketId}/close", Close)
            .WithName("CloseTicket")
            .Produces<ApiResponse<object>>(204)
            .Produces<ApiResponse<object>>(400);

        group.MapPut("/{ticketId}/email-preference", UpdateEmailPreference)
            .WithName("UpdateTicketEmailPreference")
            .Produces<ApiResponse<object>>(204)
            .Produces<ApiResponse<object>>(400);

        // Support staff only
        group.MapGet("/org/{orgId}", ListForOrg)
            .WithName("ListOrgTickets")
            .Produces<ApiResponse<TicketListResponse>>(200);

        group.MapPost("/{ticketId}/escalate", Escalate)
            .WithName("EscalateTicket")
            .Produces<ApiResponse<object>>(204)
            .Produces<ApiResponse<object>>(400);

        group.MapPost("/{ticketId}/deescalate", Deescalate)
            .WithName("DeescalateTicket")
            .Produces<ApiResponse<object>>(204)
            .Produces<ApiResponse<object>>(400);

        // Attachments
        group.MapPost("/{ticketId}/attachments", UploadAttachment)
            .WithName("UploadAttachment")
            .DisableAntiforgery()
            .Produces<ApiResponse<string>>(201)
            .Produces<ApiResponse<object>>(400);

        group.MapGet("/{ticketId}/attachments/{attachmentId}", DownloadAttachment)
            .WithName("DownloadAttachment")
            .Produces(200)
            .Produces<ApiResponse<object>>(403)
            .Produces<ApiResponse<object>>(404);

        return app;
    }

    // ── List my tickets ───────────────────────────────────────────────────
    private static async Task<IResult> ListMine(
        [FromQuery] string userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        ISender mediator = default!)
    {
        var result = await mediator.Send(new ListMyTicketsQuery(userId, page, pageSize));
        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    // ── Get detail ────────────────────────────────────────────────────────
    private static async Task<IResult> GetDetail(
        string ticketId,
        [FromQuery] bool isSupport,
        ISender mediator)
    {
        var result = await mediator.Send(new GetTicketDetailQuery(ticketId, isSupport));
        if (result.IsFailure)
            return Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));

        return result.Value is null
            ? Results.NotFound(ApiResponse.Fail(new ApiError("ticket.not_found", "Ticket not found.")))
            : Results.Ok(ApiResponse.Ok(result.Value));
    }

    // ── Raise ─────────────────────────────────────────────────────────────
    private static async Task<IResult> Raise([FromBody] RaiseTicketApiRequest request, ISender mediator)
    {
        var result = await mediator.Send(new RaiseTicketCommand(
            request.OrgId,
            request.RaisedByUserId,
            request.RaisedByFirstName,
            request.OrgSupportTeamName,
            request.Category,
            request.Priority,
            request.Subject,
            request.Description,
            request.EmailMeUpdates,
            request.ErrorContext));

        return result.IsSuccess
            ? Results.Created($"/api/v1/tickets/{result.Value}", ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    // ── Add comment / reply ────────────────────────────────────────────────
    private static async Task<IResult> AddComment(
        string ticketId,
        [FromBody] AddCommentApiRequest request,
        ISender mediator)
    {
        var result = await mediator.Send(new AddTicketCommentCommand(
            ticketId,
            request.AuthorUserId,
            request.AuthorDisplayName,
            request.CallerIsSupport,
            request.Body,
            request.IsPrivate));

        return result.IsSuccess
            ? Results.Created($"/api/v1/tickets/{ticketId}/comments/{result.Value}", ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    // ── Close ─────────────────────────────────────────────────────────────
    private static async Task<IResult> Close(
        string ticketId,
        [FromBody] CloseTicketApiRequest request,
        ISender mediator)
    {
        var result = await mediator.Send(new CloseTicketCommand(
            ticketId,
            request.ActorUserId,
            request.ActorDisplayName,
            request.ResolutionNote));

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> UpdateEmailPreference(
        string ticketId,
        [FromBody] TicketEmailPreferenceApiRequest request,
        ISender mediator)
    {
        var result = await mediator.Send(new UpdateTicketEmailPreferenceCommand(
            ticketId,
            request.ActorUserId,
            request.CallerIsSupport,
            request.EmailMeUpdates));

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    // ── List for org (support dashboard) ──────────────────────────────────
    private static async Task<IResult> ListForOrg(
        string orgId,
        [FromQuery] TicketStatus? status,
        [FromQuery] TicketQueue?  queue,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        ISender mediator = default!)
    {
        var result = await mediator.Send(new ListOrgTicketsQuery(orgId, status, queue, page, pageSize));
        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    // ── Escalate ──────────────────────────────────────────────────────────
    private static async Task<IResult> Escalate(
        string ticketId,
        [FromBody] EscalateTicketApiRequest request,
        ISender mediator)
    {
        var result = await mediator.Send(new EscalateTicketCommand(
            ticketId,
            request.ActorUserId,
            request.ActorDisplayName,
            request.ToQueue,
            request.NewPriority,
            request.Reason));

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    // ── De-escalate ───────────────────────────────────────────────────────
    private static async Task<IResult> Deescalate(
        string ticketId,
        [FromBody] DeescalateTicketApiRequest request,
        ISender mediator)
    {
        var result = await mediator.Send(new DeescalateTicketCommand(
            ticketId,
            request.ActorUserId,
            request.ActorDisplayName,
            request.ToQueue,
            request.Reason));

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    // ── Upload attachment ─────────────────────────────────────────────────
    private static async Task<IResult> UploadAttachment(
        string ticketId,
        IFormFile file,
        [FromQuery] string uploaderUserId,
        [FromQuery] string uploaderDisplayName,
        [FromQuery] bool   uploaderIsSupport,
        [FromQuery] bool   isPrivate,
        ISender mediator)
    {
        using var ms = new global::System.IO.MemoryStream();
        await file.CopyToAsync(ms);
        var content = ms.ToArray();

        var result = await mediator.Send(new UploadAttachmentCommand(
            ticketId,
            uploaderUserId,
            uploaderDisplayName,
            uploaderIsSupport,
            file.FileName,
            file.ContentType,
            content,
            isPrivate));

        return result.IsSuccess
            ? Results.Created($"/api/v1/tickets/{ticketId}/attachments/{result.Value}", ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    // ── Download attachment ───────────────────────────────────────────────
    private static async Task<IResult> DownloadAttachment(
        string ticketId,
        string attachmentId,
        [FromQuery] string callerUserId,
        [FromQuery] bool   callerIsSupport,
        ISender mediator)
    {
        var result = await mediator.Send(new DownloadAttachmentQuery(attachmentId));
        if (!result.IsSuccess || result.Value is null)
            return Results.NotFound(ApiResponse.Fail(new ApiError("attachment.not_found", "Attachment not found.")));

        var a = result.Value;

        // Access control: must belong to the requested ticket
        if (a.TicketId != ticketId)
            return Results.NotFound(ApiResponse.Fail(new ApiError("attachment.not_found", "Attachment not found.")));

        // Private attachments: support only
        if (a.IsPrivate && !callerIsSupport)
            return Results.Forbid();

        return Results.File(a.Content, a.ContentType, a.FileName);
    }
}

// ── API-layer request models (extend contracts with server-resolved fields) ──

public sealed record RaiseTicketApiRequest(
    string         OrgId,
    string         RaisedByUserId,
    string         RaisedByFirstName,
    string?        OrgSupportTeamName,
    TicketCategory Category,
    TicketPriority Priority,
    string         Subject,
    string         Description,
    bool           EmailMeUpdates,
    string?        ErrorContext);

public sealed record TicketEmailPreferenceApiRequest(
    string ActorUserId,
    bool CallerIsSupport,
    bool EmailMeUpdates);

public sealed record AddCommentApiRequest(
    string AuthorUserId,
    string AuthorDisplayName,
    bool   CallerIsSupport,
    string Body,
    bool   IsPrivate);

public sealed record CloseTicketApiRequest(
    string  ActorUserId,
    string  ActorDisplayName,
    string? ResolutionNote);

public sealed record EscalateTicketApiRequest(
    string         ActorUserId,
    string         ActorDisplayName,
    TicketQueue    ToQueue,
    TicketPriority? NewPriority,
    string?        Reason);

public sealed record DeescalateTicketApiRequest(
    string      ActorUserId,
    string      ActorDisplayName,
    TicketQueue ToQueue,
    string?     Reason);
