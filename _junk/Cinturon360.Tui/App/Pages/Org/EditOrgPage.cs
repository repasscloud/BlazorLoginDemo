using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Cinturon360.Tui.App.Infrastructure.Http;
using Cinturon360.Tui.App.Shell;
using Terminal.Gui;
using static Cinturon360.Shared.Services.Interfaces.Platform.IAdminOrgServiceUnified;
using Cinturon360.Shared.Models.Static.Billing;
using static Terminal.Gui.View;

namespace Cinturon360.Tui.App.Pages.Org;

public sealed class EditOrgPage : PageBase
{
    private readonly IApiClientFactory _clientFactory;
    private readonly string? _orgId;

    private OrgAggregate? _currentOrg;
    private bool _loadedOnce;

    private static readonly string LogFilePath =
        Path.Combine(AppContext.BaseDirectory, "cinturon360-tui-errors.log");

    // UI
    private readonly Label _lblOrgId;
    private readonly TextField _txtOrgId;

    // Identity of Org
    private readonly TextField _txtName;
    private readonly TextField _txtType;
    private readonly TextField _txtIsActive;
    private readonly TextField _txtParentOrgId;
    private readonly TextField _txtParentOrgName;
    private readonly TextField _txtParentOrgType;

    // Company Defaults
    private readonly TextField _txtDefaultCurrency;
    private readonly TextField _txtTaxIdType;
    private readonly TextField _txtTaxId;

    // Physical Address
    private readonly TextField _txtAddressLine1;
    private readonly TextField _txtAddressLine2;
    private readonly TextField _txtAddressLine3;
    private readonly TextField _txtCity;
    private readonly TextField _txtState;
    private readonly TextField _txtPostalCode;
    private readonly TextField _txtCountry;
    
    // Mailing Address
    private readonly TextField _txtMailingAddressLine1;
    private readonly TextField _txtMailingAddressLine2;
    private readonly TextField _txtMailingAddressLine3;
    private readonly TextField _txtMailingCity;
    private readonly TextField _txtMailingState;
    private readonly TextField _txtMailingPostalCode;
    private readonly TextField _txtMailingCountry;

    // General / Commercial Contact
    private readonly TextField _txtContactFirstName;
    private readonly TextField _txtContactLastName;
    private readonly TextField _txtContactPersonCountryCode;
    private readonly TextField _txtContactPhone;
    private readonly TextField _txtContactEmail;
    private readonly TextField _txtContactJobTitle;

    // Billing Contact
    private readonly TextField _txtBillingPersonFirstName;
    private readonly TextField _txtBillingPersonLastName;
    private readonly TextField _txtBillingPersonCountryCode;
    private readonly TextField _txtBillingPersonPhone;
    private readonly TextField _txtBillingPersonEmail;
    private readonly TextField _txtBillingPersonJobTitle;

    // Admin Contact
    private readonly TextField _txtAdminPersonFirstName;
    private readonly TextField _txtAdminPersonLastName;
    private readonly TextField _txtAdminPersonCountryCode;
    private readonly TextField _txtAdminPersonPhone;
    private readonly TextField _txtAdminPersonEmail;
    private readonly TextField _txtAdminPersonJobTitle;

    // Policies
    private readonly TextField _txtTravelPolicy;
    private readonly TextField _txtExpensePolicy;

    // License Agreement
    private readonly TextField _txtLicenseAgreementId;
    private readonly TextField _txtPaymentStatus;
    private readonly TextField _txtLicenseAgreementEndDate;
    private readonly TextField _txtLicenseAgreementRenewalDate;
    private readonly TextField _txtClientCountLimit;
    private readonly TextField _txtUserAccountLimit;

    // Payment status colour schemes
    private readonly ColorScheme _statusNeutralScheme;
    private readonly ColorScheme _statusInfoScheme;
    private readonly ColorScheme _statusOkScheme;
    private readonly ColorScheme _statusWarnScheme;
    private readonly ColorScheme _statusErrorScheme;

    // Buttons
    private readonly Button _btnEdit;
    private readonly Button _btnSave;
    private readonly Button _btnCancel;

    private enum EditorState
    {
        Searching, // OrgId editable, details empty/readonly
        Viewing,   // Org loaded, OrgId locked, fields readonly
        Editing    // Org loaded, OrgId locked, fields editable
    }

    private EditorState _state = EditorState.Searching;

    public override string Title => "Edit Organization";

