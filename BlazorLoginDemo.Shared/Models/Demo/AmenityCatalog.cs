// File: Amenities/AmenitySchema.cs
using System.Text.RegularExpressions;

namespace BlazorLoginDemo.Shared.Models.Demo;

public enum AmenityGroup
{
    Baggage, Seating, AirportPriority, Lounge, Catering, Connectivity, Entertainment, Comfort,
    Flexibility, Loyalty, FamilyAssist, SpecialServices, Other
}

public enum AmenityCode
{
    // Baggage
    PersonalItemIncluded, CabinBagIncluded, CabinBinPriority, CheckedBagIncluded,
    ExtraCheckedBagIncluded, PriorityBaggageHandling, InterlineThroughCheck,
    SpecialItemSportsIncluded, SpecialItemInstrumentIncluded, InfantBaggageIncluded,
    OversizeOverweightWaived,

    // Seating
    SeatSelectionIncluded, StandardSeatIncluded, PreferredSeatIncluded, ExtraLegroomSeatIncluded,
    ExitRowSeatIncluded, BassinetAvailable, SeatTypeRecliner, SeatTypeAngleFlat, SeatTypeLieFlat,
    SeatPitch32Plus, PowerAC, PowerUsbA, PowerUsbC,

    // Airport priority & lounge
    PriorityCheckIn, PrioritySecurity, PriorityBoarding,
    LoungeAccessIncluded, DedicatedBagDrop, ChauffeurTransferIncluded, StopoverHotelIncluded,

    // Catering
    MealIncluded, HotMealIncluded, SnackIncluded, SoftDrinksIncluded, AlcoholIncluded,
    SpecialMealsAvailable,

    // Connectivity & entertainment
    SeatbackIFE, StreamingIFE, LiveTV, WiFiIncluded, FreeMessagingIncluded, HeadsetsIncluded, DigitalPressIncluded,

    // Comfort
    PillowIncluded, BlanketIncluded, AmenityKitIncluded, MattressPadTurnDown, LavatoryToiletries,

    // Flexibility
    Refundable, Changeable, SameDayChangeIncluded, StandbyIncluded,

    // Loyalty
    MilesAccrual, StatusCreditsAccrual, UpgradeEligible, UpgradeWithMilesAllowed,

    // Family & assistance
    FamilySeating, FamilyPreboarding, InfantOnLapAllowed, UMNRServiceAvailable,

    // Special Services (SSR)
    WheelchairAssist, ServiceAnimalAccepted, MedicalOxygenAvailable, OtherSSRSupport
}

public sealed record Amenity(AmenityCode Code, AmenityGroup Group, string Label, string? Tooltip = null);

public static class AmenityCatalog
{
    // Minimal curated set for UI labels. Extend as needed.
    public static readonly IReadOnlyList<Amenity> All = new[]
    {
        new Amenity(AmenityCode.CabinBagIncluded, AmenityGroup.Baggage, "Cabin bag included"),
        new Amenity(AmenityCode.CheckedBagIncluded, AmenityGroup.Baggage, "Checked bag included"),
        new Amenity(AmenityCode.ExtraCheckedBagIncluded, AmenityGroup.Baggage, "Extra checked bag"),
        new Amenity(AmenityCode.PriorityBaggageHandling, AmenityGroup.Baggage, "Priority baggage"),
        new Amenity(AmenityCode.SeatSelectionIncluded, AmenityGroup.Seating, "Seat selection"),
        new Amenity(AmenityCode.PreferredSeatIncluded, AmenityGroup.Seating, "Preferred seat"),
        new Amenity(AmenityCode.ExtraLegroomSeatIncluded, AmenityGroup.Seating, "Extra legroom seat"),
        new Amenity(AmenityCode.SeatTypeLieFlat, AmenityGroup.Seating, "Lie-flat seat"),
        new Amenity(AmenityCode.PriorityCheckIn, AmenityGroup.AirportPriority, "Priority check-in"),
        new Amenity(AmenityCode.PrioritySecurity, AmenityGroup.AirportPriority, "Fast Track security"),
        new Amenity(AmenityCode.PriorityBoarding, AmenityGroup.AirportPriority, "Priority boarding"),
        new Amenity(AmenityCode.LoungeAccessIncluded, AmenityGroup.Lounge, "Lounge access"),
        new Amenity(AmenityCode.MealIncluded, AmenityGroup.Catering, "Meal included"),
        new Amenity(AmenityCode.HotMealIncluded, AmenityGroup.Catering, "Hot meal"),
        new Amenity(AmenityCode.AlcoholIncluded, AmenityGroup.Catering, "Alcoholic drinks"),
        new Amenity(AmenityCode.SpecialMealsAvailable, AmenityGroup.Catering, "Special meals"),
        new Amenity(AmenityCode.WiFiIncluded, AmenityGroup.Connectivity, "Wi-Fi included"),
        new Amenity(AmenityCode.FreeMessagingIncluded, AmenityGroup.Connectivity, "Free messaging"),
        new Amenity(AmenityCode.SeatbackIFE, AmenityGroup.Entertainment, "Seatback entertainment"),
        new Amenity(AmenityCode.StreamingIFE, AmenityGroup.Entertainment, "Streaming entertainment"),
        new Amenity(AmenityCode.Refundable, AmenityGroup.Flexibility, "Refundable ticket"),
        new Amenity(AmenityCode.Changeable, AmenityGroup.Flexibility, "Changeable ticket"),
        new Amenity(AmenityCode.SameDayChangeIncluded, AmenityGroup.Flexibility, "Same-day change"),
        new Amenity(AmenityCode.MilesAccrual, AmenityGroup.Loyalty, "Miles earn"),
        new Amenity(AmenityCode.StatusCreditsAccrual, AmenityGroup.Loyalty, "Status credits"),
        new Amenity(AmenityCode.FamilySeating, AmenityGroup.FamilyAssist, "Family seating"),
        new Amenity(AmenityCode.UMNRServiceAvailable, AmenityGroup.FamilyAssist, "Unaccompanied minor"),
        new Amenity(AmenityCode.WheelchairAssist, AmenityGroup.SpecialServices, "Wheelchair assistance"),
    };

