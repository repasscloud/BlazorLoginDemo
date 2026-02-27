using System;
using Cinturon360.Tui.App.Shell;
using Terminal.Gui;
using static Terminal.Gui.View;

namespace Cinturon360.Tui.App.Pages.Shared;

/// <summary>
/// Reference page showing the F2 / F10 / ESC edit pattern.
/// Copy this when creating new editable pages.
/// </summary>
public sealed class EditableTemplatePage : PageBase
{
    // Simple demo entity to show the pattern.
    private sealed record DemoEntity(
        string Name,
        string Description,
        bool IsActive
    );

    private DemoEntity _current;
    private DemoEntity _snapshot; // last saved state for ESC revert

    private enum EditorState
    {
        Viewing,
        Editing
    }

    private EditorState _state = EditorState.Viewing;

    // UI
    private readonly Label _lblMode;
    private readonly TextField _txtName;
    private readonly TextView _txtDescription;
    private readonly CheckBox _chkIsActive;

    public override string Title => "Editable Template (F2 / F10 / ESC)";

    public EditableTemplatePage()
    {
        // Initial data (pretend this came from API/db)
        _current = new DemoEntity(
            Name: "Sample Entity",
            Description: "Use F2 to enter edit mode, change me, F2/F10/ESC to play with states.",
            IsActive: true
        );
        _snapshot = _current;

        // --- Header / hints ---------------------------------------------------

        var lblTitle = new Label("Editable Template Page")
        {
            X = 1,
            Y = 1
        };

        var lblHints = new Label("F2 = Toggle Edit Mode   F10 = Save   ESC = Cancel Changes (only in Edit mode)")
        {
            X = 1,
            Y = Pos.Bottom(lblTitle) + 1,
            Width = Dim.Fill(1)
        };

        _lblMode = new Label(string.Empty)
        {
            X = 1,
            Y = Pos.Bottom(lblHints) + 1,
            Width = Dim.Fill(1)
        };

        // --- Fields -----------------------------------------------------------

        var lblName = new Label("Name:")
        {
            X = 1,
            Y = Pos.Bottom(_lblMode) + 1
        };

        _txtName = new TextField("")
        {
            X = Pos.Right(lblName) + 1,
            Y = Pos.Top(lblName),
            Width = 40
        };

        var lblDesc = new Label("Description:")
        {
            X = 1,
            Y = Pos.Bottom(lblName) + 1
        };

        _txtDescription = new TextView
        {
            X = Pos.Right(lblDesc) + 1,
            Y = Pos.Top(lblDesc),
            Width = 60,
            Height = 5,
            WordWrap = true
        };

        _chkIsActive = new CheckBox("Is Active")
        {
            X = 1,
            Y = Pos.Bottom(_txtDescription) + 1
        };

        _root.Add(
            lblTitle,
            lblHints,
            _lblMode,
            lblName,
            _txtName,
            lblDesc,
            _txtDescription,
            _chkIsActive
        );

        // Load initial data into controls and set initial state
        PopulateFromEntity(_current);
        SetState(EditorState.Viewing);

        // Global key handling for this page
        _root.KeyPress += RootOnKeyPress;
    }

    // ------------------------------------------------------------------------
    // Key handling: F2 / F10 / ESC
    // ------------------------------------------------------------------------

    private void RootOnKeyPress(KeyEventEventArgs args)
    {
        var key = args.KeyEvent.Key;

        // F2: toggle edit mode
        if (key == Key.F2)
        {
            if (_state == EditorState.Viewing)
            {
                EnterEditMode();
            }
            else if (_state == EditorState.Editing)
            {
                // Leave edit mode but keep whatever is in the fields (unsaved)
                ExitEditModeKeepChanges();
            }

            args.Handled = true;
            return;
        }

        // F10: save command (page-specific placeholder)
        if (key == Key.F10)
        {
            if (_state == EditorState.Viewing || _state == EditorState.Editing)
            {
                SaveCommand();
                args.Handled = true;
            }

            return;
        }

        // ESC while editing: discard changes and revert to last saved snapshot
        if (key == Key.Esc && _state == EditorState.Editing)
        {
            CancelEditAndRevert();
            args.Handled = true;
        }
    }

    // ------------------------------------------------------------------------
    // State machine helpers
    // ------------------------------------------------------------------------

    private void SetState(EditorState state)
    {
        _state = state;

        switch (state)
        {
            case EditorState.Viewing:
                _lblMode.Text = "Mode: Viewing (F2 to Edit, F10 to Save)";
                SetFieldsReadOnly(true);
                break;

            case EditorState.Editing:
                _lblMode.Text = "Mode: Editing (F2 to Exit, F10 to Save, ESC to Cancel Changes)";
                SetFieldsReadOnly(false);
                break;
        }
    }

    private void SetFieldsReadOnly(bool readOnly)
    {
        _txtName.ReadOnly = readOnly;
        _txtDescription.ReadOnly = readOnly;

        // For CheckBox: disable it instead of ReadOnly
        _chkIsActive.Enabled = !readOnly;

        // Optional: keep focus behaviour consistent
        _txtName.CanFocus = true;
        _txtDescription.CanFocus = true;
        _chkIsActive.CanFocus = !readOnly;
    }

    private void EnterEditMode()
    {
        // Take a snapshot of the last saved state so ESC can revert to it
        _snapshot = _current;
        SetState(EditorState.Editing);
        _txtName.SetFocus();
    }

    private void ExitEditModeKeepChanges()
    {
        // Keep whatever is currently typed in the fields (unsaved changes)
        // but flip back to viewing mode.
        SetState(EditorState.Viewing);
    }

    private void CancelEditAndRevert()
    {
        // Revert fields back to the last saved snapshot and go back to viewing mode.
        _current = _snapshot;
        PopulateFromEntity(_current);
        SetState(EditorState.Viewing);
    }

    // ------------------------------------------------------------------------
    // Data <-> UI mapping
    // ------------------------------------------------------------------------

    private void PopulateFromEntity(DemoEntity entity)
    {
        _txtName.Text = entity.Name ?? string.Empty;
        _txtDescription.Text = entity.Description ?? string.Empty;
        _chkIsActive.Checked = entity.IsActive;
    }

    private DemoEntity ReadEntityFromFields()
    {
        var name = _txtName.Text?.ToString() ?? string.Empty;
        var desc = _txtDescription.Text?.ToString() ?? string.Empty;
        var active = _chkIsActive.Checked;

        return new DemoEntity(
            Name: name,
            Description: desc,
            IsActive: active
        );
    }

    // ------------------------------------------------------------------------
    // Save command (placeholder)
    // ------------------------------------------------------------------------

    private void SaveCommand()
    {
        // Pull current values from the fields
        _current = ReadEntityFromFields();

        // TODO: here you would call your API / repository to persist _current
        // This is just a placeholder:
        MessageBox.Query(
            "Save",
            "Pretend we saved this entity to the backend here.\n\n" +
            "Name: " + _current.Name + "\n" +
            "IsActive: " + (_current.IsActive ? "Yes" : "No"),
            "OK");

        // After a real save you might want to refresh from the backend and
        // set _snapshot = _current again.
        _snapshot = _current;

        // Back to viewing mode
        SetState(EditorState.Viewing);
    }
}
