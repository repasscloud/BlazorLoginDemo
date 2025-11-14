using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace Cinturon360.Tui.App.Theming;

/// <summary>
/// Central place for colours. Change here to reskin the app.
/// </summary>
public static class AppTheme
{
    public static ColorScheme WindowScheme { get; private set; } = null!;
    public static ColorScheme MenuScheme { get; private set; } = null!;
    public static ColorScheme DialogScheme { get; private set; } = null!;
    public static ColorScheme StatusScheme { get; private set; } = null!;

    static AppTheme()
    {
        // Default "corporate dark" theme
        SetDarkTheme();
    }

    public static void SetDarkTheme()
    {
        WindowScheme = new ColorScheme
        {
            Normal = new Attribute(Color.Gray, Color.Black),
            Focus = new Attribute(Color.BrightCyan, Color.Black),
            HotNormal = new Attribute(Color.BrightYellow, Color.Black),
            HotFocus = new Attribute(Color.BrightYellow, Color.Black)
        };

        MenuScheme = new ColorScheme
        {
            Normal = new Attribute(Color.Black, Color.Gray),
            Focus = new Attribute(Color.Black, Color.BrightCyan),
            HotNormal = new Attribute(Color.Black, Color.BrightYellow),
            HotFocus = new Attribute(Color.Black, Color.BrightYellow)
        };

        DialogScheme = new ColorScheme
        {
            Normal = new Attribute(Color.White, Color.Blue),
            Focus = new Attribute(Color.BrightCyan, Color.Blue),
            HotNormal = new Attribute(Color.BrightYellow, Color.Blue),
            HotFocus = new Attribute(Color.BrightYellow, Color.Blue)
        };

        StatusScheme = new ColorScheme
        {
            Normal = new Attribute(Color.Black, Color.Gray),
            Focus = new Attribute(Color.Black, Color.BrightCyan)
        };
    }

    // Later you can add SetLightTheme() etc.
}
