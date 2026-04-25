namespace Cinturon360.Contracts.Bookings;

public sealed record CreateBookingRequest(
    string OrgId,
    string TravellerUserId,
    string? BookedByUserId,
    string CurrencyCode,
    string? QuoteId);

public sealed record ConfirmBookingRequest(
    string? PnrCode,
    string? SupplierRef);

public sealed record CancelBookingRequest(string Reason);

public sealed record BookingResponse(
    string Id,
    string OrgId,
    string TravellerUserId,
    string? BookedByUserId,
    string? QuoteId,
    string Status,
    decimal TotalAmountGross,
    string CurrencyCode,
    string? PnrCode,
    DateTimeOffset? TripStartDate,
    DateTimeOffset? TripEndDate,
    DateTimeOffset CreatedAt);

public sealed record PagedBookingsResponse(
    IReadOnlyList<BookingResponse> Items,
    int Total,
    int Page,
    int PageSize);
