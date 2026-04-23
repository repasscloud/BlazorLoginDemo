namespace Cinturon360.Shared.Models.Static.Travel;

public enum AirlineAlliance : int
{
    // -------------------------
    // No / special classification
    // -------------------------

    None = 0,                 // Unknown / not specified
    Independent = 9,          // Operates outside any alliance

    // -------------------------
    // Global airline alliances
    // -------------------------

    OneWorld = 1,             // oneworld (Qantas, BA, AA, CX, JL)
    StarAlliance = 2,         // Star Alliance (LH, UA, SQ, NH)
    SkyTeam = 3,              // SkyTeam (AF/KL, DL, KE)

    // -------------------------
    // Regional / commercial alliances
    // -------------------------

    ValueAlliance = 10,       // Value Alliance (Scoot, Cebu Pacific, NokScoot – mostly LCC, Asia)
    VanillaAlliance = 11,     // Vanilla Alliance (Air Austral, Air Mauritius – Indian Ocean)
    UFlyAlliance = 12,        // U-FLY Alliance (China Southern-led regional grouping)

    // -------------------------
    // Soft / marketing alliances
    // -------------------------

    JointVenture = 20,        // Metal-neutral JV (e.g. AA/BA/IB, DL/AF/KL)
    CodeshareOnly = 21,       // Codeshare partnerships only (no alliance membership)

    // -------------------------
    // Special / edge cases
    // -------------------------

    Charter = 90,             // Charter-only operators
    Government = 91           // State / military / special-purpose carriers
}

public static class AirlineAlliances
{
    public sealed record Alliance(
        AirlineAlliance Type, // enum-safe
        string Code,          // "ONEWORLD"
        string CodeLower,     // "oneworld"
        string Label,         // "oneworld"
        string Description    // UI help / tooltip
    );

    // Single source of truth
    private static readonly IReadOnlyList<Alliance> _all =
    [
        new(
            AirlineAlliance.None,
            "NONE",
            "none",
            "None",
            "No alliance specified or unknown."
        ),
        new(
            AirlineAlliance.OneWorld,
            "ONEWORLD",
            "oneworld",
            "oneworld",
            "Global alliance including Qantas, British Airways, American Airlines."
        ),
        new(
            AirlineAlliance.StarAlliance,
            "STAR_ALLIANCE",
            "star_alliance",
            "Star Alliance",
            "Global alliance including Lufthansa, Singapore Airlines, United Airlines."
        ),
        new(
            AirlineAlliance.SkyTeam,
            "SKYTEAM",
            "skyteam",
            "SkyTeam",
            "Global alliance including Air France–KLM, Delta, Korean Air."
        ),
        new(
            AirlineAlliance.Independent,
            "INDEPENDENT",
            "independent",
            "Independent",
            "Airlines not belonging to any global alliance."
        ),
        new(
            AirlineAlliance.ValueAlliance,
            "VALUE_ALLIANCE",
            "value_alliance",
            "Value Alliance",
            "Asia-Pacific low-cost carrier alliance."
        ),
        new(
            AirlineAlliance.VanillaAlliance,
            "VANILLA_ALLIANCE",
            "vanilla_alliance",
            "Vanilla Alliance",
            "Indian Ocean regional airline alliance."
        ),
        new(
            AirlineAlliance.UFlyAlliance,
            "UFLY_ALLIANCE",
            "ufly_alliance",
            "U-FLY Alliance",
            "China-led regional airline grouping."
        ),
        new(
            AirlineAlliance.JointVenture,
            "JOINT_VENTURE",
            "joint_venture",
            "Joint Venture",
            "Metal-neutral joint venture partnership."
        ),
        new(
            AirlineAlliance.CodeshareOnly,
            "CODESHARE_ONLY",
            "codeshare_only",
            "Codeshare Only",
            "Codeshare partnerships without alliance membership."
        ),
        new(
            AirlineAlliance.Charter,
            "CHARTER",
            "charter",
            "Charter",
            "Charter-only or ad-hoc operators."
        ),
        new(
            AirlineAlliance.Government,
            "GOVERNMENT",
            "government",
            "Government",
            "State, military, or special-purpose carriers."
        )
    ];

    public static IReadOnlyList<Alliance> All => _all;

    // UI projection (dropdown-safe)
    public static IReadOnlyList<(string Code, string Label)> DropdownOptions =>
        _all.Select(a => (a.Code, a.Label)).ToList();

    // Enum-safe lookup
    public static Alliance Get(AirlineAlliance type) =>
        _all.First(a => a.Type == type);

    // String-safe lookup (DB / API)
    public static Alliance? GetByCode(string code) =>
        _all.FirstOrDefault(a =>
            a.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

    // Helpers
    public static string ToCode(AirlineAlliance type) =>
        Get(type).Code;

    public static AirlineAlliance? ToEnum(string code) =>
        GetByCode(code)?.Type;
}
