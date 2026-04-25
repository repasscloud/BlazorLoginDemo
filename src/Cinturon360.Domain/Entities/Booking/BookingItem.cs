using Cinturon360.Domain.Common.Base;
using Cinturon360.Domain.Enums.Booking;
using Cinturon360.Domain.Enums.Travel;

namespace Cinturon360.Domain.Entities.Booking;

/// <summary>
/// A single line item within a booking (one flight segment, one hotel stay, etc.)
/// </summary>
public sealed class BookingItem : Entity
{
    public string BookingId { get; private set; } = string.Empty;
    public BookingItemType ItemType { get; private set; }
    public int SortOrder { get; private set; }

    // Supplier details
    public string? SupplierCode { get; private set; }
    public string? SupplierRef { get; private set; }

    // Segment details (flexible — nullable fields used per item type)
    public string? OriginCode { get; private set; }
    public string? DestinationCode { get; private set; }
    public DateTimeOffset? DepartureAt { get; private set; }
    public DateTimeOffset? ArrivalAt { get; private set; }
    public string? FlightNumber { get; private set; }
    public CabinClass? CabinClass { get; private set; }
    public string? SeatNumber { get; private set; }

    // Hotel
    public string? HotelName { get; private set; }
    public DateOnly? CheckInDate { get; private set; }
    public DateOnly? CheckOutDate { get; private set; }

    // Pricing
    public decimal GrossAmount { get; private set; }
    public decimal? TaxAmount { get; private set; }
    public decimal? FeeAmount { get; private set; }
    public string CurrencyCode { get; private set; } = "USD";

    // Status
    public string? TicketNumber { get; private set; }
    public bool IsCancelled { get; private set; }

    private BookingItem() { }

    public static BookingItem Create(
        string id,
        string bookingId,
        BookingItemType itemType,
        int sortOrder,
        decimal grossAmount,
        string currencyCode)
        => new()
        {
            Id = id,
            BookingId = bookingId,
            ItemType = itemType,
            SortOrder = sortOrder,
            GrossAmount = grossAmount,
            CurrencyCode = currencyCode,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void SetFlightDetails(
        string origin, string destination,
        DateTimeOffset departure, DateTimeOffset arrival,
        string flightNumber, CabinClass cabin)
    {
        OriginCode = origin;
        DestinationCode = destination;
        DepartureAt = departure;
        ArrivalAt = arrival;
        FlightNumber = flightNumber;
        CabinClass = cabin;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetHotelDetails(string hotelName, DateOnly checkIn, DateOnly checkOut)
    {
        HotelName = hotelName;
        CheckInDate = checkIn;
        CheckOutDate = checkOut;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetTicketNumber(string ticketNumber) { TicketNumber = ticketNumber; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Cancel() { IsCancelled = true; UpdatedAt = DateTimeOffset.UtcNow; }
}
