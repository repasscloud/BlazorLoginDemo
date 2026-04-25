namespace Cinturon360.Domain.Enums.Booking;
public enum QuoteStatus
{
    Active     = 1,
    Expired    = 2,
    Booked     = 3,
    Abandoned  = 4
}

public enum BookingItemType
{
    Flight  = 1,
    Hotel   = 2,
    Car     = 3,
    Rail    = 4,
    Other   = 5
}

public enum RefundStatus
{
    NotRequested = 1,
    Requested    = 2,
    Approved     = 3,
    Processed    = 4,
    Denied       = 5
}
