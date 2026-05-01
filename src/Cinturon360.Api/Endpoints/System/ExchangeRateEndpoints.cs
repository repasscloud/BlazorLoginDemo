using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Application.Features.DataFeeds.Commands;
using Cinturon360.Application.Features.DataFeeds.Queries;
using Cinturon360.Contracts.Common.Errors;
using Cinturon360.Contracts.Common.Results;
using Cinturon360.Contracts.Feeds;
using Cinturon360.Common.Constants;
using AppClaimTypes = Cinturon360.Common.Constants.ClaimTypes;

namespace Cinturon360.Api.Endpoints.System;

public static class ExchangeRateEndpoints
{
    public static IEndpointRouteBuilder MapExchangeRateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/system/exchange-rates")
            .WithTags("System")
            .RequireAuthorization();

        group.MapGet("/latest", ListLatest)
            .WithName("ListLatestExchangeRates")
            .Produces<ApiResponse<IReadOnlyList<ExchangeRateLatestResponse>>>(200);

        group.MapGet("/convert", Convert)
            .WithName("ConvertCurrency")
            .Produces<ApiResponse<ConvertCurrencyResponse>>(200)
            .Produces<ApiResponse<object>>(400);

        group.MapPost("/refresh", TriggerRefresh)
            .WithName("RefreshExchangeRates")
            .Produces(202)
            .Produces<ApiResponse<object>>(403)
            .Produces<ApiResponse<object>>(400);

        group.MapPost("/cleanup", TriggerCleanup)
            .WithName("CleanupExchangeRates")
            .Produces<ApiResponse<int>>(200)
            .Produces<ApiResponse<object>>(403)
            .Produces<ApiResponse<object>>(400);

        return app;
    }

    private static async Task<IResult> ListLatest(IExchangeRateRepository repository)
    {
        var rates = await repository.ListLatestAsync();
        var response = rates
            .Select(r => new ExchangeRateLatestResponse(
                r.CurrencyCode,
                r.CurrencyName,
                r.Rate,
                r.RateDate,
                r.FetchedAt))
            .ToList();

        return Results.Ok(ApiResponse.Ok<IReadOnlyList<ExchangeRateLatestResponse>>(response));
    }

    private static async Task<IResult> Convert(
        [FromQuery] decimal amount,
        [FromQuery] string fromCurrencyCode,
        [FromQuery] string toCurrencyCode,
        ISender mediator)
    {
        var result = await mediator.Send(new ConvertCurrencyQuery(amount, fromCurrencyCode, toCurrencyCode));

        if (result.IsFailure)
            return Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));

        return Results.Ok(ApiResponse.Ok(new ConvertCurrencyResponse(
            amount,
            fromCurrencyCode.ToUpperInvariant(),
            toCurrencyCode.ToUpperInvariant(),
            result.Value)));
    }

    private static async Task<IResult> TriggerRefresh(HttpContext httpContext, ISender mediator)
    {
        if (!IsGlobalAdmin(httpContext.User))
            return Results.StatusCode(StatusCodes.Status403Forbidden);

        var result = await mediator.Send(new RefreshExchangeRatesCommand());

        return result.IsSuccess
            ? Results.Accepted()
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> TriggerCleanup(
        HttpContext httpContext,
        [FromQuery] int retentionHours,
        ISender mediator)
    {
        if (!IsGlobalAdmin(httpContext.User))
            return Results.StatusCode(StatusCodes.Status403Forbidden);

        var safeRetention = retentionHours <= 0 ? 24 : retentionHours;

        var result = await mediator.Send(new CleanupExchangeRateSnapshotsCommand(TimeSpan.FromHours(safeRetention)));

        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static bool IsGlobalAdmin(ClaimsPrincipal user)
    {
        var appRole = user.FindFirstValue(AppClaimTypes.AppRole)
                      ?? user.FindFirstValue("c360:role")
                      ?? user.FindFirstValue(global::System.Security.Claims.ClaimTypes.Role);

        return string.Equals(appRole, Roles.GlobalAdmin, StringComparison.OrdinalIgnoreCase);
    }
}
