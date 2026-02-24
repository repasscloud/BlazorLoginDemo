using Cinturon360.Shared.Contracts.Policies;
using Cinturon360.Shared.Models.Static.Travel;
using Cinturon360.Shared.Models.Static.Travel.Acriss;

namespace Cinturon360.Web.Drafts.Platform.Orgs.Policies.Travel;

internal static class NewOrgTravelPolicyMapper
{
    public static CreateTravelPolicyRequest ToRequest(
        NewOrgTravelPolicyDraft draft)
    {
        if (draft is null)
        {
            throw new ArgumentNullException(nameof(draft));
        }

        var defaultCarClass = BuildAcrissClass(
            draft.DefaultCarCategory,
            draft.DefaultCarBody,
            draft.DefaultCarTransmission,
            draft.DefaultCarFuel
        );

        var maxCarClass = BuildAcrissClass(
            draft.MaxCarCategory,
            draft.MaxCarBody,
            draft.MaxCarTransmission,
            draft.MaxCarFuel
        );

        var allowedCarHireClasses =
            BuildAcrissClassRange(defaultCarClass, maxCarClass);

        return new CreateTravelPolicyRequest
        {
            PolicyName = draft.PolicyName,
            OrganizationId = draft.OrgId,
            DefaultCurrencyCode = draft.DefaultCurrencyCode,
            EffectiveFromUtc = draft.EffectiveFromUtc,
            EffectiveToUtc = draft.EffectiveToUtc,
            MaxFlightPrice = draft.MaxFlightPrice,
            DefaultFlightSeating = FareType.ToCode(draft.DefaultFlightSeating),
            MaxFlightSeating = FareType.ToCode(draft.MaxFlightSeating),
            CabinClassCoverage = CabinClassCoverages.ToCode(draft.CabinClassCoverage),
            NonStopFlight = draft.NonStopFlight,
            IncludedAirlineCodes = draft.IncludedAirlineCodes.ToArray(),
            ExcludedAirlineCodes = draft.ExcludedAirlineCodes.ToArray(),
            FlightBookingTimeAvailableFrom = draft.FlightBookingTimeAvailableFrom,
            FlightBookingTimeAvailableTo = draft.FlightBookingTimeAvailableTo,
            EnableSaturdayFlightBookings = draft.EnableSaturdayFlightBookings,
            EnableSundayFlightBookings = draft.EnableSundayFlightBookings,
            DefaultCalendarDaysInAdvanceForFlightBooking = draft.DefaultCalendarDaysInAdvanceForFlightBooking,
            MaxHotelNightlyRate = draft.MaxHotelNightlyRate,
            DefaultHotelRoomType = HotelRoomType.ToCode(draft.DefaultHotelRoomType),
            MaxHotelRoomType = HotelRoomType.ToCode(draft.MaxHotelRoomType),
            IncludedHotelChains = draft.IncludedHotelChains.ToArray(),
            ExcludedHotelChains = draft.ExcludedHotelChains.ToArray(),
            HotelBookingTimeAvailableFrom = draft.HotelBookingTimeAvailableFrom,
            HotelBookingTimeAvailableTo = draft.HotelBookingTimeAvailableTo,
            EnableSaturdayHotelBookings = draft.EnableSaturdayHotelBookings,
            EnableSundayHotelBookings = draft.EnableSundayHotelBookings,
            MaxTaxiFarePerRide = draft.MaxTaxiFarePerRide,
            MaxTaxiSurgeMultiplier = draft.MaxTaxiSurgeMultiplier,
            IncludedTaxiVendors = draft.IncludedTaxiVendors.ToArray(),
            ExcludedTaxiVendors = draft.ExcludedTaxiVendors.ToArray(),
            DefaultTrainClass = RailFareType.ToCode(draft.DefaultTrainClass),
            MaxTrainClass = RailFareType.ToCode(draft.MaxTrainClass),
            MaxTrainPrice = draft.MaxTrainPrice,
            IncludedRailOperators = draft.IncludedRailOperators.ToArray(),
            ExcludedRailOperators = draft.ExcludedRailOperators.ToArray(),
            MaxCarDailyRate = draft.MaxCarDailyRate,
            DefaultCarClass = defaultCarClass,
            MaxCarClass = maxCarClass,
            RequireInclusiveInsurance = draft.RequireInclusiveInsurance,
            AllowedCarHireClasses = allowedCarHireClasses,
            IncludedCarHireVendors = draft.IncludedCarHireVendors,
            ExcludedCarHireVendors = draft.ExcludedCarHireVendors,
            MaxBusFarePerTicket = draft.MaxBusFarePerTicket,
            IncludedBusOperators = draft.IncludedBusOperators,
            ExcludedBusOperators = draft.ExcludedBusOperators,
            MaxSimBundlePrice = draft.MaxSimCardPrice,
            MinSimDataGb = draft.MinSimDataAllowanceGb,
            MinSimValidityDays = draft.MinSimValidityDays,
            EnableVoiceSimCards = draft.EnableVoiceSimCards,
            EnableDataSimCards = draft.EnableDataSimCards,
            EnableSimHotspotTethering = draft.EnableSimHotspotTethering,
            EnablePortableWifiRental = draft.EnablePortableWifiRental,
            IncludedSimVendors = draft.IncludedSimVendors,
            ExcludedSimVendors = draft.ExcludedSimVendors,
            MaxActivityPricePerBooking = draft.MaxActivityPricePerBooking,
            AllowHighRiskActivities = draft.AllowHighRiskActivities,
            IncludedActivityProviders = draft.IncludedActivityProviders,
            ExcludedActivityProviders = draft.ExcludedActivityProviders,
            RegionIds = draft.RegionIds,
            ContinentIds = draft.ContinentIds,
            CountryIds = draft.CountryIds,
            DisabledCountryIds = draft.DisabledCountryIds,
            MaxFlightSeatingAt6Hours = FareType.ToCode(draft.MaxFlightSeatingAt6Hours),
            MaxFlightSeatingAt8Hours = FareType.ToCode(draft.MaxFlightSeatingAt8Hours),
            MaxFlightSeatingAt10Hours = FareType.ToCode(draft.MaxFlightSeatingAt10Hours),
            MaxFlightSeatingAt14Hours = FareType.ToCode(draft.MaxFlightSeatingAt14Hours),
            MaxFlightPriceAt6Hours = draft.MaxFlightPriceAt6Hours,
            MaxFlightPriceAt8Hours = draft.MaxFlightPriceAt8Hours,
            MaxFlightPriceAt10Hours = draft.MaxFlightPriceAt10Hours,
            MaxFlightPriceAt14Hours = draft.MaxFlightPriceAt14Hours,
            AutoApproveToPolicyLimit = draft.AutoApproveToPolicyLimit,
            RequireManagerApprovalToPolicyLimit = draft.RequireManagerApprovalToPolicyLimit,
            L1ApprovalRequired = draft.L1ApprovalRequired,
            L1ApprovalAmount = draft.L1ApprovalAmount,
            L2ApprovalRequired = draft.L2ApprovalRequired,
            L2ApprovalAmount = draft.L2ApprovalAmount,
            L3ApprovalRequired = draft.L3ApprovalRequired,
            L3ApprovalAmount = draft.L3ApprovalAmount,
            OrgBillingContactApprovalToPolicyLimit = draft.OrgBillingContactApprovalToPolicyLimit,
            OrgBillingContactApprovalAbovePolicyLimit = draft.OrgBillingContactApprovalAbovePolicyLimit,
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
