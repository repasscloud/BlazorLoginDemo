using Cinturon360.Tui.App.Theming;
using Terminal.Gui;

namespace Cinturon360.Tui.App.UI.Dialogs;

public sealed class MessageBoxService : IMessageBoxService
{
    public void Info(string title, string message)
    {
        Show(title, message);
    }

    public void Error(string title, string message)
    {
        Show(title, message);
    }

    private static void Show(string title, string message)
    {
        var dialog = new Dialog(title, width: 60, height: 10)
        {
            ColorScheme = AppTheme.DialogScheme
        };

        var lbl = new Label(message)
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(2),
            Height = Dim.Fill(2)
        };

        var ok = new Button("OK")
        {
            IsDefault = true,
            X = Pos.Center(),
            Y = Pos.Bottom(lbl) + 1
        };
        ok.Clicked += () => Application.RequestStop();

        dialog.Add(lbl, ok);
        Application.Run(dialog);
    }
}
