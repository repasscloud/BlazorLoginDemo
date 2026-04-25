using Cinturon360.Domain.Common.Base;
using Cinturon360.Domain.Common.ValueObjects;
using Cinturon360.Domain.Enums.Booking;

namespace Cinturon360.Domain.Entities.Booking;

/// <summary>
/// A travel booking record. Ties together a traveller, org, and one or more booking items.
/// The booking goes through a lifecycle: Draft → PendingApproval → Approved → Confirmed → Ticketed → Completed.
/// </summary>
public sealed class Booking : SoftDeletableEntity
{
    public string OrgId { get; private set; } = string.Empty;
    public string TravellerUserId { get; private set; } = string.Empty;
    public string? BookedByUserId { get; private set; }
    public string? QuoteId { get; private set; }
    public BookingStatus Status { get; private set; } = BookingStatus.Draft;

    // Totals
    public decimal TotalAmountGross { get; private set; }
    public string CurrencyCode { get; private set; } = "USD";

    // Reference
    public string? PnrCode { get; private set; }
    public string? SupplierRef { get; private set; }
    public string? ExternalRef { get; private set; }

    // Dates
    public DateTimeOffset? TripStartDate { get; private set; }
    public DateTimeOffset? TripEndDate { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    // Approval
    public int ApprovalLevelsRequired { get; private set; }
    public int ApprovalLevelsCompleted { get; private set; }

    private Booking() { }

    public static Booking Create(
        string id,
        string orgId,
        string travellerUserId,
        string? bookedByUserId,
        string currencyCode,
        string? quoteId = null)
        => new()
        {
            Id = id,
            OrgId = orgId,
            TravellerUserId = travellerUserId,
            BookedByUserId = bookedByUserId,
            QuoteId = quoteId,
            CurrencyCode = currencyCode,
            Status = BookingStatus.Draft,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void SetTripDates(DateTimeOffset start, DateTimeOffset end)
    {
        TripStartDate = start;
        TripEndDate = end;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetTotalAmount(decimal amount) { TotalAmountGross = amount; UpdatedAt = DateTimeOffset.UtcNow; }

    public void RequireApproval(int levels)
    {
        ApprovalLevelsRequired = levels;
        Status = BookingStatus.PendingApproval;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RecordApproval()
    {
        ApprovalLevelsCompleted++;
        if (ApprovalLevelsCompleted >= ApprovalLevelsRequired)
            Status = BookingStatus.Approved;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Confirm(string? pnr = null, string? supplierRef = null)
    {
        Status = BookingStatus.Confirmed;
        PnrCode = pnr;
        SupplierRef = supplierRef;
        ConfirmedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkTicketed() { Status = BookingStatus.Ticketed; UpdatedAt = DateTimeOffset.UtcNow; }
    public void MarkCompleted() { Status = BookingStatus.Completed; UpdatedAt = DateTimeOffset.UtcNow; }

    public void Cancel(string reason)
    {
        Status = BookingStatus.Cancelled;
        CancellationReason = reason;
        CancelledAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
