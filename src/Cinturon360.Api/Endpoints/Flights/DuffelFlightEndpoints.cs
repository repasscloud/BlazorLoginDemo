using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Cinturon360.Application.Features.Integrations.Duffel.Commands;
using Cinturon360.Common.Constants;
using Cinturon360.Contracts.Common.Errors;
using Cinturon360.Contracts.Common.Results;
using Cinturon360.Contracts.Flights;
using AppClaimTypes = Cinturon360.Common.Constants.ClaimTypes;

namespace Cinturon360.Api.Endpoints.Flights;

public static class DuffelFlightEndpoints
{
    public static IEndpointRouteBuilder MapDuffelFlightEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/flights/duffel")
            .WithTags("Flights")
            .RequireAuthorization();

        group.MapPost("/search", SearchOffers)
            .WithName("DuffelSearchOffers")
            .Produces<ApiResponse<DuffelOfferSearchResponse>>(200)
            .Produces<ApiResponse<object>>(400)
            .Produces<ApiResponse<object>>(403);

        group.MapPost("/orders", CreateOrder)
            .WithName("DuffelCreateOrder")
            .Produces<ApiResponse<DuffelCreateOrderResponse>>(201)
            .Produces<ApiResponse<object>>(400)
            .Produces<ApiResponse<object>>(403);

        group.MapPost("/orders/{duffelOrderId}/cancel", CancelOrder)
            .WithName("DuffelCancelOrder")
            .Produces<ApiResponse<DuffelCancelOrderResponse>>(200)
            .Produces<ApiResponse<object>>(400)
            .Produces<ApiResponse<object>>(403);

        return app;
    }

    // ── Search Offers ──────────────────────────────────────────────────────

    private static async Task<IResult> SearchOffers(
        HttpContext httpContext,
        [FromBody] DuffelOfferSearchRequest request,
        ISender mediator)
    {
        if (!IsAuthorizedForOrg(httpContext.User, request.OrgId))
            return Results.StatusCode(StatusCodes.Status403Forbidden);

        if (request.Passengers is null || request.Passengers.Count == 0)
            return Results.BadRequest(ApiResponse.Fail(
                new ApiError("duffel.invalid_request", "At least one passenger is required.")));

        if (request.Slices is null || request.Slices.Count == 0)
            return Results.BadRequest(ApiResponse.Fail(
                new ApiError("duffel.invalid_request", "At least one slice is required.")));

        var result = await mediator.Send(new DuffelSearchOffersCommand(
            OrgId:          request.OrgId,
            CabinClass:     request.CabinClass,
            MaxConnections: request.MaxConnections,
            Passengers:     request.Passengers,
            Slices:         request.Slices));

        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    // ── Create Order ───────────────────────────────────────────────────────

    private static async Task<IResult> CreateOrder(
        HttpContext httpContext,
        [FromBody] DuffelCreateOrderRequest request,
        ISender mediator)
    {
        if (!IsAuthorizedForOrg(httpContext.User, request.OrgId, requireBooker: true))
            return Results.StatusCode(StatusCodes.Status403Forbidden);

        if (string.IsNullOrWhiteSpace(request.SelectedOfferId))
            return Results.BadRequest(ApiResponse.Fail(
                new ApiError("duffel.invalid_request", "SelectedOfferId is required.")));

        if (request.Passengers is null || request.Passengers.Count == 0)
            return Results.BadRequest(ApiResponse.Fail(
                new ApiError("duffel.invalid_request", "At least one passenger is required.")));

        if (request.Payment is null)
            return Results.BadRequest(ApiResponse.Fail(
                new ApiError("duffel.invalid_request", "Payment details are required.")));

        var result = await mediator.Send(new DuffelCreateOrderCommand(
            OrgId:              request.OrgId,
            SelectedOfferId:    request.SelectedOfferId,
            TravellerUserId:    request.TravellerUserId,
            ExistingBookingId:  request.ExistingBookingId,
            Passengers:         request.Passengers,
            Payment:            request.Payment));

        return result.IsSuccess
            ? Results.Created($"/api/v1/bookings/{result.Value.BookingId}", ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    // ── Cancel Order ───────────────────────────────────────────────────────

    private static async Task<IResult> CancelOrder(
        HttpContext httpContext,
        string duffelOrderId,
        [FromBody] DuffelCancelOrderRequest request,
        ISender mediator)
    {
        if (!IsAuthorizedForOrg(httpContext.User, request.OrgId, requireBooker: true))
            return Results.StatusCode(StatusCodes.Status403Forbidden);

        if (string.IsNullOrWhiteSpace(request.DuffelOrderId)
            || !string.Equals(request.DuffelOrderId, duffelOrderId, StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest(ApiResponse.Fail(
                new ApiError("duffel.invalid_request",
                    "The DuffelOrderId in the request body must match the route parameter.")));
        }

        var result = await mediator.Send(new DuffelCancelOrderCommand(
            OrgId:         request.OrgId,
            DuffelOrderId: request.DuffelOrderId,
            BookingId:     request.BookingId));

        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    // ── Auth helper ────────────────────────────────────────────────────────

    private static bool IsAuthorizedForOrg(
        ClaimsPrincipal user,
        string orgId,
        bool requireBooker = false)
    {
        var callerOrgId = user.FindFirstValue(AppClaimTypes.OrgId);
        var appRole     = user.FindFirstValue(AppClaimTypes.AppRole);
        var orgRole     = user.FindFirstValue(AppClaimTypes.OrgRole);

        // Platform-level admins can act on any org
        if (string.Equals(appRole, Roles.GlobalAdmin,    StringComparison.OrdinalIgnoreCase)
         || string.Equals(appRole, Roles.Support,         StringComparison.OrdinalIgnoreCase)
         || string.Equals(appRole, Roles.Integrations,    StringComparison.OrdinalIgnoreCase))
            return true;

        // Caller must belong to the target org
        if (!string.Equals(callerOrgId, orgId, StringComparison.OrdinalIgnoreCase))
            return false;

        // For transactional operations, require at least booker-level access
        if (requireBooker)
        {
            return string.Equals(orgRole, Roles.OrgAdmin,   StringComparison.OrdinalIgnoreCase)
                || string.Equals(orgRole, Roles.Booker,     StringComparison.OrdinalIgnoreCase)
                || string.Equals(orgRole, Roles.Approver,   StringComparison.OrdinalIgnoreCase);
        }

        return true;
    }
}
