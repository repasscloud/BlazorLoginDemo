using Cinturon360.Shared.Contracts.Policies;
using Cinturon360.Shared.Models.Static.Travel;
using Cinturon360.Shared.Models.Static.Travel.Acriss;

namespace Cinturon360.Web.Drafts.Platform.Org.Policies.Travel;

internal static class EditOrgTravelPolicyMapper
{
    public static EditOrgTravelPolicyDraft ToDraft(
        UpdateTravelPolicyRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return new EditOrgTravelPolicyDraft
        {
            TravelPolicyId = request.PolicyId,
            PolicyName = request.PolicyName,
            OrgId = request.OrganizationId,
            DefaultCurrencyCode = request.DefaultCurrencyCode,
            EffectiveFromUtc = request.EffectiveFromUtc,
            EffectiveToUtc = request.EffectiveToUtc,
            MaxFlightPrice = request.MaxFlightPrice,
            DefaultFlightSeating = FareType.ToEnum(request.DefaultFlightSeating),
            MaxFlightSeating = FareType.ToEnum(request.MaxFlightSeating),
            CabinClassCoverage = CabinClassCoverages.ToEnum(request.CabinClassCoverage),
            NonStopFlight = request.NonStopFlight,
            IncludedAirlineCodes = request.IncludedAirlineCodes.ToList(),
            ExcludedAirlineCodes = request.ExcludedAirlineCodes.ToList(),
            FlightBookingTimeAvailableFrom = request.FlightBookingTimeAvailableFrom,
            FlightBookingTimeAvailableTo = request.FlightBookingTimeAvailableTo,
            EnableSaturdayFlightBookings = request.EnableSaturdayFlightBookings,
            EnableSundayFlightBookings = request.EnableSundayFlightBookings,
            DefaultCalendarDaysInAdvanceForFlightBooking = request.DefaultCalendarDaysInAdvanceForFlightBooking,
            MaxHotelNightlyRate = request.MaxHotelNightlyRate,
            DefaultHotelRoomType = HotelRoomType.ToEnum(request.DefaultHotelRoomType),
            MaxHotelRoomType = HotelRoomType.ToEnum(request.MaxHotelRoomType),
            IncludedHotelChains = request.IncludedHotelChains.ToArray(),
            ExcludedHotelChains = request.ExcludedHotelChains.ToArray(),
            HotelBookingTimeAvailableFrom = request.HotelBookingTimeAvailableFrom,
            HotelBookingTimeAvailableTo = request.HotelBookingTimeAvailableTo,
            EnableSaturdayHotelBookings = request.EnableSaturdayHotelBookings,
            EnableSundayHotelBookings = request.EnableSundayHotelBookings,
            MaxTaxiFarePerRide = request.MaxTaxiFarePerRide,
            MaxTaxiSurgeMultiplier = request.MaxTaxiSurgeMultiplier,
            IncludedTaxiVendors = request.IncludedTaxiVendors.ToList(),
            ExcludedTaxiVendors = request.ExcludedTaxiVendors.ToList(),
            DefaultTrainClass = RailFareType.ToEnum(request.DefaultTrainClass),
            MaxTrainClass = RailFareType.ToEnum(request.MaxTrainClass),
            MaxTrainPrice = request.MaxTrainPrice,
            IncludedRailOperators = request.IncludedRailOperators.ToList(),
            ExcludedRailOperators = request.ExcludedRailOperators.ToList(),
            MaxCarDailyRate = request.MaxCarDailyRate,
            DefaultCarCategory = request.DefaultCarClass[0],
            DefaultCarBody = request.DefaultCarClass[1],
            DefaultCarTransmission = request.DefaultCarClass[2],
            DefaultCarFuel = request.DefaultCarClass[3],
            MaxCarCategory = request.MaxCarClass[0],
            MaxCarBody = request.MaxCarClass[1],
            MaxCarTransmission = request.MaxCarClass[2],
            MaxCarFuel = request.MaxCarClass[3],
            RequireInclusiveInsurance = request.RequireInclusiveInsurance,
            IncludedCarHireVendors = request.IncludedCarHireVendors.ToArray(),
            ExcludedCarHireVendors = request.ExcludedCarHireVendors.ToArray(),
            MaxBusFarePerTicket = request.MaxBusFarePerTicket,
            IncludedBusOperators = request.IncludedBusOperators,
            ExcludedBusOperators = request.ExcludedBusOperators,
            MaxSimCardPrice = request.MaxSimBundlePrice,
            MinSimDataAllowanceGb = request.MinSimDataGb,
            MinSimValidityDays = request.MinSimValidityDays,
            EnableVoiceSimCards = request.EnableVoiceSimCards,
            EnableDataSimCards = request.EnableDataSimCards,
            EnableSimHotspotTethering = request.EnableSimHotspotTethering,
            EnablePortableWifiRental = request.EnablePortableWifiRental,
            IncludedSimVendors = request.IncludedSimVendors,
            ExcludedSimVendors = request.ExcludedSimVendors,
            MaxActivityPricePerBooking = request.MaxActivityPricePerBooking,
            AllowHighRiskActivities = request.AllowHighRiskActivities,
            IncludedActivityProviders = request.IncludedActivityProviders,
            ExcludedActivityProviders = request.ExcludedActivityProviders,
            RegionIds = request.RegionIds,
            ContinentIds = request.ContinentIds,
            CountryIds = request.CountryIds,
            DisabledCountryIds = request.DisabledCountryIds,
            MaxFlightSeatingAt6Hours = FareType.ToEnum(request.MaxFlightSeatingAt6Hours),
            MaxFlightSeatingAt8Hours = FareType.ToEnum(request.MaxFlightSeatingAt8Hours),
            MaxFlightSeatingAt10Hours = FareType.ToEnum(request.MaxFlightSeatingAt10Hours),
            MaxFlightSeatingAt14Hours = FareType.ToEnum(request.MaxFlightSeatingAt14Hours),
            MaxFlightPriceAt6Hours = request.MaxFlightPriceAt6Hours,
            MaxFlightPriceAt8Hours = request.MaxFlightPriceAt8Hours,
            MaxFlightPriceAt10Hours = request.MaxFlightPriceAt10Hours,
            MaxFlightPriceAt14Hours = request.MaxFlightPriceAt14Hours,
            AutoApproveToPolicyLimit = request.AutoApproveToPolicyLimit,
            RequireManagerApprovalToPolicyLimit = request.RequireManagerApprovalToPolicyLimit,
            L1ApprovalRequired = request.L1ApprovalRequired,
            L1ApprovalAmount = request.L1ApprovalAmount,
            L2ApprovalRequired = request.L2ApprovalRequired,
            L2ApprovalAmount = request.L2ApprovalAmount,
            L3ApprovalRequired = request.L3ApprovalRequired,
            L3ApprovalAmount = request.L3ApprovalAmount,
            OrgBillingContactApprovalToPolicyLimit = request.OrgBillingContactApprovalToPolicyLimit,
            OrgBillingContactApprovalAbovePolicyLimit = request.OrgBillingContactApprovalAbovePolicyLimit,
        };
    }

