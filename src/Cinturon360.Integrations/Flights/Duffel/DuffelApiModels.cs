using System.Text.Json.Serialization;

namespace Cinturon360.Integrations.Flights.Duffel;

// ── Offer Request ──────────────────────────────────────────────────────────

public sealed record DuffelOfferRequestPayload(
    [property: JsonPropertyName("data")] DuffelOfferRequestData Data);

public sealed record DuffelOfferRequestData(
    [property: JsonPropertyName("slices")]        IReadOnlyList<DuffelSliceInputModel> Slices,
    [property: JsonPropertyName("passengers")]    IReadOnlyList<DuffelPassengerTypeInputModel> Passengers,
    [property: JsonPropertyName("cabin_class")]   string CabinClass,
    [property: JsonPropertyName("max_connections")] int MaxConnections);

public sealed record DuffelSliceInputModel(
    [property: JsonPropertyName("origin")]         string Origin,
    [property: JsonPropertyName("destination")]    string Destination,
    [property: JsonPropertyName("departure_date")] string DepartureDate);

public sealed record DuffelPassengerTypeInputModel(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("age")]  int? Age);

// ── Offer Request Response ─────────────────────────────────────────────────

public sealed record DuffelOfferRequestResponse(
    [property: JsonPropertyName("data")] DuffelOfferRequestResponseData Data);

public sealed record DuffelOfferRequestResponseData(
    [property: JsonPropertyName("id")]         string Id,
    [property: JsonPropertyName("passengers")] IReadOnlyList<DuffelPassengerModel> Passengers);

// ── Offers List ────────────────────────────────────────────────────────────

public sealed record DuffelOffersListResponse(
    [property: JsonPropertyName("data")] IReadOnlyList<DuffelOfferModel> Data);

public sealed record DuffelOfferModel(
    [property: JsonPropertyName("id")]                 string Id,
    [property: JsonPropertyName("total_amount")]       string TotalAmount,
    [property: JsonPropertyName("total_currency")]     string TotalCurrency,
    [property: JsonPropertyName("total_emissions_kg")] string? TotalEmissionsKg,
    [property: JsonPropertyName("expires_at")]         DateTimeOffset ExpiresAt,
    [property: JsonPropertyName("owner")]              DuffelAirlineModel Owner,
    [property: JsonPropertyName("slices")]             IReadOnlyList<DuffelSliceModel> Slices,
    [property: JsonPropertyName("passengers")]         IReadOnlyList<DuffelPassengerModel> Passengers);

public sealed record DuffelAirlineModel(
    [property: JsonPropertyName("iata_code")] string? IataCode,
    [property: JsonPropertyName("name")]      string? Name);

public sealed record DuffelSliceModel(
    [property: JsonPropertyName("origin")]      DuffelLocationModel Origin,
    [property: JsonPropertyName("destination")] DuffelLocationModel Destination,
    [property: JsonPropertyName("duration")]    string? Duration,
    [property: JsonPropertyName("segments")]    IReadOnlyList<DuffelSegmentModel> Segments);

public sealed record DuffelLocationModel(
    [property: JsonPropertyName("iata_code")]  string? IataCode,
    [property: JsonPropertyName("name")]       string? Name,
    [property: JsonPropertyName("city_name")]  string? CityName);

public sealed record DuffelSegmentModel(
    [property: JsonPropertyName("id")]                              string Id,
    [property: JsonPropertyName("marketing_carrier")]               DuffelAirlineModel? MarketingCarrier,
    [property: JsonPropertyName("marketing_carrier_flight_number")] string? FlightNumber,
    [property: JsonPropertyName("origin")]                          DuffelLocationModel Origin,
    [property: JsonPropertyName("destination")]                     DuffelLocationModel Destination,
    [property: JsonPropertyName("departing_at")]                    DateTimeOffset DepartingAt,
    [property: JsonPropertyName("arriving_at")]                     DateTimeOffset ArrivingAt,
    [property: JsonPropertyName("duration")]                        string? Duration,
    [property: JsonPropertyName("passengers")]                      IReadOnlyList<DuffelSegmentPassengerModel>? Passengers);

