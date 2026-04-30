using System.Security.Claims;
using Cinturon360.Application.Features.Ticketing.Commands;
using Cinturon360.Application.Features.Ticketing.Queries;
using Cinturon360.Common.Constants;
using Cinturon360.Contracts.Common.Errors;
using Cinturon360.Contracts.Common.Results;
using Cinturon360.Contracts.Ticketing;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cinturon360.Api.Endpoints.System;

public static class TicketEmailTemplateEndpoints
{
    public static IEndpointRouteBuilder MapTicketEmailTemplateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/system/ticket-email-templates")
            .WithTags("Ticket Email Templates")
            .RequireAuthorization();

        group.MapGet("/", ListTemplates)
            .WithName("ListTicketEmailTemplates")
            .Produces<ApiResponse<IReadOnlyList<TicketEmailTemplateResponse>>>(200);

        group.MapGet("/{templateId}", GetTemplate)
            .WithName("GetTicketEmailTemplate")
            .Produces<ApiResponse<TicketEmailTemplateDetailResponse>>(200)
            .Produces<ApiResponse<object>>(403)
            .Produces<ApiResponse<object>>(404);

        group.MapPut("/", UpsertTemplate)
            .WithName("UpsertTicketEmailTemplate")
            .Produces<ApiResponse<string>>(200)
            .Produces<ApiResponse<object>>(403)
            .Produces<ApiResponse<object>>(400);

        group.MapDelete("/{templateId}", DeleteTemplate)
            .WithName("DeleteTicketEmailTemplate")
            .Produces(204)
            .Produces<ApiResponse<object>>(403)
            .Produces<ApiResponse<object>>(404);

        return app;
    }

    private static async Task<IResult> ListTemplates(
        HttpContext httpContext,
        ISender mediator,
        [FromQuery] string? code = null)
    {
        if (!IsGlobalAdmin(httpContext.User))
            return Results.Forbid();

        var result = await mediator.Send(new ListTicketEmailTemplatesQuery(code));
        return Results.Ok(ApiResponse.Ok(result.Value));
    }

    private static async Task<IResult> GetTemplate(
        string templateId,
        HttpContext httpContext,
        ISender mediator)
    {
        if (!IsGlobalAdmin(httpContext.User))
            return Results.Forbid();

        var result = await mediator.Send(new GetTicketEmailTemplateQuery(templateId));
        return result.Value is null
            ? Results.NotFound(ApiResponse.Fail(new ApiError(TicketErrors.NotFound.Code, TicketErrors.NotFound.Description)))
            : Results.Ok(ApiResponse.Ok(result.Value));
    }

    private static async Task<IResult> UpsertTemplate(
        HttpContext httpContext,
        [FromBody] UpsertTicketEmailTemplateRequest request,
        ISender mediator)
    {
        if (!IsGlobalAdmin(httpContext.User))
            return Results.Forbid();

        var result = await mediator.Send(new UpsertTicketEmailTemplateCommand(
            request.Code,
            request.LanguageCode,
            request.HtmlBody,
            request.PlainTextBody,
            request.Description,
            request.IsActive));

        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> DeleteTemplate(
        string templateId,
        HttpContext httpContext,
        ISender mediator)
    {
        if (!IsGlobalAdmin(httpContext.User))
            return Results.Forbid();

        var result = await mediator.Send(new DeleteTicketEmailTemplateCommand(templateId));
        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static bool IsGlobalAdmin(ClaimsPrincipal user)
    {
        var values = user.Claims
            .Where(x => x.Type is Cinturon360.Common.Constants.ClaimTypes.AppRole or "c360:role" or global::System.Security.Claims.ClaimTypes.Role)
            .Select(x => x.Value);

        return values.Any(x => string.Equals(x, Roles.GlobalAdmin, StringComparison.OrdinalIgnoreCase));
    }
}