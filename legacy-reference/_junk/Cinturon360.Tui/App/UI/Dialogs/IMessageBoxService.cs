namespace Cinturon360.Tui.App.UI.Dialogs;

public interface IMessageBoxService
{
    void Info(string title, string message);

    void Error(string title, string message);
}
