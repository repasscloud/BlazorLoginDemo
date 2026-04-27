using MediatR;
using Microsoft.AspNetCore.Mvc;
using Cinturon360.Application.Abstractions.Security;
using Cinturon360.Application.Features.Auth.Commands;
using Cinturon360.Application.Features.Auth.Queries;
using Cinturon360.Contracts.Auth;
using Cinturon360.Contracts.Common.Errors;
using Cinturon360.Contracts.Common.Results;

namespace Cinturon360.Api.Endpoints.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth").WithTags("Auth");

        group.MapPost("/login", Login)
            .AllowAnonymous()
            .WithName("Login")
            .Produces<ApiResponse<AuthResponse>>(200)
            .Produces<ApiResponse<object>>(400)
            .Produces<ApiResponse<object>>(401);

        group.MapPost("/logout", Logout)
            .RequireAuthorization()
            .WithName("Logout")
            .Produces(204)
            .Produces<ApiResponse<object>>(401);

        group.MapGet("/pat", ListPats)
            .RequireAuthorization()
            .WithName("ListPats")
            .Produces<ApiResponse<IReadOnlyList<PatSummary>>>(200);

        group.MapPost("/pat", CreatePat)
            .RequireAuthorization()
            .WithName("CreatePat")
            .Produces<ApiResponse<CreatePatResponse>>(201)
            .Produces<ApiResponse<object>>(400);

        group.MapDelete("/pat/{tokenId}", RevokePat)
            .RequireAuthorization()
            .WithName("RevokePat")
            .Produces(204)
            .Produces<ApiResponse<object>>(404);

        group.MapPost("/register", Register)
            .AllowAnonymous()
            .WithName("Register")
            .Produces<ApiResponse<AuthResponse>>(201)
            .Produces<ApiResponse<object>>(400)
            .Produces<ApiResponse<object>>(409);

        group.MapPost("/password/forgot", ForgotPassword)
            .AllowAnonymous()
            .WithName("ForgotPassword")
            .Produces(204)
            .Produces<ApiResponse<object>>(400);

        group.MapPost("/password/reset", ResetPassword)
            .AllowAnonymous()
            .WithName("ResetPassword")
            .Produces(204)
            .Produces<ApiResponse<object>>(400);

        return app;
    }

    private static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        ISender mediator,
        HttpContext ctx)
    {
        var command = new LoginCommand(
            Email: request.Email,
            Password: request.Password,
            MfaCode: request.MfaCode,
            IpAddress: ctx.Connection.RemoteIpAddress?.ToString(),
            UserAgent: ctx.Request.Headers.UserAgent.ToString(),
            DeviceId: request.DeviceId);

        var result = await mediator.Send(command);

        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(result.Value))
            : Results.Unauthorized();
    }

    private static async Task<IResult> Logout(
        ISender mediator,
        ICurrentUser currentUser,
        [FromQuery] string sessionId)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Results.Unauthorized();

        var result = await mediator.Send(new LogoutCommand(sessionId, currentUser.UserId));

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> ListPats(ISender mediator, ICurrentUser currentUser)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Results.Unauthorized();

        var result = await mediator.Send(new ListPatsQuery(currentUser.UserId));
        return Results.Ok(ApiResponse.Ok(result.Value));
    }

    private static async Task<IResult> CreatePat(
        [FromBody] CreatePatRequest request,
        ISender mediator,
        ICurrentUser currentUser)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Results.Unauthorized();

        var result = await mediator.Send(new CreatePatCommand(
            UserId: currentUser.UserId,
            Name: request.Name,
            ExpiresAt: request.ExpiresAt,
            Scopes: request.Scopes));

        return result.IsSuccess
            ? Results.Created($"/api/v1/auth/pat/{result.Value.TokenId}", ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> RevokePat(
        string tokenId,
        ISender mediator,
        ICurrentUser currentUser)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Results.Unauthorized();

        var result = await mediator.Send(new RevokePatCommand(tokenId, currentUser.UserId));

        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterRequest request,
        ISender mediator,
        HttpContext ctx)
    {
        var command = new RegisterUserCommand(
            FirstName: request.FirstName,
            LastName: request.LastName,
            Email: request.Email,
            Password: request.Password,
            IpAddress: ctx.Connection.RemoteIpAddress?.ToString(),
            UserAgent: ctx.Request.Headers.UserAgent.ToString());

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            var statusCode = result.Error.Code == "auth.email_already_registered" ? 409 : 400;
            return Results.Json(
                ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)),
                statusCode: statusCode);
        }

        return Results.Created("/api/v1/auth/me", ApiResponse.Ok(result.Value));
    }

    private static async Task<IResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        ISender mediator)
    {
        // Always 204 — prevents email enumeration
        await mediator.Send(new RequestPasswordResetCommand(request.Email));
        return Results.NoContent();
    }

    private static async Task<IResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        ISender mediator)
    {
        var result = await mediator.Send(new ResetPasswordCommand(
            Email: request.Email,
            Token: request.Token,
            NewPassword: request.NewPassword));

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }
}
