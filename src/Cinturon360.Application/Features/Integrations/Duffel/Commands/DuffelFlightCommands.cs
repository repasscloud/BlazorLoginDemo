using System.Text.Json;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Application.Features.Integrations.Duffel.Services;
using Cinturon360.Common.IdGeneration;
using Cinturon360.Common.Results;
using Cinturon360.Contracts.Flights;
using Cinturon360.Domain.Entities.Booking;
using Cinturon360.Domain.Enums.Booking;
using Cinturon360.Domain.Enums.Travel;
using Cinturon360.Integrations.Flights.Duffel;
using MediatR;

namespace Cinturon360.Application.Features.Integrations.Duffel.Commands;

// ════════════════════════════════════════════════════════════════════════════
// ── Shared serialiser options ────────────────────────────────────────────
// ════════════════════════════════════════════════════════════════════════════

file static class DuffelJson
{
    internal static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy        = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition      = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    internal static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    internal static T Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, Options)
        ?? throw new InvalidOperationException($"Duffel response could not be deserialised to {typeof(T).Name}.");
}

// ════════════════════════════════════════════════════════════════════════════
// ── 1. Offer Search ──────────────────────────────────────────────────────
// ════════════════════════════════════════════════════════════════════════════

public sealed record DuffelSearchOffersCommand(
    string OrgId,
    string CabinClass,
    int MaxConnections,
    IReadOnlyList<DuffelPassengerSearchRequest> Passengers,
    IReadOnlyList<DuffelSliceSearchRequest> Slices)
    : IRequest<Result<DuffelOfferSearchResponse>>;

public sealed class DuffelSearchOffersCommandHandler(
    IDuffelConfigResolver configResolver,
    IDuffelApiClient duffelClient,
    IQuoteRepository quoteRepo,
    IUnitOfWork uow)
    : IRequestHandler<DuffelSearchOffersCommand, Result<DuffelOfferSearchResponse>>
{
    private static readonly Error ConfigNotFound =
        new("duffel.config_not_found", "No enabled Duffel configuration was found for this organisation.");
    private static readonly Error SearchFailed =
        new("duffel.search_failed", "Duffel offer search failed.");

    public async Task<Result<DuffelOfferSearchResponse>> Handle(
        DuffelSearchOffersCommand request, CancellationToken ct)
    {
        var config = await configResolver.ResolveAsync(request.OrgId, ct);
        if (config is null)
            return Result.Failure<DuffelOfferSearchResponse>(ConfigNotFound);

        try
        {
            // ── Step 1: create offer request ─────────────────────────────
            var offerRequestPayload = new DuffelOfferRequestPayload(
                new DuffelOfferRequestData(
                    Slices: request.Slices.Select(s =>
                        new DuffelSliceInputModel(s.Origin, s.Destination, s.DepartureDate)).ToList(),
                    Passengers: request.Passengers.Select(p =>
                        new DuffelPassengerTypeInputModel(p.Type, p.Age)).ToList(),
                    CabinClass: request.CabinClass.ToLowerInvariant(),
                    MaxConnections: request.MaxConnections));

            var offerRequestJson = DuffelJson.Serialize(offerRequestPayload);
            var offerRequestRaw  = await duffelClient.PostAsync(
                config.ApiBaseUrl, config.ApiToken,
                "air/offer_requests?return_offers=false", offerRequestJson, ct);

            var offerRequestResponse = DuffelJson.Deserialize<DuffelOfferRequestResponse>(offerRequestRaw);
            var offerRequestId       = offerRequestResponse.Data.Id;

            // ── Step 2: fetch offers ──────────────────────────────────────
            var offersRaw = await duffelClient.GetAsync(
                config.ApiBaseUrl, config.ApiToken,
                $"air/offers?offer_request_id={offerRequestId}&sort=total_amount", ct);

            var offersResponse = DuffelJson.Deserialize<DuffelOffersListResponse>(offersRaw);
            var offers         = offersResponse.Data;

            // ── Step 3: persist snapshot as a Quote ───────────────────────
            var quote = Quote.Create(
                IdGenerator.New(IdPrefix.Quote),
                orgId:             request.OrgId,
                requestedByUserId: string.Empty, // resolved at booking creation time
                totalAmount:       0m,           // populated when user selects offer
                currencyCode:      offers.FirstOrDefault()?.TotalCurrency ?? "USD",
                expiresAt:         offers.FirstOrDefault()?.ExpiresAt ?? DateTimeOffset.UtcNow.AddMinutes(30),
                providerRef:       offerRequestId);

            quote.SetProviderPayload(offersRaw);
            await quoteRepo.AddAsync(quote, ct);
            await uow.SaveChangesAsync(ct);

            // ── Step 4: map to response ───────────────────────────────────
            var offerSummaries = offers.Select(MapOffer).ToList();

            return Result.Success(new DuffelOfferSearchResponse(offerRequestId, offerSummaries));
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure<DuffelOfferSearchResponse>(
                new Error("duffel.search_failed", $"Duffel offer search failed: {ex.Message}"));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Result.Failure<DuffelOfferSearchResponse>(SearchFailed);
        }
    }

    private static DuffelOfferSummary MapOffer(DuffelOfferModel o) =>
        new(
            OfferId:          o.Id,
            TotalAmount:      o.TotalAmount,
            TotalCurrency:    o.TotalCurrency,
            TotalEmissionsKg: o.TotalEmissionsKg,
            ExpiresAt:        o.ExpiresAt,
            Owner:            new DuffelAirlineSummary(o.Owner.IataCode, o.Owner.Name),
            Slices:           o.Slices.Select(MapSlice).ToList(),
            Passengers:       o.Passengers.Select(p => new DuffelPassengerTypeSummary(p.Id, p.Type)).ToList());

    private static DuffelSliceSummary MapSlice(DuffelSliceModel s) =>
        new(
            OriginCode:      s.Origin.IataCode ?? string.Empty,
            DestinationCode: s.Destination.IataCode ?? string.Empty,
            Duration:        s.Duration,
            Segments:        s.Segments.Select(MapSegment).ToList());

    private static DuffelSegmentSummary MapSegment(DuffelSegmentModel seg)
    {
        var cabinClass = seg.Passengers?.FirstOrDefault()?.CabinClass;
        return new DuffelSegmentSummary(
            FlightNumber:    seg.FlightNumber,
            CarrierCode:     seg.MarketingCarrier?.IataCode,
            OriginCode:      seg.Origin.IataCode ?? string.Empty,
            DestinationCode: seg.Destination.IataCode ?? string.Empty,
            DepartingAt:     seg.DepartingAt,
            ArrivingAt:      seg.ArrivingAt,
            Duration:        seg.Duration,
            CabinClass:      cabinClass);
    }
}

