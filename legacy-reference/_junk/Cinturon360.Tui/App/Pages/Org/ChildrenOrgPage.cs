using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Cinturon360.Tui.App.Infrastructure.Http;
using Cinturon360.Tui.App.Shell;
using Terminal.Gui;
using static Cinturon360.Shared.Services.Interfaces.Platform.IAdminOrgServiceUnified;

namespace Cinturon360.Tui.App.Pages.Org;

public sealed class ChildrenOrgPage : PageBase
{
    private readonly IApiClientFactory _clientFactory;
    private readonly IPageNavigator _navigator;

    private readonly Label _lblParentOrgId;
    private readonly TextField _txtParentOrgId;

    private readonly TableView _tableView;
    private DataTable _table = new();

    private const string ParentOrgId = "cp4q1IdaNIKFHOGQIs5-t";

    private readonly List<OrganizationPickerDto> _orgs = new();

    private bool _loadedOnce;

    private static readonly string LogFilePath =
        Path.Combine(AppContext.BaseDirectory, "cinturon360-tui-errors.log");

    public override string Title => "Children Org";

    public ChildrenOrgPage(IApiClientFactory clientFactory, IPageNavigator navigator)
    {
        _clientFactory = clientFactory;
        _navigator = navigator;

        _lblParentOrgId = new Label("Parent Org ID:")
        {
            X = 1,
            Y = 1
        };

        _txtParentOrgId = new TextField(ParentOrgId)
        {
            X = Pos.Right(_lblParentOrgId) + 1,
            Y = Pos.Top(_lblParentOrgId),
            Width = 40,
            ReadOnly = true
        };

        _tableView = new TableView
        {
            X = 1,
            Y = Pos.Bottom(_lblParentOrgId) + 1,
            Width = Dim.Fill(1),
            Height = Dim.Fill(),
            FullRowSelect = true
        };

        _tableView.CellActivated += _ => OpenSelectedOrg();

        _root.Add(_lblParentOrgId, _txtParentOrgId, _tableView);
    }

    public override void OnActivated()
    {
        base.OnActivated();

        if (_loadedOnce)
            return;

        _loadedOnce = true;

        try
        {
            LoadChildrenSync();
        }
        catch (Exception ex)
        {
            LogError("ChildrenOrgPage.OnActivated/LoadChildrenSync", ex, null);
            MessageBox.ErrorQuery(
                "Error",
                $"Failed to load child organisations.\n\nSee log:\n{LogFilePath}",
                "OK");
        }
    }

    // ---------------- HTTP + JSON ----------------

