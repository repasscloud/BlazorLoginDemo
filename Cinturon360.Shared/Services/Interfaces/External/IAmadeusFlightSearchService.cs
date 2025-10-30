using Cinturon360.Shared.Models.DTOs;
using Cinturon360.Shared.Models.ExternalLib.Amadeus;
using Cinturon360.Shared.Models.ExternalLib.Amadeus.Flight;

namespace Cinturon360.Shared.Services.Interfaces.External;

public interface IAmadeusFlightSearchService
{
    Task<AmadeusFlightOfferSearchResult> GetFlightOffersAsync(FlightOfferSearchRequestDto dto, CancellationToken ct = default);

    Task<AmadeusFlightOfferSearchResult> GetFlightOffersFromAmadeusFlightOfferSearch(AmadeusFlightOfferSearch criteria, CancellationToken ct = default);
}