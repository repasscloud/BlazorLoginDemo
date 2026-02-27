using System.Globalization;

namespace Cinturon360.Shared.Models.Static.Identity;

public static class CulturesType
{
    // IETF BCP 47 tags (a.k.a. CultureInfo.Name)
    public static readonly (string Tag, string Label)[] Bcp47 =
    [
        ("en-AU","🇦🇺 English"),
        ("en-GB","🇬🇧 English"),
        ("es-ES","🇪🇸 Español"),
        ("it-IT","🇮🇹 Italiano"),
        ("fr-FR","🇫🇷 Français"),
    ];

    // Handy extras if you ever need the ISO parts:
    public static readonly string[] Iso639Languages =
        Array.ConvertAll(Bcp47, x => CultureInfo.GetCultureInfo(x.Tag).TwoLetterISOLanguageName);
}