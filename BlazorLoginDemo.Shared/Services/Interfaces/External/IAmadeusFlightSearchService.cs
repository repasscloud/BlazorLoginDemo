using BlazorLoginDemo.Shared.Models.DTOs;
using BlazorLoginDemo.Shared.Models.ExternalLib.Amadeus.Flight;

namespace BlazorLoginDemo.Shared.Services.Interfaces.External;

public interface IAmadeusFlightSearchService
{
    Task<AmadeusFlightOfferSearchResult> GetFlightOffersAsync(FlightOfferSearchRequestDto dto);
}