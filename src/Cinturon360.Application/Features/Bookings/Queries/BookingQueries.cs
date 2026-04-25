using MediatR;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Common.Results;
using Cinturon360.Domain.Entities.Booking;

namespace Cinturon360.Application.Features.Bookings.Queries;

// ── Get by ID ─────────────────────────────────────────────────────────────
public sealed record GetBookingByIdQuery(string BookingId) : IRequest<Result<Booking?>>;

public sealed class GetBookingByIdHandler : IRequestHandler<GetBookingByIdQuery, Result<Booking?>>
{
    private readonly IBookingRepository _repo;
    public GetBookingByIdHandler(IBookingRepository repo) => _repo = repo;

    public async Task<Result<Booking?>> Handle(GetBookingByIdQuery request, CancellationToken ct)
    {
        var booking = await _repo.GetByIdAsync(request.BookingId, ct);
        return Result.Success(booking);
    }
}

// ── List bookings ─────────────────────────────────────────────────────────
public sealed record ListBookingsQuery(
    string? OrgId,
    string? TravellerUserId,
    int? StatusFilter,
    int Page,
    int PageSize) : IRequest<Result<(IReadOnlyList<Booking> Items, int Total)>>;

public sealed class ListBookingsHandler : IRequestHandler<ListBookingsQuery, Result<(IReadOnlyList<Booking> Items, int Total)>>
{
    private readonly IBookingRepository _repo;
    public ListBookingsHandler(IBookingRepository repo) => _repo = repo;

    public async Task<Result<(IReadOnlyList<Booking> Items, int Total)>> Handle(ListBookingsQuery request, CancellationToken ct)
    {
        var result = await _repo.ListAsync(request.OrgId, request.TravellerUserId, request.StatusFilter, request.Page, request.PageSize, ct);
        return Result.Success(result);
    }
}
