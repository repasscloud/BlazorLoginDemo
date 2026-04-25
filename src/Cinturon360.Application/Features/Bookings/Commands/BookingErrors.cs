using Cinturon360.Common.Results;

namespace Cinturon360.Application.Features.Bookings.Commands;

public static class BookingErrors
{
    public static readonly Error NotFound          = new("booking.not_found",    "Booking not found.");
    public static readonly Error InvalidStatus     = new("booking.invalid_status", "Booking status transition is not valid.");
    public static readonly Error AlreadyCancelled  = new("booking.already_cancelled", "Booking is already cancelled.");
}
