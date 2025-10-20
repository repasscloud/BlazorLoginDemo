namespace BlazorLoginDemo.Shared.Models.Search;

public sealed class FlightSearchPageConfig
{
    public string TenantName { get; set; } = "-";
    public string PolicyName { get; set; } = "-";
    public string TravelQuoteId { get; set; } = "-";
    public string TmcAssignedId { get; set; } = "-";

    public List<BookingAirport> EnabledOrigins { get; init; } = new List<BookingAirport>();
    public List<BookingAirport> EnabledDestinations { get; init; } = new List<BookingAirport>();

    public List<BookingAirline> AvailableAirlines { get; init; } = new List<BookingAirline>();
    public List<string> PreferredAirlines { get; init; } = new List<string>();

    public CabinClass DefaultCabin { get; init; } = CabinClass.Economy;
    public CabinClass MaxCabin { get; init; } = CabinClass.First;

    public int? DaysInAdvanceBookingRequired { get; init; }

    public bool HasFixedTimes { get; init; }
    public string FixedDepartEarliest { get; init; } = string.Empty;
    public string FixedDepartLatest { get; init; } = string.Empty;
    public string FixedReturnEarliest { get; init; } = string.Empty;
    public string FixedReturnLatest { get; init; } = string.Empty;

    public DateTime? SeedDepartDate { get; init; }
    public DateTime? SeedReturnDate { get; init; }

    public int Adults { get; set; } = 1;
    public int Children { get; set; } = 0;
    public int Infants { get; set; } = 0;
}

public enum CabinClass { Economy = 0, PremiumEconomy = 1, Business = 2, First = 3 }
public enum Alliance { Oneworld = 1, StarAlliance = 2, SkyTeam = 3 }
public enum TripType { OneWay = 1, Return = 2 }
public enum FlightSortOption { PriceAsc = 1, PriceDesc = 2, DurationAsc = 3, DurationDesc = 4, DepartureAsc = 5, DepartureDesc = 6, ArrivalAsc = 7, ArrivalDesc = 8 }
public enum CoverageType { MostSegments = 1, AtLeastOneSegment = 2, AllSegments = 3 }