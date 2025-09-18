using BlazorLoginDemo.Shared.Services.Interfaces.Kernel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public sealed class PolicyPdfService : IPolicyPdfService
{
    public PolicyPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> GenerateAsync(TravelPolicy p)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(32);
                page.DefaultTextStyle(t => t.FontSize(11));

                // ---------- Header ----------
                page.Header().Column(col =>
                {
                    col.Spacing(4);
                    col.Item().Text("Travel Policy").FontSize(18).SemiBold();

                    col.Item().Text(text =>
                    {
                        text.Span("Policy ID: ").SemiBold();
                        text.Span(p.Id ?? "—");
                        text.Span("    ");
                        text.Span(DateTime.Now.ToString("yyyy-MM-dd")).FontColor(Colors.Grey.Darken2);
                    });
                });

                // ---------- Content ----------
                page.Content().Column(col =>
                {
                    col.Spacing(12);

                    // Summary
                    col.Item().Element(Section("Summary", body =>
                    {
                        body.Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(1);
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                                c.RelativeColumn(2);
                            });

                            AddKeyValueRow(t, "Policy Name", D(p.PolicyName),
                                                "Default Currency", D(p.DefaultCurrencyCode));

                            // CHANGED: "Client" -> "Organization", AvaClientId -> OrganizationUnifiedId
                            AddKeyValueRow(t, "Organization", D(p.OrganizationUnifiedId),
                                                "Cabin Coverage", D(p.CabinClassCoverage?.ToString()));
                        });
                    }));

                    // Flight Rules
                    col.Item().Element(Section("Flight Rules", body =>
                    {
                        body.Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                                c.RelativeColumn();
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });

                            t.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text("Default Cabin").SemiBold();
                                header.Cell().Element(HeaderCell).Text("Max Cabin").SemiBold();
                                header.Cell().Element(HeaderCell).Text("Max Flight Price").SemiBold();
                                header.Cell().Element(HeaderCell).Text("Non-Stop Only").SemiBold();
                                header.Cell().Element(HeaderCell).Text("Cabin Coverage").SemiBold();
                            });

                            t.Cell().Element(BodyCell).Text(D(p.DefaultFlightSeating?.ToString()));
                            t.Cell().Element(BodyCell).Text(D(p.MaxFlightSeating?.ToString()));
                            t.Cell().Element(BodyCell).Text(Money(p.MaxFlightPrice, p.DefaultCurrencyCode));
                            t.Cell().Element(BodyCell).Text(B(p.NonStopFlight));
                            t.Cell().Element(BodyCell).Text(D(p.CabinClassCoverage?.ToString()));
                        });
                    }));

                    // Booking Windows
                    col.Item().Element(Section("Booking Windows", body =>
                    {
                        body.Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });

                            t.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text("From (Local)").SemiBold();
                                header.Cell().Element(HeaderCell).Text("To (Local)").SemiBold();
                                header.Cell().Element(HeaderCell).Text("Days in Advance").SemiBold();
                                header.Cell().Element(HeaderCell).Text("Weekend Allowed").SemiBold();
                            });

                            t.Cell().Element(BodyCell).Text(D(p.FlightBookingTimeAvailableFrom));
                            t.Cell().Element(BodyCell).Text(D(p.FlightBookingTimeAvailableTo));
                            t.Cell().Element(BodyCell).Text(p.DefaultCalendarDaysInAdvanceForFlightBooking?.ToString() ?? "—");
                            t.Cell().Element(BodyCell).Text($"{B(p.EnableSaturdayFlightBookings)} Sat / {B(p.EnableSundayFlightBookings)} Sun");
                        });
                    }));

                    // Airlines
                    col.Item().Element(Section("Airlines", body =>
                    {
                        body.Row(r =>
                        {
                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Included").SemiBold();
                                c.Item().Text(JoinList(p.IncludedAirlineCodes));
                            });

                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Excluded").SemiBold();
                                c.Item().Text(JoinList(p.ExcludedAirlineCodes));
                            });
                        });
                    }));

                    // Geographic Scope
                    col.Item().Element(Section("Geographic Scope", body =>
                    {
                        body.Column(c =>
                        {
                            c.Spacing(6);

                            c.Item().Table(t =>
                            {
                                t.ColumnsDefinition(x =>
                                {
                                    x.RelativeColumn();
                                    x.RelativeColumn();
                                    x.RelativeColumn();
                                });

                                t.Header(header =>
                                {
                                    header.Cell().Element(HeaderCell).Text("Continents").SemiBold();
                                    header.Cell().Element(HeaderCell).Text("Regions").SemiBold();
                                    header.Cell().Element(HeaderCell).Text("Countries").SemiBold();
                                });

                                t.Cell().Element(BodyCell).Text(JoinNames(p.Continents?.Select(x => x.Name)));
                                t.Cell().Element(BodyCell).Text(JoinNames(p.Regions?.Select(x => x.Name)));
                                t.Cell().Element(BodyCell).Text(JoinNames(p.Countries?.Select(x => x.Name)));
                            });

                            c.Item().Text("Disabled Countries").SemiBold();
                            c.Item().Text(JoinNames(p.DisabledCountries?.Select(dc => dc.Country?.Name)));
                        });
                    }));
                });

                // ---------- Footer ----------
                page.Footer().AlignRight().Text(text =>
                {
                    text.Span("Generated ").FontColor(Colors.Grey.Darken2);
                    text.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                });
            });
        });

        var bytes = doc.GeneratePdf();
        return Task.FromResult(bytes);
    }

    // ---------- helpers ----------
    private static string D(string? s) => string.IsNullOrWhiteSpace(s) ? "—" : s;
    private static string D(DateTime? dt) => dt.HasValue ? dt.Value.ToString("yyyy-MM-dd") : "—";
    private static string Money(decimal amount, string? ccy)
        => amount <= 0m ? "—" : $"{(string.IsNullOrWhiteSpace(ccy) ? "" : ccy + " ")}{amount:0.00}";
    private static string B(bool? b) => b.HasValue ? (b.Value ? "Yes" : "No") : "—";

    private static string JoinList(IEnumerable<string>? items)
        => items is null ? "—" : string.Join(", ", items.Where(s => !string.IsNullOrWhiteSpace(s)));

    private static string JoinNames(IEnumerable<string?>? items)
        => items is null ? "—" : string.Join(", ", items.Where(s => !string.IsNullOrWhiteSpace(s)));

    private static IContainer HeaderCell(IContainer c) =>
        c.PaddingVertical(4).PaddingHorizontal(6).DefaultTextStyle(x => x.SemiBold());

    private static IContainer BodyCell(IContainer c) =>
        c.PaddingVertical(3).PaddingHorizontal(6);

    private static void AddKeyValueRow(TableDescriptor t, string k1, string v1, string k2, string v2)
    {
        t.Cell().Element(BodyCell).Text(k1).SemiBold();
        t.Cell().Element(BodyCell).Text(v1);
        t.Cell().Element(BodyCell).Text(k2).SemiBold();
        t.Cell().Element(BodyCell).Text(v2);
    }

    private static Action<IContainer> Section(string title, Action<IContainer> content) => container =>
    {
        container
            .Padding(8)
            .Border(1)
            .BorderColor(Colors.Grey.Lighten3)
            .DefaultTextStyle(s => s.FontSize(10))
            .Column(col =>
            {
                col.Spacing(6);
                col.Item().Text(title).FontSize(14).SemiBold();
                col.Item().Element(content);
            });
    };
}