    // =========================================================================
    // ACRISS code helpers
    // =========================================================================
    // ACRISS code is a 4-character string made up of:
    // 1st char: Category
    // 2nd char: Body
    // 3rd char: Transmission
    // 4th char: Fuel
    private static string BuildAcrissClass(
        char category,
        char body,
        char transmission,
        char fuel)
    {
        return string.Concat(
            ValidateCategory(category),
            ValidateBody(body),
            ValidateTransmission(transmission),
            ValidateFuel(fuel)
        );
    }

    private static char ValidateCategory(char code) =>
        AcrissCategories.GetByCode(code)?.Code
            ?? throw new InvalidOperationException(
                $"Invalid ACRISS category code: '{code}'");

    private static char ValidateBody(char code) =>
        AcrissBodies.GetByCode(code)?.Code
            ?? throw new InvalidOperationException(
                $"Invalid ACRISS body code: '{code}'");

    private static char ValidateTransmission(char code) =>
        AcrissTransmissions.GetByCode(code)?.Code
            ?? throw new InvalidOperationException(
                $"Invalid ACRISS transmission code: '{code}'");

    private static char ValidateFuel(char code) =>
        AcrissFuels.GetByCode(code)?.Code
            ?? throw new InvalidOperationException(
                $"Invalid ACRISS fuel code: '{code}'");

    private static string[] BuildAcrissClassRange(
        string defaultClass,
        string maxClass)
    {
        if (defaultClass.Length != 4 || maxClass.Length != 4)
            throw new InvalidOperationException("ACRISS class must be exactly 4 characters.");

        var catRange  = SliceRange(
            AcrissCategories.All.Select(x => x.Code),
            defaultClass[0],
            maxClass[0]);

        var bodyRange = SliceRange(
            AcrissBodies.All.Select(x => x.Code),
            defaultClass[1],
            maxClass[1]);

        var transRange = SliceRange(
            AcrissTransmissions.All.Select(x => x.Code),
            defaultClass[2],
            maxClass[2]);

        var fuelRange = SliceRange(
            AcrissFuels.All.Select(x => x.Code),
            defaultClass[3],
            maxClass[3]);

        var results = new List<string>();

        foreach (var c in catRange)
        foreach (var b in bodyRange)
        foreach (var t in transRange)
        foreach (var f in fuelRange)
            results.Add(string.Concat(c, b, t, f));

        return results.ToArray();
    }

    private static IReadOnlyList<char> SliceRange(
        IEnumerable<char> orderedValues,
        char start,
        char end)
    {
        var list = orderedValues.ToList();

        var startIndex = list.IndexOf(start);
        var endIndex   = list.IndexOf(end);

        if (startIndex < 0 || endIndex < 0)
            throw new InvalidOperationException(
                $"Invalid ACRISS range '{start}' → '{end}'");

        if (startIndex > endIndex)
            throw new InvalidOperationException(
                $"ACRISS range is reversed: '{start}' → '{end}'");

        return list
            .Skip(startIndex)
            .Take(endIndex - startIndex + 1)
            .ToList();
    }
}
