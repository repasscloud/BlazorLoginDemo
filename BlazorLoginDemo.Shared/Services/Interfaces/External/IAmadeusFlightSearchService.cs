using BlazorLoginDemo.Shared.Models.DTOs;
using BlazorLoginDemo.Shared.Models.ExternalLib.Amadeus.Flight;
using BlazorLoginDemo.Shared.Models.Kernel.Travel;

namespace BlazorLoginDemo.Shared.Services.Interfaces.External;

public interface IAmadeusFlightSearchService
{
    Task<AmadeusFlightOfferSearchResult> GetFlightOffersAsync(FlightOfferSearchRequestDto dto, CancellationToken ct = default);

    Task<AmadeusFlightOfferSearchResult> GetFlightOffersFromTravelQuoteAsync(TravelQuote quote, CancellationToken ct = default);
}