using MediatR;
using Microsoft.AspNetCore.Mvc;
using Cinturon360.Application.Features.Bookings.Commands;
using Cinturon360.Application.Features.Bookings.Queries;
using Cinturon360.Contracts.Bookings;
using Cinturon360.Contracts.Common.Errors;
using Cinturon360.Contracts.Common.Results;
using Cinturon360.Domain.Entities.Booking;

namespace Cinturon360.Api.Endpoints.Bookings;

public static class BookingEndpoints
{
    public static IEndpointRouteBuilder MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/bookings").WithTags("Bookings").RequireAuthorization();

        group.MapGet("/{bookingId}", GetById)
            .WithName("GetBookingById")
            .Produces<ApiResponse<BookingResponse>>(200)
            .Produces<ApiResponse<object>>(404);

        group.MapGet("/", List)
            .WithName("ListBookings")
            .Produces<ApiResponse<PagedBookingsResponse>>(200);

        group.MapPost("/", Create)
            .WithName("CreateBooking")
            .Produces<ApiResponse<string>>(201)
            .Produces<ApiResponse<object>>(400);

        group.MapPost("/{bookingId}/confirm", Confirm)
            .WithName("ConfirmBooking")
            .Produces<ApiResponse<object>>(204)
            .Produces<ApiResponse<object>>(400);

        group.MapPost("/{bookingId}/cancel", Cancel)
            .WithName("CancelBooking")
            .Produces<ApiResponse<object>>(204)
            .Produces<ApiResponse<object>>(400);

        return app;
    }

    private static async Task<IResult> GetById(string bookingId, ISender mediator)
    {
        var result = await mediator.Send(new GetBookingByIdQuery(bookingId));
        if (result.IsFailure)
            return Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));

        return result.Value is null
            ? Results.NotFound(ApiResponse.Fail(new ApiError("booking.not_found", "Booking not found.")))
            : Results.Ok(ApiResponse.Ok(MapBooking(result.Value)));
    }

    private static async Task<IResult> List(
        [FromQuery] string? orgId,
        [FromQuery] string? travellerUserId,
        [FromQuery] int? status,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        ISender mediator)
    {
        var safePage = page <= 0 ? 1 : page;
        var safeSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

        var result = await mediator.Send(new ListBookingsQuery(orgId, travellerUserId, status, safePage, safeSize));
        if (result.IsFailure)
            return Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));

        var response = new PagedBookingsResponse(
            result.Value.Items.Select(MapBooking).ToList(),
            result.Value.Total,
            safePage,
            safeSize);

        return Results.Ok(ApiResponse.Ok(response));
    }

    private static async Task<IResult> Create([FromBody] CreateBookingRequest request, ISender mediator)
    {
        var result = await mediator.Send(new CreateBookingCommand(
            request.OrgId,
            request.TravellerUserId,
            request.BookedByUserId,
            request.CurrencyCode,
            request.QuoteId));

        return result.IsSuccess
            ? Results.Created($"/api/v1/bookings/{result.Value}", ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> Confirm(string bookingId, [FromBody] ConfirmBookingRequest request, ISender mediator)
    {
        var result = await mediator.Send(new ConfirmBookingCommand(bookingId, request.PnrCode, request.SupplierRef));
        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> Cancel(string bookingId, [FromBody] CancelBookingRequest request, ISender mediator)
    {
        var result = await mediator.Send(new CancelBookingCommand(bookingId, request.Reason));
        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static BookingResponse MapBooking(Booking b) => new(
        b.Id,
        b.OrgId,
        b.TravellerUserId,
        b.BookedByUserId,
        b.QuoteId,
        b.Status.ToString(),
        b.TotalAmountGross,
        b.CurrencyCode,
        b.PnrCode,
        b.TripStartDate,
        b.TripEndDate,
        b.CreatedAt);
}
