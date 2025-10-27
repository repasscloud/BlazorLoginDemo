using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BlazorLoginDemo.Shared.Data;
using BlazorLoginDemo.Shared.Models.DTOs;
using BlazorLoginDemo.Shared.Models.ExternalLib.Amadeus;
using BlazorLoginDemo.Shared.Models.ExternalLib.Amadeus.Flight;
using BlazorLoginDemo.Shared.Models.ExternalLib.Kernel.Flight;
using BlazorLoginDemo.Shared.Models.Kernel.Travel;
using BlazorLoginDemo.Shared.Models.Static.SysVar;
using BlazorLoginDemo.Shared.Services.Interfaces.External;
using BlazorLoginDemo.Shared.Services.Interfaces.Kernel;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NanoidDotNet;

namespace BlazorLoginDemo.Shared.Services.External;

public class AmadeusFlightSearchService : IAmadeusFlightSearchService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApplicationDbContext _db;
    private readonly AmadeusOAuthClientSettings _settings;
    private readonly IAmadeusAuthService _authService;
    private readonly ILoggerService _loggerService;
    private readonly JsonSerializerOptions _jsonOptions;

    public AmadeusFlightSearchService(
        IHttpClientFactory httpClientFactory,
        ApplicationDbContext db,
        IOptions<AmadeusOAuthClientSettings> options,
        IAmadeusAuthService authService,
        ILoggerService loggerService,
        JsonSerializerOptions jsonOptions)
    {
        _httpClientFactory = httpClientFactory;
        _db = db;
        _settings = options.Value;
        _authService = authService;
        _loggerService = loggerService;
        _jsonOptions = jsonOptions;
    }

    public async Task<AmadeusFlightOfferSearchResult> GetFlightOffersAsync(FlightOfferSearchRequestDto dto, CancellationToken ct = default)
    {
        await _loggerService.InformationAsync(
            evt: "FLIGHT_OFFERS_REQ_START",
            cat: SysLogCatType.Api,  // we RECEIVED a request (not calling Amadeus yet)
            act: SysLogActionType.Start,
            message: $"Received flight offers request (dto={nameof(FlightOfferSearchRequestDto)}, id={dto.Id})",
            ent: nameof(FlightOfferSearchRequestDto),
            entId: dto.Id,
            note: "ingress:start");

        // 1) Load Travel Policy (optional)
        TravelPolicyBookingContextDto? policyCtx = null;
        if (!string.IsNullOrEmpty(dto.TravelPolicyId))
        {
            policyCtx = await _db.TravelPolicies
                .AsNoTracking()
                .Where(tp => tp.Id == dto.TravelPolicyId)
                .Select(tp => new TravelPolicyBookingContextDto
                {
                    Id = tp.Id,
                    PolicyName = tp.PolicyName,
                    // OrganizationUnifiedId is the new owner; the booking context does not need it here
                    Currency = tp.DefaultCurrencyCode,
                    MaxFlightPrice = tp.MaxFlightPrice,
                    DefaultFlightSeating = tp.DefaultFlightSeating,
                    MaxFlightSeating = tp.MaxFlightSeating,
                    CabinClassCoverage = tp.CabinClassCoverage,
                    FlightBookingTimeAvailableFrom = tp.FlightBookingTimeAvailableFrom,
                    FlightBookingTimeAvailableTo = tp.FlightBookingTimeAvailableTo,
                    IncludedAirlineCodes = (tp.IncludedAirlineCodes != null && tp.IncludedAirlineCodes.Length > 0)
                        ? tp.IncludedAirlineCodes.ToList()
                        : null,
                    ExcludedAirlineCodes = (tp.ExcludedAirlineCodes != null && tp.ExcludedAirlineCodes.Length > 0)
                        ? tp.ExcludedAirlineCodes.ToList()
                        : null,
                })
                .FirstOrDefaultAsync(ct);
        }

        // 2) Load the ApplicationUser (new unified user profile)
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == dto.CustomerId, ct);

        // 3) Currency selection: Policy -> User -> AUD
        string currencyCode = policyCtx?.Currency ?? user?.DefaultCurrencyCode ?? "AUD";

        // 4) Build origin/destination legs (one-way / return)
        var originDestinations = new List<OriginDestination>();

        var leg1 = new OriginDestination
        {
            Id = "1",
            OriginLocationCode = dto.OriginLocationCode,
            DestinationLocationCode = dto.DestinationLocationCode,
            DateTimeRange = new DepartureDateTimeRange
            {
                Date = dto.DepartureDate,
                Time = policyCtx?.FlightBookingTimeAvailableFrom ?? user?.FlightBookingTimeAvailableFrom,
            }
        };
        originDestinations.Add(leg1);

        if (!dto.IsOneWay && dto.DepartureDateReturn is not null)
        {
            var leg2 = new OriginDestination
            {
                Id = "2",
                OriginLocationCode = dto.DestinationLocationCode,
                DestinationLocationCode = dto.OriginLocationCode,
                DateTimeRange = new DepartureDateTimeRange
                {
                    Date = dto.DepartureDateReturn,
                    Time = policyCtx?.FlightBookingTimeAvailableFrom ?? user?.FlightBookingTimeAvailableFrom,
                }
            };
            originDestinations.Add(leg2);
        }

        // 5) Travelers
        var travelers = new List<Traveler>();
        foreach (var i in Enumerable.Range(1, dto.Adults))
        {
            travelers.Add(new Traveler
            {
                Id = i.ToString(),
                TravelerType = "ADULT",
                FareOptions = ["STANDARD"]
            });
        }

        // 6) Search criteria (max offers, filters)
        var searchCriteria = new SearchCriteria();

        // Max results: respect user's MaxResults when provided (1..250), else default 20
        if (user?.MaxResults is int mr && mr > 0)
            searchCriteria.MaxFlightOffers = Math.Min(mr, 250);
        else
            searchCriteria.MaxFlightOffers = 20;

        var filters = new FlightFilters();

        // Cabin restrictions (cover one-way vs return)
        var coverage = policyCtx?.CabinClassCoverage ?? user?.CabinClassCoverage ?? "MOST_SEGMENTS";
        var cabin = policyCtx?.DefaultFlightSeating ?? dto.CabinClass; // keep request cabin as fallback

        var cabinRestrictions = new List<CabinRestriction>
        {
            new CabinRestriction
            {
                Cabin = cabin,
                Coverage = coverage,
                OriginDestinationIds = dto.DepartureDateReturn is null ? [ "1" ] : [ "1", "2" ]
            }
        };
        filters.CabinRestrictions = cabinRestrictions;

        // Carrier restrictions: prefer EXCLUDED when both present; else INCLUDED; else none
        var included = policyCtx?.IncludedAirlineCodes
            ?? (user?.IncludedAirlineCodes?.Any() == true ? user.IncludedAirlineCodes.ToList() : null);

        var excluded = policyCtx?.ExcludedAirlineCodes
            ?? (user?.ExcludedAirlineCodes?.Any() == true ? user.ExcludedAirlineCodes.ToList() : null);

        CarrierRestriction? carrierRestrictions = null;
        if (excluded is { Count: > 0 })
            carrierRestrictions = new CarrierRestriction { ExcludedCarrierCodes = excluded };
        else if (included is { Count: > 0 })
            carrierRestrictions = new CarrierRestriction { IncludedCarrierCodes = included };

        if (carrierRestrictions is not null)
            filters.CarrierRestrictions = carrierRestrictions;

        searchCriteria.Filters = filters;

        // 7) Build request
        var flightOfferSearch = new AmadeusFlightOfferSearch
        {
            CurrencyCode = currencyCode,
            OriginDestinations = originDestinations,
            Travelers = travelers,
            SearchCriteria = searchCriteria
        };

        // DEBUG persist
        string debugJsonX = $@"/app/searchRequestDTO_debugJsonX_{dto.Id}.json";
        await _loggerService.DebugAsync(
            evt: "FLIGHT_OFFERS_SAVE_RESULTS",
            cat: SysLogCatType.Storage,                 // file/blob write
            act: SysLogActionType.Update,               // use Create if writing first time
            message: $"Saving results for request id={dto.Id} to {debugJsonX}",
            ent: "FlightOfferResults",
            entId: dto.Id,
            note: $"path:{debugJsonX}");

        var xJson = JsonSerializer.Serialize(flightOfferSearch, _jsonOptions);
        await File.WriteAllTextAsync(debugJsonX, xJson, ct);

        // 8) Auth
        var token = await _authService.GetTokenInformationAsync();
        if (string.IsNullOrEmpty(token))
        {
            await _loggerService.ErrorAsync(
                evt: "AMADEUS_OAUTH_TOKEN_FAIL",
                cat: SysLogCatType.Integration,
                act: SysLogActionType.Exec,
                ex: new InvalidOperationException("Unable to retrieve valid OAuth token."),
                message: "Unable to retrieve valid OAuth token.",
                ent: "AmadeusOAuth");
            throw new Exception("Unable to retrieve valid OAuth token.");
        }

        // 9) HTTP post
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string flightOfferUrl = _settings.Url.FlightOffer
            ?? throw new ArgumentNullException("Amadeus:Url:FlightOffer is missing in configuration.");

        var response = await httpClient.PostAsJsonAsync(flightOfferUrl, flightOfferSearch, _jsonOptions, ct);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AmadeusFlightOfferSearchResult>(cancellationToken: ct);
            if (result is null)
                throw new InvalidOperationException("Deserialization returned null.");

            // 10) Persist a record of the search
            var record = new FlightOfferSearchResultRecord
            {
                Id = await Nanoid.GenerateAsync(),
                MetaCount = result.Meta.Count,
                FlightOfferSearchRequestDtoId = dto.Id,
                ClientId = dto.ClientId,                // unchanged field in your record model
                AvaUserId = dto.CustomerId,             // now stores ApplicationUser.Id
                Source = SearchResultSource.Amadeus,
                AmadeusPayload = result
            };

            await _db.FlightOfferSearchResultRecords.AddAsync(record, ct);
            await _db.SaveChangesAsync(ct);
            await _loggerService.InformationAsync(
                evt: "AMADEUS_FLIGHT_REQ_SUCCESS",
                cat: SysLogCatType.Integration,
                act: SysLogActionType.Exec,
                message: "Calling Amadeus Flight Offers",
                ent: "FlightOfferSearch",
                entId: dto.Id,
                note: "provider:Amadeus");

            return result;
        }
        else
        {
            string errorBody = await response.Content.ReadAsStringAsync(ct);
            await _loggerService.ErrorAsync(
                evt: "AMADEUS_API_ERROR",
                cat: SysLogCatType.Integration,
                act: SysLogActionType.Exec,
                ex: new HttpRequestException($"Amadeus error {response.StatusCode}: {errorBody}"),
                message: "Amadeus API call failed",
                ent: "Amadeus",
                stat: (int)response.StatusCode,
                note: "provider:Amadeus");
            throw new InvalidOperationException($"Error '{response.StatusCode}' Response: {errorBody}");
        }
    }
    
    public async Task<AmadeusFlightOfferSearchResult> GetFlightOffersFromTravelQuoteAsync(TravelQuote quote, CancellationToken ct = default)
    {
        await _loggerService.InformationAsync(
            evt: "FLIGHT_OFFERS_REQ_START",
            cat: SysLogCatType.Api,  // we RECEIVED a request (not calling Amadeus yet)
            act: SysLogActionType.Start,
            message: $"Received flight offers request (quoteId={nameof(TravelQuote)}, id={quote.Id})",
            ent: nameof(TravelQuote),
            entId: quote.Id,
            note: "ingress:start");

        // the travel quote has everything we need to call Amadeus api directly
        // var flightOfferSearch = new AmadeusFlightOfferSearch
        // {
        //     CurrencyCode = quote.
        //     OriginDestinations = quote.OriginDestinations,
        //     Travelers = quote.Travelers,
        //     SearchCriteria = quote.SearchCriteria
        // };



        return new AmadeusFlightOfferSearchResult
        {
            Meta = new Meta { Count = 0 }
        };
    }
}
