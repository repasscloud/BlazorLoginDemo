// Services/Interfaces/Travel/ITravelQuoteService.cs
using Cinturon360.Shared.Models.DTOs;
using Cinturon360.Shared.Models.ExternalLib.Amadeus;
using Cinturon360.Shared.Models.ExternalLib.Amadeus.Flight;
using Cinturon360.Shared.Models.Kernel.Travel;
using Cinturon360.Shared.Models.Search;

namespace Cinturon360.Shared.Services.Interfaces.Travel;

public interface ITravelQuoteService
{
    // CREATE
    Task<TravelQuote> CreateAsync(TravelQuote model, CancellationToken ct = default);
    Task<(bool Ok, string? Error, string? TravelQuoteId)> CreateFromDtoAsync(TravelQuoteDto dto, CancellationToken ct = default);

    // READ (NO TRACKING ALWAYS)
    Task<TravelQuote?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<TravelQuote>> SearchAsync(
        string? organizationId = null,
        string? createdByUserId = null,
        string? tmcAssignedId = null,
        TravelQuoteType? type = null,
        QuoteState? state = null,
        CancellationToken ct = default);

    // UPDATE (PUT semantics: overwrite whole aggregate, including Travellers)
    Task<TravelQuote> UpdatePutAsync(TravelQuote model, CancellationToken ct = default);

    // POINT UPDATERS
    Task<bool> ReassignCreatedByAsync(string travelQuoteId, string newUserId, CancellationToken ct = default);
    Task<bool> UpdateStateAsync(string travelQuoteId, QuoteState newState, CancellationToken ct = default);
    Task IngestTravelQuoteFlightUIResultPatchDto(TravelQuoteFlightUIResultPatchDto dto, CancellationToken ct = default);

    // DELETE
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);

    // HELPERS
    bool TryParseQuoteType(string value, out TravelQuoteType type);
    Task<int> ExpireOldQuotesAsync(CancellationToken ct = default);
    Task<List<string>?> GetExcludedAirlinesFromPolicyAsync(string travelPolicyId, TravelQuotePolicyType policyType, CancellationToken ct = default);
    Task<AmadeusFlightOfferSearch> BuildAmadeusFlightOfferSearchFromQuote(TravelQuote quote, bool returnTrip, CancellationToken ct = default);

    // UI HELPERS
    Task<FlightSearchPageConfig> GenerateFlightSearchUIOptionsAsync(string travelQuoteId, CancellationToken ct = default);
    Task<FlightSearchResponse> GetFlightSearchResultsAsync(string travelQuoteId, AmadeusFlightOfferSearchResult results, CancellationToken ct = default);
}
