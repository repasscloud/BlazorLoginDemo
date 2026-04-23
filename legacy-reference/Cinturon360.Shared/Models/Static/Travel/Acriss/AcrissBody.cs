namespace Cinturon360.Shared.Models.Static.Travel.Acriss;

public enum AcrissBody : int
{
    DOOR_2_3,
    DOOR_2_4,
    DOOR_4_5,
    WAGON_ESTATE,
    PASSENGER_VAN,
    LIMOUSINE_SEDAN,
    SPORT,
    CONVERTIBLE,
    SUV,
    OPEN_AIR_ALL_TERRAIN,
    PICKUP_REGULAR,
    PICKUP_EXTENDED,
    SPECIAL,
    SPECIAL_OFFER,
    COUPE,
    MPV,
    RV,
    MOTOR_HOME,
    TWO_WHEEL,
    ROADSTER,
    CROSSOVER,
    COMMERCIAL
}

public static class AcrissBodies
{
    public sealed record Body(
        AcrissBody Type,
        char Code,
        string CodeUpper,
        string CodeLower,
        string Label
    );

    private static readonly IReadOnlyList<Body> _all =
    [
        new(AcrissBody.DOOR_2_3, 'B', "DOOR_2_3", "2_3_door", "2–3 Door"),
        new(AcrissBody.DOOR_2_4, 'C', "DOOR_2_4", "2_4_door", "2/4 Door"),
        new(AcrissBody.DOOR_4_5, 'D', "DOOR_4_5", "4_5_door", "4–5 Door"),

        new(AcrissBody.WAGON_ESTATE, 'W', "WAGON_ESTATE", "wagon_estate", "Wagon / Estate"),
        new(AcrissBody.PASSENGER_VAN, 'V', "PASSENGER_VAN", "passenger_van", "Passenger Van"),
        new(AcrissBody.LIMOUSINE_SEDAN, 'L', "LIMOUSINE_SEDAN", "limousine_sedan", "Limousine / Sedan"),

        new(AcrissBody.SPORT, 'S', "SPORT", "sport", "Sport"),
        new(AcrissBody.CONVERTIBLE, 'T', "CONVERTIBLE", "convertible", "Convertible"),
        new(AcrissBody.SUV, 'F', "SUV", "suv", "SUV"),

        new(AcrissBody.OPEN_AIR_ALL_TERRAIN, 'J', "OPEN_AIR_ALL_TERRAIN", "open_air_all_terrain", "Open Air All Terrain"),
        new(AcrissBody.SPECIAL, 'X', "SPECIAL", "special", "Special"),

        new(AcrissBody.PICKUP_REGULAR, 'P', "PICKUP_REGULAR", "pickup_regular", "Pickup – Regular Cab"),
        new(AcrissBody.PICKUP_EXTENDED, 'Q', "PICKUP_EXTENDED", "pickup_extended", "Pickup – Extended Cab"),

        new(AcrissBody.SPECIAL_OFFER, 'Z', "SPECIAL_OFFER", "special_offer", "Special Offer"),
        new(AcrissBody.COUPE, 'E', "COUPE", "coupe", "Coupe"),

        new(AcrissBody.MPV, 'M', "MPV", "mpv", "MPV / Monospace"),
        new(AcrissBody.RV, 'R', "RV", "rv", "Recreational Vehicle"),
        new(AcrissBody.MOTOR_HOME, 'H', "MOTOR_HOME", "motor_home", "Motor Home"),

        new(AcrissBody.TWO_WHEEL, 'Y', "TWO_WHEEL", "two_wheel", "2-Wheel Vehicle"),
        new(AcrissBody.ROADSTER, 'N', "ROADSTER", "roadster", "Roadster"),
        new(AcrissBody.CROSSOVER, 'G', "CROSSOVER", "crossover", "Crossover"),
        new(AcrissBody.COMMERCIAL, 'K', "COMMERCIAL", "commercial", "Commercial Van / Truck")
    ];

    // --------------------
    // Public accessors
    // --------------------

    public static IReadOnlyList<Body> All => _all;

    public static IReadOnlyList<(char Code, string Label)> DropdownOptions =>
        _all.Select(b => (b.Code, b.Label)).ToList();

    public static Body Get(AcrissBody type) =>
        _all.First(b => b.Type == type);

    public static Body? GetByCode(char code) =>
        _all.FirstOrDefault(b => b.Code == code);

    public static AcrissBody? ToEnum(char code) =>
        GetByCode(code)?.Type;
}

