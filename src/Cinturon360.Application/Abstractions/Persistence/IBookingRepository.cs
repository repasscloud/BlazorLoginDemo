using Cinturon360.Domain.Entities.Booking;

namespace Cinturon360.Application.Abstractions.Persistence;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<(IReadOnlyList<Booking> Items, int Total)> ListAsync(
        string? orgId,
        string? travellerUserId,
        int? statusFilter,
        int page,
        int pageSize,
        CancellationToken ct = default);
    Task AddAsync(Booking booking, CancellationToken ct = default);
    void Update(Booking booking);

    Task<IReadOnlyList<BookingItem>> GetItemsAsync(string bookingId, CancellationToken ct = default);
    Task AddItemAsync(BookingItem item, CancellationToken ct = default);
    void UpdateItem(BookingItem item);
}

public interface IQuoteRepository
{
    Task<Quote?> GetByIdAsync(string id, CancellationToken ct = default);
    Task AddAsync(Quote quote, CancellationToken ct = default);
    void Update(Quote quote);
}
