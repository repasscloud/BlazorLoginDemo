using Cinturon360.Tui.App.Theming;
using Terminal.Gui;

namespace Cinturon360.Tui.App.Shell;

/// <summary>
/// Base page implementation with a plain content View (no inner frame).
/// </summary>
public abstract class PageBase : IPage
{
    public abstract string Title { get; }

    public View View => _root;

    protected readonly View _root;

    protected PageBase()
    {
        _root = new View
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ColorScheme = AppTheme.WindowScheme
        };
    }

    public virtual void OnActivated()
    {
        // no-op for now; outer shell owns the visible title
    }

    public virtual void OnDeactivated()
    {
        // no-op by default
    }
}