    // Direct term → code. Case-insensitive. Extend per carrier marketing names.
    public static readonly IReadOnlyDictionary<string, AmenityCode> Synonyms =
        new Dictionary<string, AmenityCode>(StringComparer.OrdinalIgnoreCase)
    {
        // Priority
        ["fast track"] = AmenityCode.PrioritySecurity,
        ["priority security"] = AmenityCode.PrioritySecurity,
        ["priority boarding"] = AmenityCode.PriorityBoarding,
        ["priority check-in"] = AmenityCode.PriorityCheckIn,
        ["skypriority"] = AmenityCode.PriorityBoarding,
        ["premier access"] = AmenityCode.PrioritySecurity,

        // Lounge
        ["lounge access"] = AmenityCode.LoungeAccessIncluded,
        ["club access"] = AmenityCode.LoungeAccessIncluded,
        ["qantas club"] = AmenityCode.LoungeAccessIncluded,

        // Baggage
        ["carry on included"] = AmenityCode.CabinBagIncluded,
        ["carry-on included"] = AmenityCode.CabinBagIncluded,
        ["cabin baggage"] = AmenityCode.CabinBagIncluded,
        ["first checked bag free"] = AmenityCode.CheckedBagIncluded,
        ["checked bag included"] = AmenityCode.CheckedBagIncluded,
        ["priority baggage"] = AmenityCode.PriorityBaggageHandling,

        // Seating
        ["seat selection"] = AmenityCode.SeatSelectionIncluded,
        ["preferred seat"] = AmenityCode.PreferredSeatIncluded,
        ["extra legroom"] = AmenityCode.ExtraLegroomSeatIncluded,
        ["economy comfort"] = AmenityCode.ExtraLegroomSeatIncluded,
        ["main cabin extra"] = AmenityCode.ExtraLegroomSeatIncluded,
        ["exit row"] = AmenityCode.ExitRowSeatIncluded,
        ["lie-flat"] = AmenityCode.SeatTypeLieFlat,
        ["flat bed"] = AmenityCode.SeatTypeLieFlat,

        // Catering
        ["meal"] = AmenityCode.MealIncluded,
        ["hot meal"] = AmenityCode.HotMealIncluded,
        ["alcohol"] = AmenityCode.AlcoholIncluded,
        ["special meal"] = AmenityCode.SpecialMealsAvailable,

        // Connectivity & entertainment
        ["wifi"] = AmenityCode.WiFiIncluded,
        ["wi-fi"] = AmenityCode.WiFiIncluded,
        ["free messaging"] = AmenityCode.FreeMessagingIncluded,
        ["seatback entertainment"] = AmenityCode.SeatbackIFE,
        ["streaming entertainment"] = AmenityCode.StreamingIFE,
        ["live tv"] = AmenityCode.LiveTV,

        // Flex
        ["refundable"] = AmenityCode.Refundable,
        ["flexible"] = AmenityCode.Changeable,
        ["same-day change"] = AmenityCode.SameDayChangeIncluded,

        // Loyalty
        ["miles"] = AmenityCode.MilesAccrual,
        ["status credits"] = AmenityCode.StatusCreditsAccrual,

        // Family & SSR
        ["family seating"] = AmenityCode.FamilySeating,
        ["unaccompanied minor"] = AmenityCode.UMNRServiceAvailable,
        ["wheelchair"] = AmenityCode.WheelchairAssist
    };

