namespace Cinturon360.Shared.Models.Static.Travel.Acriss;

public enum AcrissFuel : int
{
    UNSPEC_AIR,
    UNSPEC_NO_AIR,
    DIESEL_AIR,
    DIESEL_NO_AIR,
    HYBRID_AIR,
    HYBRID_PLUGIN_AIR,
    ELECTRIC,
    LPG_AIR,
    LPG_NO_AIR,
    HYDROGEN_AIR,
    HYDROGEN_NO_AIR,
    FLEX_FUEL_AIR,
    FLEX_FUEL_NO_AIR,
    PETROL_AIR,
    PETROL_NO_AIR,
    PETROL,
    DIESEL,
    HYBRID,
    LPG,
    HYDROGEN,
    FLEX_FUEL
}

public static class AcrissFuels
{
    public sealed record Fuel(
        AcrissFuel Type,
        char Code,
        string CodeUpper,
        string CodeLower,
        string Label
    );

    private static readonly IReadOnlyList<Fuel> _all =
    [
        new(AcrissFuel.UNSPEC_AIR, 'R', "UNSPEC_AIR", "unspec_air", "Unspecified Power (Air)"),
        new(AcrissFuel.UNSPEC_NO_AIR, 'U', "UNSPEC_NO_AIR", "unspec_no_air", "Unspecified Power (No Air)"),

        new(AcrissFuel.DIESEL_AIR, 'D', "DIESEL_AIR", "diesel_air", "Diesel (Air)"),
        new(AcrissFuel.DIESEL_NO_AIR, 'Q', "DIESEL_NO_AIR", "diesel_no_air", "Diesel (No Air)"),

        new(AcrissFuel.HYBRID_AIR, 'H', "HYBRID_AIR", "hybrid_air", "Hybrid (Air)"),
        new(AcrissFuel.HYBRID_PLUGIN_AIR, 'I', "HYBRID_PLUGIN_AIR", "hybrid_plugin_air", "Hybrid Plug-in (Air)"),

        new(AcrissFuel.ELECTRIC, 'E', "ELECTRIC", "electric", "Electric"),

        new(AcrissFuel.LPG_AIR, 'L', "LPG_AIR", "lpg_air", "LPG (Air)"),
        new(AcrissFuel.LPG_NO_AIR, 'S', "LPG_NO_AIR", "lpg_no_air", "LPG (No Air)"),

        new(AcrissFuel.HYDROGEN_AIR, 'A', "HYDROGEN_AIR", "hydrogen_air", "Hydrogen (Air)"),
        new(AcrissFuel.HYDROGEN_NO_AIR, 'B', "HYDROGEN_NO_AIR", "hydrogen_no_air", "Hydrogen (No Air)"),

        new(AcrissFuel.FLEX_FUEL_AIR, 'M', "FLEX_FUEL_AIR", "flex_fuel_air", "Flex Fuel (Air)"),
        new(AcrissFuel.FLEX_FUEL_NO_AIR, 'F', "FLEX_FUEL_NO_AIR", "flex_fuel_no_air", "Flex Fuel (No Air)"),

        new(AcrissFuel.PETROL_AIR, 'V', "PETROL_AIR", "petrol_air", "Petrol (Air)"),
        new(AcrissFuel.PETROL_NO_AIR, 'Z', "PETROL_NO_AIR", "petrol_no_air", "Petrol (No Air)"),

        new(AcrissFuel.PETROL, 'N', "PETROL", "petrol", "Petrol / Gasoline"),
        new(AcrissFuel.DIESEL, 'D', "DIESEL", "diesel", "Diesel"),
        new(AcrissFuel.HYBRID, 'H', "HYBRID", "hybrid", "Hybrid"),
        new(AcrissFuel.LPG, 'L', "LPG", "lpg", "LPG"),
        new(AcrissFuel.HYDROGEN, 'A', "HYDROGEN", "hydrogen", "Hydrogen"),
        new(AcrissFuel.FLEX_FUEL, 'C', "FLEX_FUEL", "flex_fuel", "Flex Fuel")
    ];

    // --------------------
    // Public accessors
    // --------------------

    public static IReadOnlyList<Fuel> All => _all;

    public static IReadOnlyList<(char Code, string Label)> DropdownOptions =>
        _all.Select(f => (f.Code, f.Label)).ToList();

    public static Fuel Get(AcrissFuel type) =>
        _all.First(f => f.Type == type);

    public static Fuel? GetByCode(char code) =>
        _all.FirstOrDefault(f => f.Code == code);

    public static AcrissFuel? ToEnum(char code) =>
        GetByCode(code)?.Type;
}
