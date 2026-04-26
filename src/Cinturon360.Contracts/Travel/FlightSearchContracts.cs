namespace Cinturon360.Contracts.Travel;

// ── Flight Search Requests ────────────────────────────────────────────────

public sealed record FlightSearchRequest(
    string Origin,
    string Destination,
    DateOnly DepartureDate,
    DateOnly? ReturnDate,
    int Adults,
    string CabinClass,
    string CurrencyCode,
    bool DirectOnly = false);

// ── Flight Search Results ─────────────────────────────────────────────────

public sealed record FlightSearchResponse(
    IReadOnlyList<FlightOfferResponse> Offers,
    string SearchId,
    DateTimeOffset ExpiresAt);

public sealed record FlightOfferResponse(
    string OfferId,
    string ProviderId,
    decimal TotalPrice,
    decimal TaxesAndFees,
    string CurrencyCode,
    string CabinClass,
    bool IsRefundable,
    bool IsChangeable,
    PolicyComplianceInfo PolicyCompliance,
    IReadOnlyList<FlightSegmentResponse> OutboundSegments,
    IReadOnlyList<FlightSegmentResponse>? ReturnSegments,
    IReadOnlyList<FareConditionResponse> FareConditions);

public sealed record FlightSegmentResponse(
    string FlightNumber,
    string OperatingCarrierCode,
    string OperatingCarrierName,
    string MarketingCarrierCode,
    string Origin,
    string OriginCity,
    string Destination,
    string DestinationCity,
    DateTimeOffset DepartsAt,
    DateTimeOffset ArrivesAt,
    int DurationMinutes,
    string Aircraft,
    int Stops,
    IReadOnlyList<FlightStopResponse>? StopDetails);

public sealed record FlightStopResponse(
    string AirportCode,
    string AirportName,
    int LayoverMinutes);

public sealed record FareConditionResponse(
    string Category,
    string Description,
    bool Applicable);

public sealed record PolicyComplianceInfo(
    bool IsCompliant,
    string? ViolationReason,
    string PolicyBadgeLabel,
    string PolicyBadgeColour);