    // Regex fallbacks catch marketing variants. Keep fast and simple.
    static readonly (Regex rx, AmenityCode code)[] Heuristics =
    {
        (new Regex(@"\bcarry[\s-]?on\b|\bcabin\s+bag", RegexOptions.IgnoreCase), AmenityCode.CabinBagIncluded),
        (new Regex(@"\bchecked\s+bag|\bfirst\s+bag\s+free", RegexOptions.IgnoreCase), AmenityCode.CheckedBagIncluded),
        (new Regex(@"\bpriority\s+baggage", RegexOptions.IgnoreCase), AmenityCode.PriorityBaggageHandling),
        (new Regex(@"\bextra\s+legroom|\bmore\s+legroom", RegexOptions.IgnoreCase), AmenityCode.ExtraLegroomSeatIncluded),
        (new Regex(@"\bexit\s+row", RegexOptions.IgnoreCase), AmenityCode.ExitRowSeatIncluded),
        (new Regex(@"\blie[-\s]?flat|\bflat\s+bed", RegexOptions.IgnoreCase), AmenityCode.SeatTypeLieFlat),
        (new Regex(@"\blounge\b|\bclub\s+access", RegexOptions.IgnoreCase), AmenityCode.LoungeAccessIncluded),
        (new Regex(@"\bmeal\b", RegexOptions.IgnoreCase), AmenityCode.MealIncluded),
        (new Regex(@"\bhot\s+meal", RegexOptions.IgnoreCase), AmenityCode.HotMealIncluded),
        (new Regex(@"\balcohol(ic)?\b", RegexOptions.IgnoreCase), AmenityCode.AlcoholIncluded),
        (new Regex(@"\bspecial\s+meal", RegexOptions.IgnoreCase), AmenityCode.SpecialMealsAvailable),
        (new Regex(@"\bwi-?fi\b|\bwlan\b", RegexOptions.IgnoreCase), AmenityCode.WiFiIncluded),
        (new Regex(@"\bseat(back)?\s+(ife|screen|entertainment)", RegexOptions.IgnoreCase), AmenityCode.SeatbackIFE),
        (new Regex(@"\brefundable\b", RegexOptions.IgnoreCase), AmenityCode.Refundable),
        (new Regex(@"\bchange(able|s)?\b|\bflex(ible)?\b", RegexOptions.IgnoreCase), AmenityCode.Changeable),
        (new Regex(@"\bsame[-\s]?day\s+change", RegexOptions.IgnoreCase), AmenityCode.SameDayChangeIncluded),
        (new Regex(@"\bmiles?\b|\bavios\b|\bpoints\b", RegexOptions.IgnoreCase), AmenityCode.MilesAccrual),
        (new Regex(@"\bstatus\s+credit", RegexOptions.IgnoreCase), AmenityCode.StatusCreditsAccrual),
        (new Regex(@"\bfamily\s+seating", RegexOptions.IgnoreCase), AmenityCode.FamilySeating),
        (new Regex(@"\bumnr|\bunaccompanied\s+minor", RegexOptions.IgnoreCase), AmenityCode.UMNRServiceAvailable),
        (new Regex(@"\bwheelchair\b", RegexOptions.IgnoreCase), AmenityCode.WheelchairAssist),
    };

    public static IReadOnlySet<AmenityCode> NormalizeFromStrings(IEnumerable<string> rawTerms)
    {
        var set = new HashSet<AmenityCode>();
        foreach (var term in rawTerms)
        {
            if (string.IsNullOrWhiteSpace(term)) continue;

            // 1) Exact synonym hits
            if (Synonyms.TryGetValue(term.Trim(), out var mapped))
            {
                set.Add(mapped);
                continue;
            }

            // 2) Heuristic pattern hits
            foreach (var (rx, code) in Heuristics)
            {
                if (rx.IsMatch(term))
                {
                    set.Add(code);
                }
            }
        }
        return set;
    }

    public static IReadOnlyDictionary<AmenityGroup, Amenity[]> Group(IReadOnlyCollection<AmenityCode> codes)
    {
        var dict = new Dictionary<AmenityGroup, List<Amenity>>();
        foreach (var c in codes)
        {
            var meta = All.FirstOrDefault(a => a.Code == c);
            var group = meta?.Group ?? AmenityGroup.Other;
            if (!dict.TryGetValue(group, out var list)) dict[group] = list = new();
            list.Add(meta ?? new Amenity(c, AmenityGroup.Other, c.ToString()));
        }
        return dict.ToDictionary(kv => kv.Key, kv => kv.Value.OrderBy(a => a.Label).ToArray());
    }
}
