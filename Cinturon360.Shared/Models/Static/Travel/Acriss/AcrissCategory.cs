namespace Cinturon360.Shared.Models.Static.Travel.Acriss;

public enum AcrissCategory : int
{
    MINI,
    MINI_ELITE,
    ECONOMY,
    ECONOMY_ELITE,
    COMPACT,
    COMPACT_ELITE,
    INTERMEDIATE,
    INTERMEDIATE_ELITE,
    STANDARD,
    STANDARD_ELITE,
    FULL_SIZE,
    FULL_SIZE_ELITE,
    PREMIUM,
    PREMIUM_ELITE,
    LUXURY,
    LUXURY_ELITE,
    OVERSIZE,
    SPECIAL
}

public static class AcrissCategories
{
    public sealed record Category(
        AcrissCategory Type,
        char Code,
        string CodeUpper,
        string CodeLower,
        string Label
    );

    private static readonly IReadOnlyList<Category> _all =
    [
        new(AcrissCategory.MINI, 'M', "MINI", "mini", "Mini"),
        new(AcrissCategory.MINI_ELITE, 'M', "MINI_ELITE", "mini_elite", "Mini Elite"),

        new(AcrissCategory.ECONOMY, 'E', "ECONOMY", "economy", "Economy"),
        new(AcrissCategory.ECONOMY_ELITE, 'H', "ECONOMY_ELITE", "economy_elite", "Economy Elite"),

        new(AcrissCategory.COMPACT, 'C', "COMPACT", "compact", "Compact"),
        new(AcrissCategory.COMPACT_ELITE, 'D', "COMPACT_ELITE", "compact_elite", "Compact Elite"),

        new(AcrissCategory.INTERMEDIATE, 'I', "INTERMEDIATE", "intermediate", "Intermediate"),
        new(AcrissCategory.INTERMEDIATE_ELITE, 'J', "INTERMEDIATE_ELITE", "intermediate_elite", "Intermediate Elite"),

        new(AcrissCategory.STANDARD, 'S', "STANDARD", "standard", "Standard"),
        new(AcrissCategory.STANDARD_ELITE, 'R', "STANDARD_ELITE", "standard_elite", "Standard Elite"),

        new(AcrissCategory.FULL_SIZE, 'F', "FULL_SIZE", "full_size", "Full Size"),
        new(AcrissCategory.FULL_SIZE_ELITE, 'G', "FULL_SIZE_ELITE", "full_size_elite", "Full Size Elite"),

        new(AcrissCategory.PREMIUM, 'P', "PREMIUM", "premium", "Premium"),
        new(AcrissCategory.PREMIUM_ELITE, 'U', "PREMIUM_ELITE", "premium_elite", "Premium Elite"),

        new(AcrissCategory.LUXURY, 'L', "LUXURY", "luxury", "Luxury"),
        new(AcrissCategory.LUXURY_ELITE, 'W', "LUXURY_ELITE", "luxury_elite", "Luxury Elite"),

        new(AcrissCategory.OVERSIZE, 'O', "OVERSIZE", "oversize", "Oversize"),
        new(AcrissCategory.SPECIAL, 'X', "SPECIAL", "special", "Special")
    ];

    // --------------------
    // Public accessors
    // --------------------

    public static IReadOnlyList<Category> All => _all;

    public static IReadOnlyList<(char Code, string Label)> DropdownOptions =>
        _all.Select(c => (c.Code, c.Label)).ToList();

    public static Category Get(AcrissCategory type) =>
        _all.First(c => c.Type == type);

    public static Category? GetByCode(char code) =>
        _all.FirstOrDefault(c => c.Code == code);

    public static AcrissCategory? ToEnum(char code) =>
        GetByCode(code)?.Type;
}

