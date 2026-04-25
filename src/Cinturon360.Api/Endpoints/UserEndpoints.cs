using MediatR;
using Microsoft.AspNetCore.Mvc;
using Cinturon360.Application.Abstractions.Security;
using Cinturon360.Application.Features.Users.Commands;
using Cinturon360.Application.Features.Users.Queries;
using Cinturon360.Contracts.Common.Errors;
using Cinturon360.Contracts.Common.Results;
using Cinturon360.Contracts.Users;
using Cinturon360.Domain.Enums.Security;

namespace Cinturon360.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/users").WithTags("Users").RequireAuthorization();

        group.MapGet("/{userId}", GetUser)
            .WithName("GetUser")
            .Produces<ApiResponse<UserDetail>>(200)
            .Produces<ApiResponse<object>>(404);

        group.MapPost("/", CreateUser)
            .WithName("CreateUser")
            .Produces<ApiResponse<string>>(201)
            .Produces<ApiResponse<object>>(400);

        return app;
    }

    private static async Task<IResult> GetUser(string userId, ISender mediator)
    {
        var result = await mediator.Send(new GetUserByIdQuery(userId));

        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(result.Value))
            : Results.NotFound(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> CreateUser(
        [FromBody] CreateUserRequest request,
        ISender mediator,
        ICurrentUser currentUser)
    {
        if (!Enum.TryParse<UserCategory>(request.UserCategory, ignoreCase: true, out var category))
            return Results.BadRequest(ApiResponse.Fail(new ApiError("invalid_user_category", $"Unknown user category: {request.UserCategory}")));

        var result = await mediator.Send(new CreateUserCommand(
            Email: request.Email,
            FirstName: request.FirstName,
            LastName: request.LastName,
            UserCategory: category,
            HomeOrgId: request.HomeOrgId,
            PlaintextPassword: request.Password,
            CreatedByUserId: currentUser.UserId));

        return result.IsSuccess
            ? Results.Created($"/api/v1/users/{result.Value}", ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }
}
