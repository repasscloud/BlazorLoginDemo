using Microsoft.EntityFrameworkCore;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Data.Context;
using Cinturon360.Domain.Entities.Booking;

namespace Cinturon360.Data.Repositories;

internal sealed class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _db;
    public BookingRepository(AppDbContext db) => _db = db;

    public Task<Booking?> GetByIdAsync(string id, CancellationToken ct)
        => _db.Bookings.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<(IReadOnlyList<Booking> Items, int Total)> ListAsync(
        string? orgId,
        string? travellerUserId,
        int? statusFilter,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var query = _db.Bookings.AsQueryable();
        if (orgId is not null) query = query.Where(x => x.OrgId == orgId);
        if (travellerUserId is not null) query = query.Where(x => x.TravellerUserId == travellerUserId);
        if (statusFilter.HasValue) query = query.Where(x => (int)x.Status == statusFilter.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task AddAsync(Booking booking, CancellationToken ct)
        => await _db.Bookings.AddAsync(booking, ct);

    public void Update(Booking booking)
        => _db.Bookings.Update(booking);

    public async Task<IReadOnlyList<BookingItem>> GetItemsAsync(string bookingId, CancellationToken ct)
        => await _db.BookingItems
            .Where(x => x.BookingId == bookingId)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(ct);

    public async Task AddItemAsync(BookingItem item, CancellationToken ct)
        => await _db.BookingItems.AddAsync(item, ct);

    public void UpdateItem(BookingItem item)
        => _db.BookingItems.Update(item);
}

internal sealed class QuoteRepository : IQuoteRepository
{
    private readonly AppDbContext _db;
    public QuoteRepository(AppDbContext db) => _db = db;

    public Task<Quote?> GetByIdAsync(string id, CancellationToken ct)
        => _db.Quotes.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Quote quote, CancellationToken ct)
        => await _db.Quotes.AddAsync(quote, ct);

    public void Update(Quote quote)
        => _db.Quotes.Update(quote);
}
