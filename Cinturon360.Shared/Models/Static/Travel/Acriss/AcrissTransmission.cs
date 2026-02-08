namespace Cinturon360.Shared.Models.Static.Travel.Acriss;

public enum AcrissTransmission : int
{
    AUTOMATIC,
    AUTOMATIC_4WD,
    AUTOMATIC_AWD,
    MANUAL,
    MANUAL_4WD,
    MANUAL_AWD
}

public static class AcrissTransmissions
{
    public sealed record Transmission(
        AcrissTransmission Type,
        char Code,
        string CodeUpper,
        string CodeLower,
        string Label
    );

    private static readonly IReadOnlyList<Transmission> _all =
    [
        new(AcrissTransmission.AUTOMATIC, 'A', "AUTOMATIC", "automatic", "Automatic"),
        new(AcrissTransmission.AUTOMATIC_4WD, 'B', "AUTOMATIC_4WD", "automatic_4wd", "Automatic 4WD"),
        new(AcrissTransmission.AUTOMATIC_AWD, 'D', "AUTOMATIC_AWD", "automatic_awd", "Automatic AWD"),

        new(AcrissTransmission.MANUAL, 'M', "MANUAL", "manual", "Manual"),
        new(AcrissTransmission.MANUAL_4WD, 'N', "MANUAL_4WD", "manual_4wd", "Manual 4WD"),
        new(AcrissTransmission.MANUAL_AWD, 'C', "MANUAL_AWD", "manual_awd", "Manual AWD")
    ];

    // --------------------
    // Public accessors
    // --------------------

    public static IReadOnlyList<Transmission> All => _all;

    public static IReadOnlyList<(char Code, string Label)> DropdownOptions =>
        _all.Select(t => (t.Code, t.Label)).ToList();

    public static Transmission Get(AcrissTransmission type) =>
        _all.First(t => t.Type == type);

    public static Transmission? GetByCode(char code) =>
        _all.FirstOrDefault(t => t.Code == code);

    public static AcrissTransmission? ToEnum(char code) =>
        GetByCode(code)?.Type;
}
