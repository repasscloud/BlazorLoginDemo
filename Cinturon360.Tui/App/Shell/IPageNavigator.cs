using Terminal.Gui;

namespace Cinturon360.Tui.App.Shell;

public interface IPageNavigator
{
    void ShowPage(IPage page, string title);
}
