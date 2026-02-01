namespace Cinturon360.Tui.App.UI.Dialogs;

public interface IBusyDialogService
{
    Task RunWithBusyOverlayAsync(
        string message,
        Func<CancellationToken, Task> operation,
        CancellationToken parentToken = default);
}
