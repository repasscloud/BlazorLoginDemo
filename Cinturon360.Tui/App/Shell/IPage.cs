using Terminal.Gui;

namespace Cinturon360.Tui.App.Shell;

public interface IPage
{
    string Title { get; }

    /// <summary>Main view for the page.</summary>
    View View { get; }

    /// <summary>Called when page becomes active.</summary>
    void OnActivated();

    /// <summary>Called when page is hidden.</summary>
    void OnDeactivated();
}