    private void LoadChildrenSync()
    {
        var client = _clientFactory.CreateClient(ApiClientName.CinturonApi);

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/v1/tui/org/list/{ParentOrgId}");

        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = client.Send(request);
        var body = response.Content.ReadAsStringAsync().Result;

        if (!response.IsSuccessStatusCode)
        {
            LogError(
                $"ChildrenOrgPage.LoadChildrenSync: HTTP {(int)response.StatusCode} {response.ReasonPhrase}",
                new HttpRequestException($"Status {(int)response.StatusCode} {response.ReasonPhrase}"),
                body);

            MessageBox.ErrorQuery(
                "HTTP error",
                $"Status: {(int)response.StatusCode} {response.ReasonPhrase}\n\nSee log:\n{LogFilePath}",
                "OK");

            BuildEmptyTable();
            return;
        }

        IReadOnlyList<OrganizationPickerDto>? orgs;

        try
        {
            orgs = JsonSerializer.Deserialize<IReadOnlyList<OrganizationPickerDto>>(
                body,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch (Exception ex)
        {
            LogError("ChildrenOrgPage.LoadChildrenSync: JSON parse error", ex, body);

            MessageBox.ErrorQuery(
                "JSON error",
                $"Failed to parse organisation list.\n\nSee log:\n{LogFilePath}",
                "OK");

            BuildEmptyTable();
            return;
        }

        _orgs.Clear();

        if (orgs is null || orgs.Count == 0)
        {
            MessageBox.Query("Children Org", "No child organisations found.", "OK");
            BuildEmptyTable();
            return;
        }

        _orgs.AddRange(orgs);
        BuildOrgTable();
    }

    // ---------------- DataTable / TableView ----------------

    private void BuildEmptyTable()
    {
        _table = new DataTable();
        _table.Columns.Add("Id", typeof(string));
        _table.Columns.Add("Name", typeof(string));
        _table.Columns.Add("Type", typeof(string));
        _table.Columns.Add("Active", typeof(string));
        _table.Columns.Add("Country", typeof(string));

        _tableView.Table = _table;
        _tableView.SelectedRow = _table.Rows.Count > 0 ? 0 : -1;
    }

    private void BuildOrgTable()
    {
        _table = new DataTable();

        _table.Columns.Add("Id", typeof(string));
        _table.Columns.Add("Name", typeof(string));
        _table.Columns.Add("Type", typeof(string));
        _table.Columns.Add("Active", typeof(string));
        _table.Columns.Add("Country", typeof(string));

        foreach (var org in _orgs)
        {
            _table.Rows.Add(
                org.Id,
                org.Name,
                org.Type.ToString(),
                org.IsActive ? "true" : "false",
                org.Country);
        }

        _tableView.Table = _table;
        _tableView.SelectedRow = _table.Rows.Count > 0 ? 0 : -1;
    }

    // ---------------- Details popup ----------------

    private void OpenSelectedOrg()
    {
        var rowIndex = _tableView.SelectedRow;
        if (rowIndex < 0 || rowIndex >= _orgs.Count)
            return;

        var org = _orgs[rowIndex];
        ShowOrgDetails(org);
    }

    private static string S(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "-" : value;

    private void ShowOrgDetails(OrganizationPickerDto org)
    {
        var sb = new StringBuilder();

        void L(string line = "") => sb.AppendLine(line);

        L($"Id:        {org.Id}");
        L($"Name:      {org.Name}");
        L($"Type:      {org.Type}");
        L($"Active:    {org.IsActive}");
        L();

        L("Contact:");
        L($"  Name:    {S(org.ContactPersonFirstName)} {S(org.ContactPersonLastName)}");
        L($"  Email:   {S(org.ContactPersonEmail)}");
        L($"  Phone:   {S(org.ContactPersonPhone)}");
        L();

        L("Billing:");
        L($"  Name:    {S(org.BillingPersonFirstName)} {S(org.BillingPersonLastName)}");
        L($"  Email:   {S(org.BillingPersonEmail)}");
        L($"  Phone:   {S(org.BillingPersonPhone)}");
        L();

        L("Admin:");
        L($"  Name:    {S(org.AdminPersonFirstName)} {S(org.AdminPersonLastName)}");
        L($"  Email:   {S(org.AdminPersonEmail)}");
        L($"  Phone:   {S(org.AdminPersonPhone)}");
        L();

        L($"Tax ID:    {S(org.TaxId)}");
        L($"Country:   {S(org.Country)}");

        var msg = sb.ToString();

        const int width = 70;
        const int height = 20;

        var dialog = new Dialog("Organization Details", width, height);

        var textView = new TextView
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(1),
            Height = Dim.Fill(3), // leave room for buttons
            ReadOnly = true,
            WordWrap = false,
            Text = msg
        };

        dialog.Add(textView);

        var ok = new Button("OK", is_default: true)
        {
            X = Pos.Center() - 6,
            Y = Pos.Bottom(textView) + 1
        };
        ok.Clicked += () => Application.RequestStop(dialog);

        var edit = new Button("Edit")
        {
            X = Pos.Right(ok) + 2,
            Y = ok.Y
        };
        edit.Clicked += () =>
        {
            // Close the dialog first
            Application.RequestStop(dialog);

            // Navigate to the edit page for this org
            var editPage = new EditOrgPage(_clientFactory, org.Id);
            var title = $"Edit Org: {org.Name}";
            _navigator.ShowPage(editPage, title);
        };

        dialog.Add(ok, edit);

        Application.Run(dialog);
    }


    // ---------------- file logger ----------------

    private static void LogError(string context, Exception ex, string? body)
    {
        try
        {
            var lines = new List<string>
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
