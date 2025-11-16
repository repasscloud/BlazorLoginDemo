using Cinturon360.Tui.App.Shell;
using Terminal.Gui;

namespace Cinturon360.Tui.App.Pages.Org;

public sealed class EditOrgPage : PageBase
{
    private readonly string _orgId;

    public override string Title => "Edit Organization";

    public EditOrgPage(string orgId)
    {
        _orgId = orgId;

        var lbl = new Label("Edit Organization")
        {
            X = 1,
            Y = 1
        };

        var idLabel = new Label($"Id: {_orgId}")
        {
            X = 1,
            Y = Pos.Bottom(lbl) + 1
        };

        _root.Add(lbl, idLabel);

        // You can add more controls here later (fields, save button, etc.)
    }
}
