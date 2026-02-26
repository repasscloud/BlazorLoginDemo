namespace Cinturon360.Shared.Models.Static.Support;

/// <summary>
/// Static reference data for SLA tiers.
/// Record model is the source of truth.
/// Enum exists for legacy compatibility only.
/// </summary>
public static partial class SlaTiers
{
    // ------------------------------------------------------------------------
    // LEGACY ENUM (do NOT remove — used by existing aggregates, DB mappings)
    // ------------------------------------------------------------------------

    public enum SlaTier : int
    {
        None = 0,
        Basic = 10,
        Standard = 20,
        Premium = 30,
        Enterprise = 40,
        Custom = 100
    }

    // ------------------------------------------------------------------------
    // NEW RECORD MODEL (source of truth)
    // ------------------------------------------------------------------------

    public sealed record SlaTierInfo(
        int Id,
        string Code,
        string CodeLower,
        string Name,
        string Description,
        bool IsAssignable,
        bool IsCustom
    )
    {
        public SlaTier Enum => (SlaTier)Id;
    }

    // ------------------------------------------------------------------------
    // SINGLE SOURCE OF TRUTH
    // ------------------------------------------------------------------------

    private static readonly IReadOnlyList<SlaTierInfo> _all =
    [
        new(
            Id: 0,
            Code: "NONE",
            CodeLower: "none",
            Name: "None",
            Description: "No service level agreement",
            IsAssignable: true,
            IsCustom: false
        ),

        new(
            Id: 10,
            Code: "BASIC",
            CodeLower: "basic",
            Name: "Basic",
            Description: "Basic support during business hours",
            IsAssignable: true,
            IsCustom: false
        ),

        new(
            Id: 20,
            Code: "STANDARD",
            CodeLower: "standard",
            Name: "Standard",
            Description: "Standard support with defined response times",
            IsAssignable: true,
            IsCustom: false
        ),

        new(
            Id: 30,
            Code: "PREMIUM",
            CodeLower: "premium",
            Name: "Premium",
            Description: "Priority support with faster response times",
            IsAssignable: true,
            IsCustom: false
        ),

        new(
            Id: 40,
            Code: "ENTERPRISE",
            CodeLower: "enterprise",
            Name: "Enterprise",
            Description: "24/7 enterprise-grade support",
            IsAssignable: true,
            IsCustom: false
        ),

        new(
            Id: 100,
            Code: "CUSTOM",
            CodeLower: "custom",
            Name: "Custom",
            Description: "Custom negotiated SLA",
            IsAssignable: false,
            IsCustom: true
        )
    ];

    public static IReadOnlyList<SlaTierInfo> All => _all;

    // ------------------------------------------------------------------------
    // LOOKUPS
    // ------------------------------------------------------------------------

    public static SlaTierInfo Get(int id)
        => _all.First(x => x.Id == id);

    public static SlaTierInfo Get(SlaTier tier)
        => Get((int)tier);

    public static SlaTierInfo? TryGet(int id)
        => _all.FirstOrDefault(x => x.Id == id);

    public static SlaTier ToEnum(int id)
        => (SlaTier)id;

    public static int ToId(SlaTier tier)
        => (int)tier;

    // ------------------------------------------------------------------------
    // STRONGLY TYPED ACCESSORS
    // ------------------------------------------------------------------------

    public static SlaTierInfo None       => Get(SlaTier.None);
    public static SlaTierInfo Basic      => Get(SlaTier.Basic);
    public static SlaTierInfo Standard   => Get(SlaTier.Standard);
    public static SlaTierInfo Premium    => Get(SlaTier.Premium);
    public static SlaTierInfo Enterprise => Get(SlaTier.Enterprise);
    public static SlaTierInfo Custom     => Get(SlaTier.Custom);

    // ------------------------------------------------------------------------
    // UI PROJECTIONS
    // ------------------------------------------------------------------------

    public static IReadOnlyList<(SlaTier Tier, string Name)> DropdownOptions =>
        _all
            .Where(x => x.IsAssignable)
            .Select(x => ((SlaTier)x.Id, x.Name))
            .ToList();

    public static IReadOnlyList<(SlaTier Tier, string Name)> AssignableOptions =>
        _all
            .Where(x => x.IsAssignable)
            .Select(x => ((SlaTier)x.Id, x.Name))
            .ToList();

    public static IReadOnlyList<(SlaTier Tier, string Name)> CustomOptions =>
        _all
            .Where(x => x.IsCustom)
            .Select(x => ((SlaTier)x.Id, x.Name))
            .ToList();
}