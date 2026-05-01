using MediatR;
using Microsoft.Extensions.Logging;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Application.Abstractions.Services;
using Cinturon360.Common.IdGeneration;
using Cinturon360.Common.Results;
using Cinturon360.Domain.Entities.Booking;
using Cinturon360.Domain.Enums.Booking;

namespace Cinturon360.Application.Features.Bookings.Commands;

// ── Create booking ────────────────────────────────────────────────────────
public sealed record CreateBookingCommand(
    string OrgId,
    string TravellerUserId,
    string? BookedByUserId,
    string CurrencyCode,
    string? QuoteId) : IRequest<Result<string>>;

public sealed class CreateBookingHandler : IRequestHandler<CreateBookingCommand, Result<string>>
{
    private readonly IBookingRepository _repo;
    private readonly IUnitOfWork _uow;

    public CreateBookingHandler(IBookingRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result<string>> Handle(CreateBookingCommand request, CancellationToken ct)
    {
        var id = IdGenerator.New(IdPrefix.Booking);
        var booking = Booking.Create(id, request.OrgId, request.TravellerUserId,
            request.BookedByUserId, request.CurrencyCode, request.QuoteId);

        await _repo.AddAsync(booking, ct);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(id);
    }
}

// ── Cancel booking ────────────────────────────────────────────────────────
public sealed record CancelBookingCommand(string BookingId, string Reason) : IRequest<Result>;

public sealed class CancelBookingHandler : IRequestHandler<CancelBookingCommand, Result>
{
    private readonly IBookingRepository _repo;
    private readonly IUnitOfWork _uow;

    public CancelBookingHandler(IBookingRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result> Handle(CancelBookingCommand request, CancellationToken ct)
    {
        var booking = await _repo.GetByIdAsync(request.BookingId, ct);
        if (booking is null) return Result.Failure(BookingErrors.NotFound);
        if (booking.Status == BookingStatus.Cancelled) return Result.Failure(BookingErrors.AlreadyCancelled);

        booking.Cancel(request.Reason);
        _repo.Update(booking);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Confirm booking ───────────────────────────────────────────────────────
public sealed record ConfirmBookingCommand(
    string BookingId,
    string? PnrCode,
    string? SupplierRef) : IRequest<Result>;

public sealed class ConfirmBookingHandler(
    IBookingRepository repo,
    IUserRepository userRepo,
    IEmailService email,
    IUnitOfWork uow,
    ILogger<ConfirmBookingHandler> logger) : IRequestHandler<ConfirmBookingCommand, Result>
{
    public async Task<Result> Handle(ConfirmBookingCommand request, CancellationToken ct)
    {
        var booking = await repo.GetByIdAsync(request.BookingId, ct);
        if (booking is null) return Result.Failure(BookingErrors.NotFound);

        if (booking.Status is not (BookingStatus.Draft or BookingStatus.Approved))
            return Result.Failure(BookingErrors.InvalidStatus);

        booking.Confirm(request.PnrCode, request.SupplierRef);
        repo.Update(booking);
        await uow.SaveChangesAsync(ct);

        // Send booking confirmation email to traveller
        var traveller = await userRepo.GetByIdAsync(booking.TravellerUserId, ct);
        if (traveller is not null)
        {
            try
            {
                await email.SendAsync(
                    traveller.Email,
                    traveller.FullName,
                    $"Booking Confirmed — {booking.Id}",
                    $"<p>Hi {traveller.FirstName},</p>" +
                    $"<p>Your booking <strong>{booking.Id}</strong> has been confirmed." +
                    (request.PnrCode is not null ? $" PNR: <strong>{request.PnrCode}</strong>." : string.Empty) +
                    "</p><p>Thank you for booking with Cinturon360.</p>",
                    ct: ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send booking confirmation email for {BookingId}", booking.Id);
            }
        }

        return Result.Success();
    }
}
