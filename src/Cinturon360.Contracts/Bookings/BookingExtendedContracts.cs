namespace Cinturon360.Contracts.Bookings;

/// <summary>Request to update a booking's status.</summary>
public sealed record UpdateBookingStatusRequest(string Status, string? Reason = null);

/// <summary>Alias for paginated booking list.</summary>
public sealed record BookingListResponse(
    IReadOnlyList<BookingResponse> Items,
    int Total,
    int Page,
    int PageSize);