    public EditOrgPage(IApiClientFactory clientFactory, string? orgId)
    {
        _clientFactory = clientFactory;
        _orgId = orgId;

        // --- Payment status colour schemes ---
        _statusNeutralScheme = new ColorScheme
        {
            Normal   = Application.Driver.MakeAttribute(Color.Gray,        Color.Black),
            Focus    = Application.Driver.MakeAttribute(Color.Gray,        Color.Black),
            HotNormal= Application.Driver.MakeAttribute(Color.Gray,        Color.Black),
            HotFocus = Application.Driver.MakeAttribute(Color.Gray,        Color.Black),
            Disabled = Application.Driver.MakeAttribute(Color.Gray,        Color.Black)
        };

        _statusInfoScheme = new ColorScheme
        {
            Normal   = Application.Driver.MakeAttribute(Color.BrightBlue,  Color.Black),
            Focus    = Application.Driver.MakeAttribute(Color.BrightBlue,  Color.Black),
            HotNormal= Application.Driver.MakeAttribute(Color.BrightBlue,  Color.Black),
            HotFocus = Application.Driver.MakeAttribute(Color.BrightBlue,  Color.Black),
            Disabled = Application.Driver.MakeAttribute(Color.BrightBlue,  Color.Black)
        };

        _statusOkScheme = new ColorScheme
        {
            Normal   = Application.Driver.MakeAttribute(Color.BrightGreen, Color.Black),
            Focus    = Application.Driver.MakeAttribute(Color.BrightGreen, Color.Black),
            HotNormal= Application.Driver.MakeAttribute(Color.BrightGreen, Color.Black),
            HotFocus = Application.Driver.MakeAttribute(Color.BrightGreen, Color.Black),
            Disabled = Application.Driver.MakeAttribute(Color.BrightGreen, Color.Black)
        };

        _statusWarnScheme = new ColorScheme
        {
            Normal   = Application.Driver.MakeAttribute(Color.BrightYellow, Color.Black),
            Focus    = Application.Driver.MakeAttribute(Color.BrightYellow, Color.Black),
            HotNormal= Application.Driver.MakeAttribute(Color.BrightYellow, Color.Black),
            HotFocus = Application.Driver.MakeAttribute(Color.BrightYellow, Color.Black),
            Disabled = Application.Driver.MakeAttribute(Color.BrightYellow, Color.Black)
        };

        _statusErrorScheme = new ColorScheme
        {
            Normal   = Application.Driver.MakeAttribute(Color.BrightRed,   Color.Black),
            Focus    = Application.Driver.MakeAttribute(Color.BrightRed,   Color.Black),
            HotNormal= Application.Driver.MakeAttribute(Color.BrightRed,   Color.Black),
            HotFocus = Application.Driver.MakeAttribute(Color.BrightRed,   Color.Black),
            Disabled = Application.Driver.MakeAttribute(Color.BrightRed,   Color.Black)
        };


        const int labelX = 1;
        const int valueX = 20;

        // --- OrgId row + buttons ---
        _lblOrgId = new Label("Org ID:")
        {
            X = 1,
            Y = 1
        };

        _txtOrgId = new TextField(_orgId ?? string.Empty)
        {
            X = Pos.Right(_lblOrgId) + 1,
            Y = Pos.Top(_lblOrgId),
            Width = 40
        };
        _txtOrgId.KeyPress += args =>
        {
            if (args.KeyEvent.Key == Key.Enter)
            {
                args.Handled = true;
                var id = _txtOrgId.Text?.ToString() ?? string.Empty;
                LoadOrgFromApi(id);
            }
        };

        _btnCancel = new Button("Cancel");
        _btnSave   = new Button("Save");
        _btnEdit   = new Button("Edit");

        _btnCancel.X = Pos.AnchorEnd(_btnCancel.Text.Length + 4);
        _btnCancel.Y = Pos.Top(_lblOrgId);

        _btnSave.X = Pos.Left(_btnCancel) - (_btnSave.Text.Length + 6);
        _btnSave.Y = Pos.Top(_lblOrgId);

        _btnEdit.X = Pos.Left(_btnSave) - (_btnEdit.Text.Length + 6);
        _btnEdit.Y = Pos.Top(_lblOrgId);

        _btnEdit.Clicked  += OnEditClicked;
        _btnSave.Clicked  += OnSaveClicked;
        _btnCancel.Clicked += OnCancelClicked;


        // ------------------- TOP ROW: IDENTITY + DEFAULTS/POLICIES -------------------

        var identityFrame = new FrameView("Identity")
        {
            X = 1,
            Y = Pos.Bottom(_lblOrgId) + 1,   // was Pos.Bottom(_btnEdit) + 1
            Width = Dim.Percent(50) - 2,
            Height = 7                       // we’ll tweak heights next
        };

        _txtName = new TextField("")
        {
            X = valueX,
            Y = 0,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        identityFrame.Add(
            new Label("Name:")
            {
                X = labelX,
                Y = 0
            },
            _txtName
        );

        _txtType = new TextField("")
        {
            X = valueX,
            Y = 1,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        identityFrame.Add(
            new Label("Type:")
            {
                X = labelX,
                Y = 1
            },
            _txtType
        );

        _txtIsActive = new TextField("")
        {
            X = valueX,
            Y = 2,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        identityFrame.Add(
            new Label("Is Active:")
            {
                X = labelX,
                Y = 2
            },
            _txtIsActive
        );

        _txtParentOrgId = new TextField("")
        {
            X = valueX,
            Y = 3,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        identityFrame.Add(
            new Label("Parent Id:")
            {
                X = labelX,
                Y = 3
            },
            _txtParentOrgId
        );

        _txtParentOrgName = new TextField("")
        {
            X = valueX,
            Y = 4,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        identityFrame.Add(
            new Label("Parent Name:")
            {
                X = labelX,
                Y = 4
            },
            _txtParentOrgName
        );

        _txtParentOrgType = new TextField("")
        {
            X = valueX,
            Y = 5,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        identityFrame.Add(
            new Label("Parent Type:")
            {
                X = labelX,
                Y = 5
            },
            _txtParentOrgType
        );

        var defaultsFrame = new FrameView("Defaults & Policies")
        {
            X = Pos.Right(identityFrame) + 1,
            Y = identityFrame.Y,
            Width = Dim.Fill(1),
            Height = 7   // 5 rows (0..4) + 2 borders, no extra blank row
        };

        _txtDefaultCurrency = new TextField("")
        {
            X = valueX,
            Y = 0,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        defaultsFrame.Add(
            new Label("Default Currency:")
            {
                X = labelX,
                Y = 0
            },
            _txtDefaultCurrency
        );

        _txtTaxIdType = new TextField("")
        {
            X = valueX,
            Y = 1,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        defaultsFrame.Add(
            new Label("Tax Id Type:")
            {
                X = labelX,
                Y = 1
            },
            _txtTaxIdType
        );

        _txtTaxId = new TextField("")
        {
            X = valueX,
            Y = 2,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        defaultsFrame.Add(
            new Label("Tax Id:")
            {
                X = labelX,
                Y = 2
            },
            _txtTaxId
        );

        _txtTravelPolicy = new TextField("")
        {
            X = valueX,
            Y = 3,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        defaultsFrame.Add(
            new Label("Travel Policy:")
            {
                X = labelX,
                Y = 3
            },
            _txtTravelPolicy
        );

        _txtExpensePolicy = new TextField("")
        {
            X = valueX,
            Y = 4,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        defaultsFrame.Add(
            new Label("Expense Policy:")
            {
                X = labelX,
                Y = 4
            },
            _txtExpensePolicy
        );

        // ------------------- SECOND ROW: PHYSICAL + MAILING -------------------
        var physicalFrame = new FrameView("Physical Address")
        {
            X = 1,
            Y = Pos.Bottom(identityFrame) + 1, // keeps 1 blank line between boxes
            Width = Dim.Percent(50) - 2,
            Height = 9   // 7 rows (0..6) + 2 for borders
        };

        _txtAddressLine1 = new TextField("")
        {
            X = valueX,
            Y = 0,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        physicalFrame.Add(
            new Label("Address Line 1:")
            {
                X = labelX,
                Y = 0
            },
            _txtAddressLine1
        );

        _txtAddressLine2 = new TextField("")
        {
            X = valueX,
            Y = 1,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        physicalFrame.Add(
            new Label("Address Line 2:")
            {
                X = labelX,
                Y = 1
            },
            _txtAddressLine2
        );

        _txtAddressLine3 = new TextField("")
        {
            X = valueX,
            Y = 2,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        physicalFrame.Add(
            new Label("Address Line 3:")
            {
                X = labelX,
                Y = 2
            },
            _txtAddressLine3
        );

        _txtCity = new TextField("")
        {
            X = valueX,
            Y = 3,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        physicalFrame.Add(
            new Label("City:")
            {
                X = labelX,
                Y = 3
            },
            _txtCity
        );

        _txtState = new TextField("")
        {
            X = valueX,
            Y = 4,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        physicalFrame.Add(
            new Label("State:")
            {
                X = labelX,
                Y = 4
            },
            _txtState
        );

        _txtPostalCode = new TextField("")
        {
            X = valueX,
            Y = 5,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        physicalFrame.Add(
            new Label("Postal Code:")
            {
                X = labelX,
                Y = 5
            },
            _txtPostalCode
        );

        _txtCountry = new TextField("")
        {
            X = valueX,
            Y = 6,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        physicalFrame.Add(
            new Label("Country:")
            {
                X = labelX,
                Y = 6
            },
            _txtCountry
        );

        var mailingFrame = new FrameView("Mailing Address")
        {
            X = Pos.Right(physicalFrame) + 1,
            Y = physicalFrame.Y,
            Width = Dim.Fill(1),
            Height = 9
        };

        _txtMailingAddressLine1 = new TextField("")
        {
            X = valueX,
            Y = 0,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        mailingFrame.Add(
            new Label("Address Line 1:")
            {
                X = labelX,
                Y = 0
            },
            _txtMailingAddressLine1
        );

        _txtMailingAddressLine2 = new TextField("")
        {
            X = valueX,
            Y = 1,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        mailingFrame.Add(
            new Label("Address Line 2:")
            {
                X = labelX,
                Y = 1
            },
            _txtMailingAddressLine2
        );

        _txtMailingAddressLine3 = new TextField("")
        {
            X = valueX,
            Y = 2,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        mailingFrame.Add(
            new Label("Address Line 3:")
            {
                X = labelX,
                Y = 2
            },
            _txtMailingAddressLine3
        );

        _txtMailingCity = new TextField("")
        {
            X = valueX,
            Y = 3,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        mailingFrame.Add(
            new Label("City:")
            {
                X = labelX,
                Y = 3
            },
            _txtMailingCity
        );

        _txtMailingState = new TextField("")
        {
            X = valueX,
            Y = 4,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        mailingFrame.Add(
            new Label("State:")
            {
                X = labelX,
                Y = 4
            },
            _txtMailingState
        );

        _txtMailingPostalCode = new TextField("")
        {
            X = valueX,
            Y = 5,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        mailingFrame.Add(
            new Label("Postal Code:")
            {
                X = labelX,
                Y = 5
            },
            _txtMailingPostalCode
        );

        _txtMailingCountry = new TextField("")
        {
            X = valueX,
            Y = 6,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        mailingFrame.Add(
            new Label("Country:")
            {
                X = labelX,
                Y = 6
            },
            _txtMailingCountry
        );

        // ------------------- THIRD ROW: CONTACTS (GENERAL + BILLING) -------------------

        var contactFrame = new FrameView("General / Commercial Contact")
        {
            X = 1,
            Y = Pos.Bottom(physicalFrame) + 1,
            Width = Dim.Percent(50) - 2,
            Height = 8
        };

        _txtContactFirstName = new TextField("")
        {
            X = valueX,
            Y = 0,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        contactFrame.Add(
            new Label("First Name:")
            {
                X = labelX,
                Y = 0
            },
            _txtContactFirstName
        );

        _txtContactLastName = new TextField("")
        {
            X = valueX,
            Y = 1,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        contactFrame.Add(
            new Label("Last Name:")
            {
                X = labelX,
                Y = 1
            },
            _txtContactLastName
        );

        _txtContactPersonCountryCode = new TextField("")
        {
            X = valueX,
            Y = 2,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        contactFrame.Add(
            new Label("Country Code:")
            {
                X = labelX,
                Y = 2
            },
            _txtContactPersonCountryCode
        );

        _txtContactPhone = new TextField("")
        {
            X = valueX,
            Y = 3,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        contactFrame.Add(
            new Label("Phone:")
            {
                X = labelX,
                Y = 3
            },
            _txtContactPhone
        );

        _txtContactEmail = new TextField("")
        {
            X = valueX,
            Y = 4,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        contactFrame.Add(
            new Label("Email:")
            {
                X = labelX,
                Y = 4
            },
            _txtContactEmail
        );

        _txtContactJobTitle = new TextField("")
        {
            X = valueX,
            Y = 5,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        contactFrame.Add(
            new Label("Job Title:")
            {
                X = labelX,
                Y = 5
            },
            _txtContactJobTitle
        );

        var billingFrame = new FrameView("Billing Contact")
        {
            X = Pos.Right(contactFrame) + 1,
            Y = contactFrame.Y,
            Width = Dim.Fill(1),
            Height = 8
        };

        _txtBillingPersonFirstName = new TextField("")
        {
            X = valueX,
            Y = 0,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        billingFrame.Add(
            new Label("First Name:")
            {
                X = labelX,
                Y = 0
            },
            _txtBillingPersonFirstName
        );

        _txtBillingPersonLastName = new TextField("")
        {
            X = valueX,
            Y = 1,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        billingFrame.Add(
            new Label("Last Name:")
            {
                X = labelX,
                Y = 1
            },
            _txtBillingPersonLastName
        );

        _txtBillingPersonCountryCode = new TextField("")
        {
            X = valueX,
            Y = 2,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        billingFrame.Add(
            new Label("Country Code:")
            {
                X = labelX,
                Y = 2
            },
            _txtBillingPersonCountryCode
        );

        _txtBillingPersonPhone = new TextField("")
        {
            X = valueX,
            Y = 3,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        billingFrame.Add(
            new Label("Phone:")
            {
                X = labelX,
                Y = 3
            },
            _txtBillingPersonPhone
        );

        _txtBillingPersonEmail = new TextField("")
        {
            X = valueX,
            Y = 4,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        billingFrame.Add(
            new Label("Email:")
            {
                X = labelX,
                Y = 4
            },
            _txtBillingPersonEmail
        );

        _txtBillingPersonJobTitle = new TextField("")
        {
            X = valueX,
            Y = 5,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        billingFrame.Add(
            new Label("Job Title:")
            {
                X = labelX,
                Y = 5
            },
            _txtBillingPersonJobTitle
        );

        // ------------------- FOURTH ROW: ADMIN + LICENSE -------------------

        var adminFrame = new FrameView("Admin / Technical Contact")
        {
            X = 1,
            Y = Pos.Bottom(contactFrame) + 1,
            Width = Dim.Percent(50) - 2,
            Height = 8
        };

        _txtAdminPersonFirstName = new TextField("")
        {
            X = valueX,
            Y = 0,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        adminFrame.Add(
            new Label("First Name:")
            {
                X = labelX,
                Y = 0
            },
            _txtAdminPersonFirstName
        );

        _txtAdminPersonLastName = new TextField("")
        {
            X = valueX,
            Y = 1,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        adminFrame.Add(
            new Label("Last Name:")
            {
                X = labelX,
                Y = 1
            },
            _txtAdminPersonLastName
        );

        _txtAdminPersonCountryCode = new TextField("")
        {
            X = valueX,
            Y = 2,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        adminFrame.Add(
            new Label("Country Code:")
            {
                X = labelX,
                Y = 2
            },
            _txtAdminPersonCountryCode
        );

        _txtAdminPersonPhone = new TextField("")
        {
            X = valueX,
            Y = 3,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        adminFrame.Add(
            new Label("Phone:")
            {
                X = labelX,
                Y = 3
            },
            _txtAdminPersonPhone
        );

        _txtAdminPersonEmail = new TextField("")
        {
            X = valueX,
            Y = 4,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        adminFrame.Add(
            new Label("Email:")
            {
                X = labelX,
                Y = 4
            },
            _txtAdminPersonEmail
        );

        _txtAdminPersonJobTitle = new TextField("")
        {
            X = valueX,
            Y = 5,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        adminFrame.Add(
            new Label("Job Title:")
            {
                X = labelX,
                Y = 5
            },
            _txtAdminPersonJobTitle
        );

        var licenseFrame = new FrameView("License Agreement")
        {
            X = Pos.Right(adminFrame) + 1,
            Y = adminFrame.Y,
            Width = Dim.Fill(1),
            Height = 8
        };

        _txtLicenseAgreementId = new TextField("")
        {
            X = valueX,
            Y = 0,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        licenseFrame.Add(
            new Label("License Id:")
            {
                X = labelX,
                Y = 0
            },
            _txtLicenseAgreementId
        );

        _txtPaymentStatus = new TextField("")
        {
            X = valueX,
            Y = 1,
            Width = Dim.Fill(1),
            ReadOnly = true,
            ColorScheme = _statusNeutralScheme
        };
        licenseFrame.Add(
            new Label("Payment Status:")
            {
                X = labelX,
                Y = 1
            },
            _txtPaymentStatus
        );

        _txtLicenseAgreementEndDate = new TextField("")
        {
            X = valueX,
            Y = 2,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        licenseFrame.Add(
            new Label("End Date:")
            {
                X = labelX,
                Y = 2
            },
            _txtLicenseAgreementEndDate
        );

        _txtLicenseAgreementRenewalDate = new TextField("")
        {
            X = valueX,
            Y = 3,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        licenseFrame.Add(
            new Label("Renewal Date:")
            {
                X = labelX,
                Y = 3
            },
            _txtLicenseAgreementRenewalDate
        );

        _txtClientCountLimit = new TextField("")
        {
            X = valueX,
            Y = 4,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        licenseFrame.Add(
            new Label("Client Limit:")
            {
                X = labelX,
                Y = 4
            },
            _txtClientCountLimit
        );

        _txtUserAccountLimit = new TextField("")
        {
            X = valueX,
            Y = 5,
            Width = Dim.Fill(1),
            ReadOnly = true
        };
        licenseFrame.Add(
            new Label("User Limit:")
            {
                X = labelX,
                Y = 5
            },
            _txtUserAccountLimit
        );

        // Add everything to root
        _root.Add(
            _lblOrgId, _txtOrgId,
            _btnEdit, _btnSave, _btnCancel,
            identityFrame, defaultsFrame,
            physicalFrame, mailingFrame,
            contactFrame, billingFrame,
            adminFrame, licenseFrame
        );

        // Global edit/save keys for this page:
        // F2  -> toggle edit mode
        // F10 -> save
        // ESC -> cancel changes when in edit mode
        _root.KeyPress += RootOnKeyPress;

        SetState(EditorState.Searching);
    }

    public override void OnActivated()
    {
        base.OnActivated();

        if (_loadedOnce)
            return;

        _loadedOnce = true;

        if (!string.IsNullOrWhiteSpace(_orgId))
        {
            // Initial load from the org id passed in
            LoadOrgFromApi(_orgId);
        }
    }

    private void RootOnKeyPress(KeyEventEventArgs args)
    {
        var key = args.KeyEvent.Key;

        // F2: toggle edit mode
        if (key == Key.F2)
        {
            if (_state == EditorState.Viewing && _currentOrg is not null)
            {
                // Enter edit mode
                SetState(EditorState.Editing);
            }
            else if (_state == EditorState.Editing)
            {
                // Leave edit mode, keep whatever is typed in the fields (unsaved)
                SetState(EditorState.Viewing);
            }

            args.Handled = true;
            return;
        }

        // F10: save (placeholder)
        if (key == Key.F10)
        {
            if (_currentOrg is not null && _state != EditorState.Searching)
            {
                OnSaveClicked();
                args.Handled = true;
            }

            return;
        }

        // ESC while editing: cancel ALL changes and go back to Viewing
        if (key == Key.Esc && _state == EditorState.Editing)
        {
            OnCancelClicked();
            args.Handled = true;
        }
    }


    // ---------------- State helpers ----------------

    private void SetState(EditorState state)
    {
        _state = state;

        switch (state)
        {
            case EditorState.Searching:
                _txtOrgId.ReadOnly = false;
                SetDetailFieldsReadOnly(true);
                _btnEdit.Enabled = false;
                _btnSave.Enabled = false;
                _btnCancel.Enabled = false;
                break;

            case EditorState.Viewing:
                _txtOrgId.ReadOnly = true;  // lock OrgId after a successful load
                SetDetailFieldsReadOnly(true);
                _btnEdit.Enabled = _currentOrg is not null;
                _btnSave.Enabled = false;
                _btnCancel.Enabled = false;
                break;

            case EditorState.Editing:
                _txtOrgId.ReadOnly = true;
                SetDetailFieldsReadOnly(false);
                _btnEdit.Enabled = false;
                _btnSave.Enabled = true;
                _btnCancel.Enabled = true;
                break;
        }
    }

    private void SetDetailFieldsReadOnly(bool readOnly)
    {
        // Editable in Edit mode
        _txtName.ReadOnly = readOnly;
        _txtDefaultCurrency.ReadOnly = readOnly;
        _txtTaxIdType.ReadOnly = readOnly;
        _txtTaxId.ReadOnly = readOnly;

        _txtAddressLine1.ReadOnly = readOnly;
        _txtAddressLine2.ReadOnly = readOnly;
        _txtAddressLine3.ReadOnly = readOnly;
        _txtCity.ReadOnly = readOnly;
        _txtState.ReadOnly = readOnly;
        _txtPostalCode.ReadOnly = readOnly;
        _txtCountry.ReadOnly = readOnly;

        _txtMailingAddressLine1.ReadOnly = readOnly;
        _txtMailingAddressLine2.ReadOnly = readOnly;
        _txtMailingAddressLine3.ReadOnly = readOnly;
        _txtMailingCity.ReadOnly = readOnly;
        _txtMailingState.ReadOnly = readOnly;
        _txtMailingPostalCode.ReadOnly = readOnly;
        _txtMailingCountry.ReadOnly = readOnly;

        _txtContactFirstName.ReadOnly = readOnly;
        _txtContactLastName.ReadOnly = readOnly;
        _txtContactPersonCountryCode.ReadOnly = readOnly;
        _txtContactPhone.ReadOnly = readOnly;
        _txtContactEmail.ReadOnly = readOnly;

        _txtBillingPersonFirstName.ReadOnly = readOnly;
        _txtBillingPersonLastName.ReadOnly = readOnly;
        _txtBillingPersonCountryCode.ReadOnly = readOnly;
        _txtBillingPersonPhone.ReadOnly = readOnly;
        _txtBillingPersonEmail.ReadOnly = readOnly;
        _txtBillingPersonJobTitle.ReadOnly = readOnly;

        _txtAdminPersonFirstName.ReadOnly = readOnly;
        _txtAdminPersonLastName.ReadOnly = readOnly;
        _txtAdminPersonCountryCode.ReadOnly = readOnly;
        _txtAdminPersonPhone.ReadOnly = readOnly;
        _txtAdminPersonEmail.ReadOnly = readOnly;
        _txtAdminPersonJobTitle.ReadOnly = readOnly;

        // Always read-only – ignore `readOnly` flag
        _txtIsActive.ReadOnly = true;
        _txtType.ReadOnly = true;
        _txtParentOrgId.ReadOnly = true;
        _txtParentOrgName.ReadOnly = true;
        _txtParentOrgType.ReadOnly = true;
        _txtLicenseAgreementId.ReadOnly = true;
        _txtPaymentStatus.ReadOnly = true;
        _txtLicenseAgreementEndDate.ReadOnly = true;
        _txtLicenseAgreementRenewalDate.ReadOnly = true;
        _txtClientCountLimit.ReadOnly = true;
        _txtUserAccountLimit.ReadOnly = true;
        _txtTravelPolicy.ReadOnly = true;
        _txtExpensePolicy.ReadOnly = true;

        // Optional: stop focus on non-editable system fields
        _txtIsActive.CanFocus = false;
        _txtType.CanFocus = false;
        _txtParentOrgId.CanFocus = false;
        _txtParentOrgName.CanFocus = false;
        _txtParentOrgType.CanFocus = false;
        _txtLicenseAgreementId.CanFocus = false;
        _txtPaymentStatus.CanFocus = false;
        _txtLicenseAgreementEndDate.CanFocus = false;
        _txtLicenseAgreementRenewalDate.CanFocus = false;
        _txtClientCountLimit.CanFocus = false;
        _txtUserAccountLimit.CanFocus = false;
        _txtTravelPolicy.CanFocus = false;
        _txtExpensePolicy.CanFocus = false;
    }

    private void ClearOrgFields()
    {
        _txtName.Text = string.Empty;
        _txtType.Text = string.Empty;
        _txtIsActive.Text = string.Empty;
        _txtParentOrgId.Text = string.Empty;
        _txtParentOrgName.Text = string.Empty;
        _txtParentOrgType.Text = string.Empty;

        _txtDefaultCurrency.Text = string.Empty;
        _txtTaxIdType.Text = string.Empty;
        _txtTaxId.Text = string.Empty;

        _txtAddressLine1.Text = string.Empty;
        _txtAddressLine2.Text = string.Empty;
        _txtAddressLine3.Text = string.Empty;
        _txtCity.Text = string.Empty;
        _txtState.Text = string.Empty;
        _txtPostalCode.Text = string.Empty;
        _txtCountry.Text = string.Empty;

        _txtMailingAddressLine1.Text = string.Empty;
        _txtMailingAddressLine2.Text = string.Empty;
        _txtMailingAddressLine3.Text = string.Empty;
        _txtMailingCity.Text = string.Empty;
        _txtMailingState.Text = string.Empty;
        _txtMailingPostalCode.Text = string.Empty;
        _txtMailingCountry.Text = string.Empty;

        _txtContactFirstName.Text = string.Empty;
        _txtContactLastName.Text = string.Empty;
        _txtContactPersonCountryCode.Text = string.Empty;
        _txtContactPhone.Text = string.Empty;
        _txtContactEmail.Text = string.Empty;

        _txtBillingPersonFirstName.Text = string.Empty;
        _txtBillingPersonLastName.Text = string.Empty;
        _txtBillingPersonCountryCode.Text = string.Empty;
        _txtBillingPersonPhone.Text = string.Empty;
        _txtBillingPersonEmail.Text = string.Empty;
        _txtBillingPersonJobTitle.Text = string.Empty;

        _txtAdminPersonFirstName.Text = string.Empty;
        _txtAdminPersonLastName.Text = string.Empty;
        _txtAdminPersonCountryCode.Text = string.Empty;
        _txtAdminPersonPhone.Text = string.Empty;
        _txtAdminPersonEmail.Text = string.Empty;
        _txtAdminPersonJobTitle.Text = string.Empty;

        _txtTravelPolicy.Text = string.Empty;
        _txtExpensePolicy.Text = string.Empty;

        _txtLicenseAgreementId.Text = string.Empty;
        _txtPaymentStatus.Text = string.Empty;
        _txtLicenseAgreementEndDate.Text = string.Empty;
        _txtLicenseAgreementRenewalDate.Text = string.Empty;
        _txtClientCountLimit.Text = string.Empty;
        _txtUserAccountLimit.Text = string.Empty;
    }

    private void PopulateFields(OrgAggregate aggregate)
    {
        _currentOrg = aggregate;

        var org = aggregate.Org;
        var license = aggregate.LicenseAgreement;

        // Identity
        _txtName.Text = org.Name ?? string.Empty;
        _txtType.Text = org.Type.ToString();
        _txtIsActive.Text = org.IsActive ? "Yes" : "No";
        _txtParentOrgId.Text = org.ParentOrganizationId ?? string.Empty;
        _txtParentOrgName.Text = org.Parent?.Name ?? string.Empty;
        _txtParentOrgType.Text = org.Parent?.Type.ToString() ?? string.Empty;

        // Defaults
        _txtDefaultCurrency.Text = org.DefaultCurrency ?? string.Empty;
        _txtTaxIdType.Text = org.TaxIdType ?? string.Empty;
        _txtTaxId.Text = org.TaxId ?? string.Empty;

        // Physical address
        _txtAddressLine1.Text = org.AddressLine1 ?? string.Empty;
        _txtAddressLine2.Text = org.AddressLine2 ?? string.Empty;
        _txtAddressLine3.Text = org.AddressLine3 ?? string.Empty;
        _txtCity.Text = org.City ?? string.Empty;
        _txtState.Text = org.State ?? string.Empty;
        _txtPostalCode.Text = org.PostalCode ?? string.Empty;
        _txtCountry.Text = org.Country ?? string.Empty;

        // Mailing
        _txtMailingAddressLine1.Text = org.MailingAddressLine1 ?? string.Empty;
        _txtMailingAddressLine2.Text = org.MailingAddressLine2 ?? string.Empty;
        _txtMailingAddressLine3.Text = org.MailingAddressLine3 ?? string.Empty;
        _txtMailingCity.Text = org.MailingCity ?? string.Empty;
        _txtMailingState.Text = org.MailingState ?? string.Empty;
        _txtMailingPostalCode.Text = org.MailingPostalCode ?? string.Empty;
        _txtMailingCountry.Text = org.MailingCountry ?? string.Empty;

        // Contacts
        _txtContactFirstName.Text = org.ContactPersonFirstName ?? string.Empty;
        _txtContactLastName.Text = org.ContactPersonLastName ?? string.Empty;
        _txtContactPersonCountryCode.Text = org.ContactPersonCountryCode ?? string.Empty;
        _txtContactPhone.Text = org.ContactPersonPhone ?? string.Empty;
        _txtContactEmail.Text = org.ContactPersonEmail ?? string.Empty;
        _txtContactJobTitle.Text = org.ContactPersonJobTitle ?? string.Empty;

        _txtBillingPersonFirstName.Text = org.BillingPersonFirstName ?? string.Empty;
        _txtBillingPersonLastName.Text = org.BillingPersonLastName ?? string.Empty;
        _txtBillingPersonCountryCode.Text = org.BillingPersonCountryCode ?? string.Empty;
        _txtBillingPersonPhone.Text = org.BillingPersonPhone ?? string.Empty;
        _txtBillingPersonEmail.Text = org.BillingPersonEmail ?? string.Empty;
        _txtBillingPersonJobTitle.Text = org.BillingPersonJobTitle ?? string.Empty;

        _txtAdminPersonFirstName.Text = org.AdminPersonFirstName ?? string.Empty;
        _txtAdminPersonLastName.Text = org.AdminPersonLastName ?? string.Empty;
        _txtAdminPersonCountryCode.Text = org.AdminPersonCountryCode ?? string.Empty;
        _txtAdminPersonPhone.Text = org.AdminPersonPhone ?? string.Empty;
        _txtAdminPersonEmail.Text = org.AdminPersonEmail ?? string.Empty;
        _txtAdminPersonJobTitle.Text = org.AdminPersonJobTitle ?? string.Empty;

        // Policies
        _txtTravelPolicy.Text = org.DefaultTravelPolicyId ?? string.Empty;
        _txtExpensePolicy.Text = org.DefaultExpensePolicyId ?? string.Empty;

        // License
        _txtLicenseAgreementId.Text =
            license?.Id ?? org.LicenseAgreementId ?? string.Empty;

        if (license is null)
        {
            SetPaymentStatusDisplay(null);
        }
        else
        {
            // Assuming license.PaymentStatus is PaymentStatus enum
            SetPaymentStatusDisplay(license.PaymentStatus);
        }

        _txtLicenseAgreementEndDate.Text = license is null
            ? string.Empty
            : license.ExpiryDate.ToString("yyyy-MM-dd");

        _txtLicenseAgreementRenewalDate.Text = license?.RenewalDate is null
            ? string.Empty
            : license.RenewalDate.Value.ToString("yyyy-MM-dd");

        _txtClientCountLimit.Text = license?.ClientCountLimit?.ToString() ?? string.Empty;
        _txtUserAccountLimit.Text = license?.UserAccountLimit?.ToString() ?? string.Empty;
    }

    
    // ---------------- HTTP + JSON ----------------

    private void LoadOrgFromApi(string orgId)
    {
        if (string.IsNullOrWhiteSpace(orgId))
        {
            MessageBox.Query("Org Lookup", "Org ID is required.", "OK");
            SetState(EditorState.Searching);
            return;
        }

        ClearOrgFields();
        _currentOrg = null;

        try
        {
            var client = _clientFactory.CreateClient(ApiClientName.CinturonApi);

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/v1/tui/org/{orgId}");

            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = client.Send(request);
            var body = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                LogError(
                    $"EditOrgPage.LoadOrgFromApi: HTTP {(int)response.StatusCode} {response.ReasonPhrase}",
                    new HttpRequestException($"Status {(int)response.StatusCode} {response.ReasonPhrase}"),
                    body);

                MessageBox.ErrorQuery(
                    "Org Lookup",
                    "Organisation could not be loaded. Check the Org ID and try again.",
                    "OK");

                SetState(EditorState.Searching);
                return;
            }

            OrgAggregate? aggregate;

            try
            {
                aggregate = JsonSerializer.Deserialize<OrgAggregate>(
                    body,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch (Exception ex)
            {
                LogError("EditOrgPage.LoadOrgFromApi: JSON parse error", ex, body);

                MessageBox.ErrorQuery(
                    "JSON error",
                    "Failed to parse organisation payload.\n\nSee log for details.",
                    "OK");

                SetState(EditorState.Searching);
                return;
            }

            if (aggregate is null || aggregate.Org is null)
            {
                MessageBox.Query(
                    "Org Lookup",
                    "Organisation not found or empty response.",
                    "OK");

                SetState(EditorState.Searching);
                return;
            }

            // Successful load
            _txtOrgId.Text = aggregate.Org.Id;
            PopulateFields(aggregate);
            SetState(EditorState.Viewing);
        }
        catch (Exception ex)
        {
            LogError("EditOrgPage.LoadOrgFromApi: unexpected error", ex, null);

            MessageBox.ErrorQuery(
                "Error",
                "Unexpected error while loading organisation.\n\nSee log for details.",
                "OK");

            SetState(EditorState.Searching);
        }
    }

    // ---------------- Button handlers ----------------

    private void OnEditClicked()
    {
        if (_currentOrg is null)
            return;

        SetState(EditorState.Editing);
    }

    private void OnSaveClicked()
    {
        if (_currentOrg is null)
            return;

        // Placeholder: this is where the save/PUT logic will go.
        // Read values back from the fields and push to the API when implemented.
        MessageBox.Query(
            "Save",
            "Organisation saved (placeholder – API call not yet implemented).",
            "OK");

        // After save: fields readonly, OrgId editable again so user can change ID or press Enter to reload
        SetState(EditorState.Searching);
        _txtOrgId.SetFocus();
    }

    private void OnCancelClicked()
    {
        // Revert any changes in the fields back to the last loaded entity
        if (_currentOrg is not null)
        {
            PopulateFields(_currentOrg);
        }

        // After cancelling, allow OrgId to be updated again
        SetState(EditorState.Searching);
        _txtOrgId.SetFocus();
    }

    // ---------------- file logger ----------------

    private static void LogError(string context, Exception ex, string? body)
    {
        try
        {
            var lines = new System.Collections.Generic.List<string>
            {
                "==============================",
                DateTimeOffset.Now.ToString("O"),
                context,
                "",
                "Exception:",
                ex.ToString(),
                ""
            };

            if (body != null)
            {
                lines.Add("Body:");
                lines.Add(body);
                lines.Add("");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(LogFilePath)!);
            File.AppendAllLines(LogFilePath, lines);
        }
        catch
        {
            // ignore logging failures
        }
    }


    // ---------------- Payment Status Display ----------------
    private void SetPaymentStatusDisplay(PaymentStatus? status)
    {
        if (status is null)
        {
            _txtPaymentStatus.Text = string.Empty;
            _txtPaymentStatus.ColorScheme = _statusNeutralScheme;
            return;
        }

        _txtPaymentStatus.Text = status.Value.ToString();

        ColorScheme scheme = status switch
        {
            // BLUE: informational / in-flight
            PaymentStatus.Pending
            or PaymentStatus.Processing
            or PaymentStatus.Scheduled
            or PaymentStatus.OnHold
                => _statusInfoScheme,

            // GREEN: good
            PaymentStatus.Authorized
            or PaymentStatus.PartiallyPaid
            or PaymentStatus.Paid
            or PaymentStatus.PartiallyRefunded
            or PaymentStatus.Refunded
                => _statusOkScheme,

            // YELLOW: warning
            PaymentStatus.Overdue
            or PaymentStatus.Disputed
                => _statusWarnScheme,

            // RED: bad / terminal
            PaymentStatus.Failed
            or PaymentStatus.Cancelled
            or PaymentStatus.WrittenOff
            or PaymentStatus.Chargeback
            or PaymentStatus.Voided
                => _statusErrorScheme,

            _ => _statusNeutralScheme
        };

        _txtPaymentStatus.ColorScheme = scheme;
    }
}
