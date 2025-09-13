using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BlazorLoginDemo.Shared.Models.DTOs;
using BlazorLoginDemo.Shared.Models.ExternalLib.Amadeus;
using BlazorLoginDemo.Shared.Models.ExternalLib.Amadeus.Flight;
using BlazorLoginDemo.Shared.Models.Kernel.User;
using BlazorLoginDemo.Shared.Services.Interfaces.External;
using BlazorLoginDemo.Shared.Services.Interfaces.Kernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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

    public async Task<AmadeusFlightOfferSearchResult> GetFlightOffersAsync(FlightOfferSearchRequestDto dto)
    {
        await _loggerService.LogInfoAsync($"FlightOfferService received request from API with ID: {dto.Id}");

        // find the travel policy (if it exists) for the user account, and load it into memory
        TravelPolicyBookingContextDto? _travelPolicyInterResult = null;
        if (!string.IsNullOrEmpty(dto.TravelPolicyId))
        {
            //_travelPolicyInterResult = await _avaApiService.GetTravelPolicyInterResultByIdAsync(dto.TravelPolicyId, "")!;
            _travelPolicyInterResult = await _db.TravelPolicies
                .Where(c => c.Id == dto.TravelPolicyId)
                .Select(tp => new TravelPolicyBookingContextDto
                {
                    Id = tp.Id,
                    PolicyName = tp.PolicyName,
                    AvaClientId = tp.AvaClientId,
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
                .FirstOrDefaultAsync();
        }

        // find the user preference service now too (everyone has one of these*)
        // * - no they don't!
        // also, we don't use the .AspNetUsersId, we use the AvaUserId
        // (which is called customerId in this particular record, not sure why?)
        AvaUserSysPreference? _userSysPreferences = await _db.AvaUserSysPreferences
            .Where(x => x.AvaUserId == dto.CustomerId)
            .FirstOrDefaultAsync();
        
        // set the currency code
        string _currencyCode = _travelPolicyInterResult?.Currency ?? _userSysPreferences?.DefaultCurrencyCode ?? "AUD";

        // list of flights to be added to search to amadeus (and other providers)
        List<OriginDestination> _originDestinationList = new List<OriginDestination>();
        
        // first flight
        OriginDestination _originDestination1 = new OriginDestination
        {
            Id = "1",
            OriginLocationCode = dto.OriginLocationCode,
            DestinationLocationCode = dto.DestinationLocationCode,
            DateTimeRange = new DepartureDateTimeRange 
            {
                Date = dto.DepartureDate,
                Time = _travelPolicyInterResult?.FlightBookingTimeAvailableFrom 
                    ?? _userSysPreferences?.FlightBookingTimeAvailableFrom,
            },
        };
        _originDestinationList.Add(_originDestination1);

        // does second flight exist?
        if (!dto.IsOneWay)
        {
            if (dto.DepartureDateReturn is not null)
            {
                OriginDestination _originDestination2 = new OriginDestination
                {
                    Id = "2",
                    OriginLocationCode = dto.DestinationLocationCode,
                    DestinationLocationCode = dto.OriginLocationCode,
                    DateTimeRange = new DepartureDateTimeRange
                    {
                        Date = dto.DepartureDateReturn,
                        Time = _travelPolicyInterResult?.FlightBookingTimeAvailableFrom 
                            ?? _userSysPreferences?.FlightBookingTimeAvailableFrom,
                    }
                };
                _originDestinationList.Add(_originDestination2);
            }
        }

        // list of travellers
        List<Traveler> _travelersList = new List<Traveler>();
        
        // create travellers
        foreach (var i in Enumerable.Range(1, dto.Adults))
        {
            Traveler _traveler = new Traveler
            {
                Id = i.ToString(),
                TravelerType = "ADULT",
                FareOptions = [ "STANDARD" ]
            };

            _travelersList.Add(_traveler);
        }

        // create SearchCriteria
        SearchCriteria _searchCriteria = new SearchCriteria();

        // add the max flight offers value (if not null) and set the value else ignore, it will default to 10 results
        // nothing needs to be added, it gets added in this region
        if (_userSysPreferences?.MaxResults == 0 || _userSysPreferences?.MaxResults == 250)
        {
            _searchCriteria.MaxFlightOffers = 250;
        }
        if (_userSysPreferences?.MaxResults > 0 || _userSysPreferences?.MaxResults < 250)
        {
            _searchCriteria.MaxFlightOffers = 20;
        }

        // create the default FlightFilters object (it's got null initialization, so it's OK)
        FlightFilters _filters = new FlightFilters();
        // flight filters is made up of List<CabinRestriction>? and CarrierRestriction?, so
        // those will be created next, starting with CabinRestrictions

        // create the .CabinRestrictions object (it will be appended to .Filters at the end of this region)
        List<CabinRestriction> _cabinRestrictions = new List<CabinRestriction>();

        // when adding to .CabinRestrictions, we need to know if there's a return
        // trip or oneway, so check if the return date exists, then create the
        // required item as an object, add it to a temp list, and add that list
        // to the searchCriteria object too
        if (dto.DepartureDateReturn is not null)
        {
            CabinRestriction _cabinRestrictionAllFlights = new CabinRestriction
            {
                Cabin = _travelPolicyInterResult?.DefaultFlightSeating ?? dto.CabinClass,
                Coverage = _travelPolicyInterResult?.CabinClassCoverage ?? _userSysPreferences?.CabinClassCoverage ?? "MOST_SEGMENTS",
                OriginDestinationIds = [ "1", "2" ]
            };

            _cabinRestrictions.Add(_cabinRestrictionAllFlights);
        }
        else
        {
            CabinRestriction _cabinRestrictionAllFlights = new CabinRestriction
            {
                Cabin = _travelPolicyInterResult?.DefaultFlightSeating ?? dto.CabinClass,
                Coverage = _travelPolicyInterResult?.CabinClassCoverage ?? _userSysPreferences?.CabinClassCoverage ?? "MOST_SEGMENTS",
                OriginDestinationIds = [ "1" ]
            };

            _cabinRestrictions.Add(_cabinRestrictionAllFlights);
        }

        _filters.CabinRestrictions = _cabinRestrictions;

        // create the .CarrrierRestriction object (this gets added at the end of the region)
        CarrierRestriction _carrierRestriction = new CarrierRestriction();

        // do not edit the .BlacklistedInEUAllowed value (bool), it's set in the class

        // carrier excluded codes excludes, but carrier included restricts, so included is counted first then skip
        // else use excluded second
        if (_travelPolicyInterResult?.IncludedAirlineCodes is not null)
        {
            _carrierRestriction.IncludedCarrierCodes = _travelPolicyInterResult.IncludedAirlineCodes;
        }
        else if (_travelPolicyInterResult?.ExcludedAirlineCodes is not null)
        {
            _carrierRestriction.ExcludedCarrierCodes = _travelPolicyInterResult.ExcludedAirlineCodes;
        }
        else if (_userSysPreferences?.IncludedAirlineCodes is not null)
        {
            _carrierRestriction.IncludedCarrierCodes = _userSysPreferences.IncludedAirlineCodes.ToList();
        }
        else if (_userSysPreferences?.ExcludedAirlineCodes is not null)
        {
            _carrierRestriction.ExcludedCarrierCodes = _userSysPreferences.ExcludedAirlineCodes.ToList();
        }

        _filters.CarrierRestrictions = _carrierRestriction;
        
        _searchCriteria.Filters = _filters;

        // create the flight offer search (Amadeus)
        AmadeusFlightOfferSearch flightOfferSearch = new()
        {
            CurrencyCode = _currencyCode,
            OriginDestinations = _originDestinationList,
            Travelers = _travelersList ,
            SearchCriteria = _searchCriteria
        };

        //-->START DEBUG
        // this whole section is just debug code
        string debugJsonX = $@"/app/searchRequestDTO_debugJsonX_{dto.Id}.json";
        await _loggerService.LogInfoAsync($"Saving results of {dto.Id} to {debugJsonX}");
        var xJson = JsonSerializer.Serialize(flightOfferSearch, _jsonOptions);
        await File.WriteAllTextAsync(debugJsonX, xJson);
        //-->END DEBUG

        // get the token information
        var token = await _authService.GetTokenInformationAsync();
        if (string.IsNullOrEmpty(token))
        {
            await _loggerService.LogErrorAsync("Unable to retrieve valid OAuth token.");
            throw new Exception("Unable to retrieve valid OAuth token.");
        }

        // create instance of HTTP client (from the factory)
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // get URL from appsettings to use
        string _flightOfferUrl = _settings.Url.FlightOffer
            ?? throw new ArgumentNullException("Amadeus:Url:FlightOffer is missing in configuration.");

        var response = await httpClient.PostAsJsonAsync(_flightOfferUrl, flightOfferSearch, _jsonOptions);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AmadeusFlightOfferSearchResult>();
            return result ?? throw new InvalidOperationException("Deserialization returned null.");
        }

        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
            string errorBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error Response: {errorBody}");
            throw new InvalidOperationException($"Error '{response.StatusCode}' Response: {errorBody}");
        }
    }

    // private static List<string> SplitCommaSeparatedString(string input)
    // {
    //     if (string.IsNullOrWhiteSpace(input))
    //     {
    //         return new List<string>();
    //     }

    //     return input.Split(',', StringSplitOptions.RemoveEmptyEntries)
    //         .Select(s => s.Trim())
    //         .ToList();
    // }
}
