namespace Cinturon360.Contracts.Flights;

// ── Offer Search ───────────────────────────────────────────────────────────

public sealed record DuffelOfferSearchRequest(
    string OrgId,
    string CabinClass,
    int MaxConnections,
    IReadOnlyList<DuffelPassengerSearchRequest> Passengers,
    IReadOnlyList<DuffelSliceSearchRequest> Slices);

public sealed record DuffelPassengerSearchRequest(
    string Type,
    int? Age);

public sealed record DuffelSliceSearchRequest(
    string Origin,
    string Destination,
    string DepartureDate);

public sealed record DuffelOfferSearchResponse(
    string OfferRequestId,
    IReadOnlyList<DuffelOfferSummary> Offers);

public sealed record DuffelOfferSummary(
    string OfferId,
    string TotalAmount,
    string TotalCurrency,
    string? TotalEmissionsKg,
    DateTimeOffset ExpiresAt,
    DuffelAirlineSummary Owner,
    IReadOnlyList<DuffelSliceSummary> Slices,
    IReadOnlyList<DuffelPassengerTypeSummary> Passengers);

public sealed record DuffelAirlineSummary(
    string? IataCode,
    string? Name);

public sealed record DuffelSliceSummary(
    string OriginCode,
    string DestinationCode,
    string? Duration,
    IReadOnlyList<DuffelSegmentSummary> Segments);

public sealed record DuffelSegmentSummary(
    string? FlightNumber,
    string? CarrierCode,
    string OriginCode,
    string DestinationCode,
    DateTimeOffset DepartingAt,
    DateTimeOffset ArrivingAt,
    string? Duration,
    string? CabinClass);

public sealed record DuffelPassengerTypeSummary(
    string PassengerId,
    string Type);

// ── Order Creation ─────────────────────────────────────────────────────────

public sealed record DuffelCreateOrderRequest(
    string OrgId,
    string SelectedOfferId,
    string TravellerUserId,
    string? ExistingBookingId,
    IReadOnlyList<DuffelOrderPassengerRequest> Passengers,
    DuffelOrderPaymentRequest Payment);

public sealed record DuffelOrderPassengerRequest(
    string PassengerId,
    string Title,
    string GivenName,
    string FamilyName,
    string Email,
    string PhoneNumber,
    string DateOfBirth,
    string? Gender,
    string? PassportNumber,
    string? PassportExpiresOn,
    string? PassportCountryCode);

public sealed record DuffelOrderPaymentRequest(
    string Type,
    string Currency,
    string Amount);

public sealed record DuffelCreateOrderResponse(
    string DuffelOrderId,
    string BookingId,
    string Status,
    string TotalAmount,
    string TotalCurrency,
    string? PnrCode,
    DateTimeOffset OrderedAt);

// ── Order Cancellation ─────────────────────────────────────────────────────

public sealed record DuffelCancelOrderRequest(
    string OrgId,
    string DuffelOrderId,
    string? BookingId);

public sealed record DuffelCancelOrderResponse(
    string DuffelOrderId,
    string CancellationId,
    string? BookingId,
    string? RefundAmount,
    string? RefundCurrency,
    string? RefundTo,
    DateTimeOffset? ConfirmedAt);
