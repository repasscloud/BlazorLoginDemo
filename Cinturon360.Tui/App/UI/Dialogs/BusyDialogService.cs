using Cinturon360.Tui.App.Theming;
using Terminal.Gui;

namespace Cinturon360.Tui.App.UI.Dialogs;

/// <summary>
/// Shows a modal "please wait" dialog while an async operation runs.
/// Prevents repeated button mashing.
/// </summary>
public sealed class BusyDialogService : IBusyDialogService
{
    public async Task RunWithBusyOverlayAsync(
        string message,
        Func<CancellationToken, Task> operation,
        CancellationToken parentToken = default)
    {
        // 1. REMOVE the `using` so CTS is not disposed while the dialog is still alive
        var cts = CancellationTokenSource.CreateLinkedTokenSource(parentToken);

        Dialog? busyDialog = null;

        Application.MainLoop.Invoke(() =>
        {
            busyDialog = BuildBusyDialog(message, cts);
            Application.Run(busyDialog);
        });

        Exception? captured = null;

        try
        {
            await operation(cts.Token);
        }
        catch (Exception ex)
        {
            captured = ex;
        }
        finally
        {
            // Close the dialog when the operation finishes
            Application.MainLoop.Invoke(() =>
            {
                if (busyDialog is not null)
                    Application.RequestStop(busyDialog);
            });

            // optional: if you really want to dispose,
            // you can do it here AFTER the dialog is closed and no more clicks can happen.
            // cts.Dispose();
        }

        if (captured != null)
            throw captured;
    }

    private static Dialog BuildBusyDialog(string message, CancellationTokenSource cts)
    {
        var dialog = new Dialog("Please wait...", width: 50, height: 8)
        {
            ColorScheme = AppTheme.DialogScheme
        };

        var lbl = new Label(message)
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(2),
            Height = 1
        };

        var cancelBtn = new Button("Cancel")
        {
            X = Pos.Center(),
            Y = 3
        };

        cancelBtn.Clicked += () =>
        {
            // 2. Cancel the token
            if (!cts.IsCancellationRequested)
                cts.Cancel();

            // 3. Close the dialog immediately
            Application.RequestStop(dialog);
        };

        dialog.Add(lbl, cancelBtn);

        return dialog;
    }
}
