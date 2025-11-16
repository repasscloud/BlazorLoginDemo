using System.Net.Http.Headers;
using System.Text.Json;
using Cinturon360.Tui.App.Infrastructure.Http;
using Cinturon360.Tui.App.Shell;
using Terminal.Gui;
using static Cinturon360.Shared.Services.Interfaces.Platform.IAdminOrgServiceUnified;

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
    private readonly TextField _txtLastValidatedDate;

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

    // Tenant Domains
    private readonly TextField _txtTenantDomains;

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

        // --- OrgId row ---
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

        // --- Buttons ---
        _btnEdit = new Button("Edit")
        {
            X = 1,
            Y = Pos.Bottom(_lblOrgId) + 2
        };
        _btnEdit.Clicked += OnEditClicked;

        _btnSave = new Button("Save")
        {
            X = Pos.Right(_btnEdit) + 2,
            Y = _btnEdit.Y
        };
        _btnSave.Clicked += OnSaveClicked;

        _btnCancel = new Button("Cancel")
        {
            X = Pos.Right(_btnSave) + 2,
            Y = _btnEdit.Y
        };
        _btnCancel.Clicked += OnCancelClicked;

        // --- Basic org fields (placeholder set – expand later) ---
        var lblName = new Label("Name:")
        {
            X = 1,
            Y = Pos.Bottom(_btnEdit) + 2
        };
        _txtName = new TextField("")
        {
            X = Pos.Right(lblName) + 1,
            Y = Pos.Top(lblName),
            Width = 40,
            ReadOnly = true
        };

        var lblType = new Label("Type:")
        {
            X = 1,
            Y = Pos.Bottom(lblName) + 1
        };
        _txtType = new TextField("")
        {
            X = Pos.Right(lblType) + 1,
            Y = Pos.Top(lblType),
            Width = 40,
            ReadOnly = true
        };

        var lblCountry = new Label("Country:")
        {
            X = 1,
            Y = Pos.Bottom(lblType) + 1
        };
        _txtCountry = new TextField("")
        {
            X = Pos.Right(lblCountry) + 1,
            Y = Pos.Top(lblCountry),
            Width = 40,
            ReadOnly = true
        };

        var lblTaxId = new Label("Tax ID:")
        {
            X = 1,
            Y = Pos.Bottom(lblCountry) + 1
        };
        _txtTaxId = new TextField("")
        {
            X = Pos.Right(lblTaxId) + 1,
            Y = Pos.Top(lblTaxId),
            Width = 40,
            ReadOnly = true
        };

        var lblContactFirst = new Label("Contact First:")
        {
            X = 1,
            Y = Pos.Bottom(lblTaxId) + 1
        };
        _txtContactFirstName = new TextField("")
        {
            X = Pos.Right(lblContactFirst) + 1,
            Y = Pos.Top(lblContactFirst),
            Width = 40,
            ReadOnly = true
        };

        var lblContactLast = new Label("Contact Last:")
        {
            X = 1,
            Y = Pos.Bottom(lblContactFirst) + 1
        };
        _txtContactLastName = new TextField("")
        {
            X = Pos.Right(lblContactLast) + 1,
            Y = Pos.Top(lblContactLast),
            Width = 40,
            ReadOnly = true
        };

        var lblContactEmail = new Label("Contact Email:")
        {
            X = 1,
            Y = Pos.Bottom(lblContactLast) + 1
        };
        _txtContactEmail = new TextField("")
        {
            X = Pos.Right(lblContactEmail) + 1,
            Y = Pos.Top(lblContactEmail),
            Width = 40,
            ReadOnly = true
        };

        _root.Add(
            _lblOrgId, _txtOrgId,
            _btnEdit, _btnSave, _btnCancel,
            lblName, _txtName,
            lblType, _txtType,
            lblCountry, _txtCountry,
            lblTaxId, _txtTaxId,
            lblContactFirst, _txtContactFirstName,
            lblContactLast, _txtContactLastName,
            lblContactEmail, _txtContactEmail
        );

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
        _txtName.ReadOnly = readOnly;
        _txtType.ReadOnly = true; // keep type read-only for now
        _txtCountry.ReadOnly = readOnly;
        _txtTaxId.ReadOnly = readOnly;
        _txtContactFirstName.ReadOnly = readOnly;
        _txtContactLastName.ReadOnly = readOnly;
        _txtContactEmail.ReadOnly = readOnly;
    }

    private void ClearOrgFields()
    {
        _txtName.Text = string.Empty;
        _txtType.Text = string.Empty;
        _txtCountry.Text = string.Empty;
        _txtTaxId.Text = string.Empty;
        _txtContactFirstName.Text = string.Empty;
        _txtContactLastName.Text = string.Empty;
        _txtContactEmail.Text = string.Empty;
    }

    private void PopulateFields(OrgAggregate aggregate)
    {
        _currentOrg = aggregate;

        var org = aggregate.Org;

        _txtName.Text = org.Name ?? string.Empty;
        _txtType.Text = org.Type.ToString();
        _txtCountry.Text = org.Country ?? string.Empty;
        _txtTaxId.Text = org.TaxId ?? string.Empty;
        _txtContactFirstName.Text = org.ContactPersonFirstName ?? string.Empty;
        _txtContactLastName.Text = org.ContactPersonLastName ?? string.Empty;
        _txtContactEmail.Text = org.ContactPersonEmail ?? string.Empty;
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
        // For now we just show a confirmation and return to "searching" so OrgId can be changed.
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
}