public sealed record DuffelSegmentPassengerModel(
    [property: JsonPropertyName("passenger_id")]               string PassengerId,
    [property: JsonPropertyName("cabin_class")]                string? CabinClass,
    [property: JsonPropertyName("cabin_class_marketing_name")] string? CabinClassMarketingName);

public sealed record DuffelPassengerModel(
    [property: JsonPropertyName("id")]   string Id,
    [property: JsonPropertyName("type")] string Type);

// ── Order Creation ─────────────────────────────────────────────────────────

public sealed record DuffelCreateOrderPayload(
    [property: JsonPropertyName("data")] DuffelCreateOrderDataModel Data);

public sealed record DuffelCreateOrderDataModel(
    [property: JsonPropertyName("selected_offers")] IReadOnlyList<string> SelectedOffers,
    [property: JsonPropertyName("passengers")]      IReadOnlyList<DuffelOrderPassengerModel> Passengers,
    [property: JsonPropertyName("payments")]        IReadOnlyList<DuffelPaymentModel> Payments,
    [property: JsonPropertyName("type")]            string Type);

public sealed record DuffelOrderPassengerModel(
    [property: JsonPropertyName("id")]                  string Id,
    [property: JsonPropertyName("title")]               string Title,
    [property: JsonPropertyName("given_name")]          string GivenName,
    [property: JsonPropertyName("family_name")]         string FamilyName,
    [property: JsonPropertyName("email")]               string Email,
    [property: JsonPropertyName("phone_number")]        string PhoneNumber,
    [property: JsonPropertyName("date_of_birth")]       string DateOfBirth,
    [property: JsonPropertyName("gender")]              string? Gender,
    [property: JsonPropertyName("identity_documents")]  IReadOnlyList<DuffelIdentityDocumentModel>? IdentityDocuments);

public sealed record DuffelIdentityDocumentModel(
    [property: JsonPropertyName("type")]                string Type,
    [property: JsonPropertyName("unique_identifier")]   string UniqueIdentifier,
    [property: JsonPropertyName("expires_on")]          string? ExpiresOn,
    [property: JsonPropertyName("issuing_country_code")] string? IssuingCountryCode);

public sealed record DuffelPaymentModel(
    [property: JsonPropertyName("type")]     string Type,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("amount")]   string Amount);

// ── Order Response ─────────────────────────────────────────────────────────

public sealed record DuffelOrderResponse(
    [property: JsonPropertyName("data")] DuffelOrderDataModel Data);

public sealed record DuffelOrderDataModel(
    [property: JsonPropertyName("id")]                string Id,
    [property: JsonPropertyName("booking_reference")] string? BookingReference,
    [property: JsonPropertyName("total_amount")]      string TotalAmount,
    [property: JsonPropertyName("total_currency")]    string TotalCurrency,
    [property: JsonPropertyName("created_at")]        DateTimeOffset CreatedAt,
    [property: JsonPropertyName("slices")]            IReadOnlyList<DuffelSliceModel> Slices,
    [property: JsonPropertyName("passengers")]        IReadOnlyList<DuffelPassengerModel> Passengers);

// ── Cancellation ───────────────────────────────────────────────────────────

public sealed record DuffelCancellationApiResponse(
    [property: JsonPropertyName("data")] DuffelCancellationDataModel Data);

public sealed record DuffelCancellationDataModel(
    [property: JsonPropertyName("id")]              string Id,
    [property: JsonPropertyName("refund_amount")]   string? RefundAmount,
    [property: JsonPropertyName("refund_currency")] string? RefundCurrency,
    [property: JsonPropertyName("refund_to")]       string? RefundTo,
    [property: JsonPropertyName("confirmed_at")]    DateTimeOffset? ConfirmedAt);