// ════════════════════════════════════════════════════════════════════════════
// ── 2. Create Order ──────────────────────────────────────────────────────
// ════════════════════════════════════════════════════════════════════════════

public sealed record DuffelCreateOrderCommand(
    string OrgId,
    string SelectedOfferId,
    string TravellerUserId,
    string? ExistingBookingId,
    IReadOnlyList<DuffelOrderPassengerRequest> Passengers,
    DuffelOrderPaymentRequest Payment)
    : IRequest<Result<DuffelCreateOrderResponse>>;

public sealed class DuffelCreateOrderCommandHandler(
    IDuffelConfigResolver configResolver,
    IDuffelApiClient duffelClient,
    IBookingRepository bookingRepo,
    IUnitOfWork uow)
    : IRequestHandler<DuffelCreateOrderCommand, Result<DuffelCreateOrderResponse>>
{
    private static readonly Error ConfigNotFound =
        new("duffel.config_not_found", "No enabled Duffel configuration was found for this organisation.");
    private static readonly Error BookingNotFound =
        new("duffel.booking_not_found", "The specified booking was not found.");
    private static readonly Error OrderFailed =
        new("duffel.order_failed", "Duffel order creation failed.");

    public async Task<Result<DuffelCreateOrderResponse>> Handle(
        DuffelCreateOrderCommand request, CancellationToken ct)
    {
        var config = await configResolver.ResolveAsync(request.OrgId, ct);
        if (config is null)
            return Result.Failure<DuffelCreateOrderResponse>(ConfigNotFound);

        try
        {
            // ── Step 1: create order via Duffel ──────────────────────────
            var passengers = request.Passengers.Select(p => new DuffelOrderPassengerModel(
                Id:         p.PassengerId,
                Title:      p.Title.ToLowerInvariant(),
                GivenName:  p.GivenName,
                FamilyName: p.FamilyName,
                Email:      p.Email,
                PhoneNumber: p.PhoneNumber,
                DateOfBirth: p.DateOfBirth,
                Gender:     p.Gender?.ToLowerInvariant(),
                IdentityDocuments: BuildIdentityDocuments(p))).ToList();

            var payload = new DuffelCreateOrderPayload(new DuffelCreateOrderDataModel(
                SelectedOffers: [request.SelectedOfferId],
                Passengers:     passengers,
                Payments:
                [
                    new DuffelPaymentModel(
                        request.Payment.Type,
                        request.Payment.Currency,
                        request.Payment.Amount)
                ],
                Type: "instant"));

            var orderJson = DuffelJson.Serialize(payload);
            var orderRaw  = await duffelClient.PostAsync(
                config.ApiBaseUrl, config.ApiToken, "air/orders", orderJson, ct);

            var orderResponse = DuffelJson.Deserialize<DuffelOrderResponse>(orderRaw);
            var order         = orderResponse.Data;

            // ── Step 2: resolve or create internal booking ────────────────
            Booking booking;
            if (!string.IsNullOrWhiteSpace(request.ExistingBookingId))
            {
                booking = await bookingRepo.GetByIdAsync(request.ExistingBookingId, ct)
                    ?? throw new InvalidOperationException("Booking not found.");
            }
            else
            {
                var newId = IdGenerator.New(IdPrefix.Booking);
                booking = Booking.Create(newId, request.OrgId, request.TravellerUserId,
                    bookedByUserId: null, currencyCode: order.TotalCurrency);
                await bookingRepo.AddAsync(booking, ct);
            }

            // ── Step 3: update booking state ──────────────────────────────
            if (decimal.TryParse(order.TotalAmount,
                    System.Globalization.NumberStyles.AllowDecimalPoint,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var totalAmount))
            {
                booking.SetTotalAmount(totalAmount);
            }

            booking.Confirm(pnr: order.BookingReference, supplierRef: order.Id);

            // ── Step 4: create booking items from slices ──────────────────
            var sortOrder = 0;
            foreach (var slice in order.Slices)
            {
                foreach (var segment in slice.Segments)
                {
                    var itemId   = IdGenerator.New(IdPrefix.BookingItem);
                    var cabinStr = segment.Passengers?.FirstOrDefault()?.CabinClass ?? string.Empty;
                    var cabin    = ParseCabinClass(cabinStr);

                    var item = BookingItem.Create(
                        itemId, booking.Id,
                        BookingItemType.Flight, sortOrder++,
                        grossAmount:  totalAmount / Math.Max(order.Slices.Sum(sl => sl.Segments.Count), 1),
                        currencyCode: order.TotalCurrency);

                    item.SetFlightDetails(
                        origin:       segment.Origin.IataCode      ?? string.Empty,
                        destination:  segment.Destination.IataCode ?? string.Empty,
                        departure:    segment.DepartingAt,
                        arrival:      segment.ArrivingAt,
                        flightNumber: $"{segment.MarketingCarrier?.IataCode}{segment.FlightNumber}",
                        cabin:        cabin);

                    await bookingRepo.AddItemAsync(item, ct);
                }
            }

            // ── Step 5: set trip dates ─────────────────────────────────────
            var firstDeparture = order.Slices
                .SelectMany(s => s.Segments)
                .OrderBy(s => s.DepartingAt)
                .Select(s => s.DepartingAt)
                .FirstOrDefault();

            var lastArrival = order.Slices
                .SelectMany(s => s.Segments)
                .OrderByDescending(s => s.ArrivingAt)
                .Select(s => s.ArrivingAt)
                .FirstOrDefault();

            if (firstDeparture != default && lastArrival != default)
                booking.SetTripDates(firstDeparture, lastArrival);

            await uow.SaveChangesAsync(ct);

            return Result.Success(new DuffelCreateOrderResponse(
                DuffelOrderId: order.Id,
                BookingId:     booking.Id,
                Status:        booking.Status.ToString(),
                TotalAmount:   order.TotalAmount,
                TotalCurrency: order.TotalCurrency,
                PnrCode:       order.BookingReference,
                OrderedAt:     order.CreatedAt));
        }
        catch (InvalidOperationException ex) when (ex.Message == "Booking not found.")
        {
            return Result.Failure<DuffelCreateOrderResponse>(BookingNotFound);
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure<DuffelCreateOrderResponse>(
                new Error("duffel.order_failed", $"Duffel order creation failed: {ex.Message}"));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Result.Failure<DuffelCreateOrderResponse>(OrderFailed);
        }
    }

    private static IReadOnlyList<DuffelIdentityDocumentModel>? BuildIdentityDocuments(
        DuffelOrderPassengerRequest p)
    {
        if (string.IsNullOrWhiteSpace(p.PassportNumber))
            return null;

        return
        [
            new DuffelIdentityDocumentModel(
                Type:               "passport",
                UniqueIdentifier:   p.PassportNumber,
                ExpiresOn:          p.PassportExpiresOn,
                IssuingCountryCode: p.PassportCountryCode)
        ];
    }

    private static CabinClass ParseCabinClass(string raw) =>
        raw.ToLowerInvariant() switch
        {
            "business"         => CabinClass.Business,
            "first"            => CabinClass.First,
            "premium_economy"  => CabinClass.PremiumEconomy,
            "premium economy"  => CabinClass.PremiumEconomy,
            _                  => CabinClass.Economy
        };
}

// ════════════════════════════════════════════════════════════════════════════
// ── 3. Cancel Order ──────────────────────────────────────────────────────
// ════════════════════════════════════════════════════════════════════════════

public sealed record DuffelCancelOrderCommand(
    string OrgId,
    string DuffelOrderId,
    string? BookingId)
    : IRequest<Result<DuffelCancelOrderResponse>>;

public sealed class DuffelCancelOrderCommandHandler(
    IDuffelConfigResolver configResolver,
    IDuffelApiClient duffelClient,
    IBookingRepository bookingRepo,
    IUnitOfWork uow)
    : IRequestHandler<DuffelCancelOrderCommand, Result<DuffelCancelOrderResponse>>
{
    private static readonly Error ConfigNotFound =
        new("duffel.config_not_found", "No enabled Duffel configuration was found for this organisation.");
    private static readonly Error CancelFailed =
        new("duffel.cancel_failed", "Duffel order cancellation failed.");

    public async Task<Result<DuffelCancelOrderResponse>> Handle(
        DuffelCancelOrderCommand request, CancellationToken ct)
    {
        var config = await configResolver.ResolveAsync(request.OrgId, ct);
        if (config is null)
            return Result.Failure<DuffelCancelOrderResponse>(ConfigNotFound);

        try
        {
            // ── Step 1: initiate cancellation ─────────────────────────────
            var cancellationInitRaw = await duffelClient.PostAsync(
                config.ApiBaseUrl, config.ApiToken,
                $"air/orders/{request.DuffelOrderId}/cancellations",
                "{\"data\":{}}",
                ct);

            var cancellationInit = DuffelJson.Deserialize<DuffelCancellationApiResponse>(cancellationInitRaw);
            var cancellationId   = cancellationInit.Data.Id;

            // ── Step 2: confirm cancellation ──────────────────────────────
            var confirmRaw = await duffelClient.PostAsync(
                config.ApiBaseUrl, config.ApiToken,
                $"air/orders/{request.DuffelOrderId}/cancellations/{cancellationId}/actions/confirm",
                "{}",
                ct);

            var confirmed = DuffelJson.Deserialize<DuffelCancellationApiResponse>(confirmRaw);

            // ── Step 3: update internal booking if provided ───────────────
            string? internalBookingId = null;
            if (!string.IsNullOrWhiteSpace(request.BookingId))
            {
                var booking = await bookingRepo.GetByIdAsync(request.BookingId, ct);
                if (booking is not null && booking.Status != BookingStatus.Cancelled)
                {
                    booking.Cancel($"Cancelled via Duffel — cancellation ID: {cancellationId}");
                    bookingRepo.Update(booking);
                    await uow.SaveChangesAsync(ct);
                    internalBookingId = booking.Id;
                }
            }

            return Result.Success(new DuffelCancelOrderResponse(
                DuffelOrderId:  request.DuffelOrderId,
                CancellationId: cancellationId,
                BookingId:      internalBookingId,
                RefundAmount:   confirmed.Data.RefundAmount,
                RefundCurrency: confirmed.Data.RefundCurrency,
                RefundTo:       confirmed.Data.RefundTo,
                ConfirmedAt:    confirmed.Data.ConfirmedAt));
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure<DuffelCancelOrderResponse>(
                new Error("duffel.cancel_failed", $"Duffel order cancellation failed: {ex.Message}"));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Result.Failure<DuffelCancelOrderResponse>(CancelFailed);
        }
    }
}
