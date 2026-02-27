using System;
using System.Collections.Generic;
using System.Linq;
using Cinturon360.Tui.App.Shell;
using Terminal.Gui;

namespace Cinturon360.Tui.App.Pages.Pnr
{
    /// <summary>
    /// Simple "New PNR" category picker.
    /// </summary>
    public sealed class NewPnrPage : PageBase
    {
        private readonly Label _lblHeading;
        private readonly ListView _listView;

        private readonly IReadOnlyList<string> _categories = new[]
        {
            "Flight",
            "Accommodation",
            "Taxi",
            "Train",
            "Hire Car",
            "Bus",
            "eSim Card",
            "Holiday Activity",
            "Mixed Booking"
        };

        public override string Title => "New PNR";

        public NewPnrPage()
        {
            // Heading
            _lblHeading = new Label("Select booking category:")
            {
                X = 1,
                Y = 1
            };

            // List of categories
            _listView = new ListView(_categories.ToList())
            {
                X = 1,
                Y = Pos.Bottom(_lblHeading) + 1,
                Width = Dim.Fill(1),
                Height = Dim.Fill(1),
                CanFocus = true,
                AllowsMarking = false
                // FullRowSelect = true  // <-- remove this, not valid on ListView
            };

            // Enter / double-click handler
            _listView.OpenSelectedItem += args =>
            {
                if (args.Item < 0 || args.Item >= _categories.Count)
                    return;

                var category = _categories[args.Item];
                ShowNewPnrPlaceholder(category);
            };

            _root.Add(_lblHeading, _listView);
        }

        /// <summary>
        /// Placeholder dialog for now – later you can replace this with real navigation
        /// to a dedicated "New {Category} PNR" editor page.
        /// </summary>
        private static void ShowNewPnrPlaceholder(string category)
        {
            const int width = 60;
            const int height = 12;

            var dialog = new Dialog("New PNR", width, height);

            var msg = new Label($"Create a new PNR for:\n\n  {category}")
            {
                X = 2,
                Y = 1
            };

            dialog.Add(msg);

            // You can later add more controls here (fields, hints, etc.)

            var ok = new Button("OK", is_default: true)
            {
                X = Pos.Center(),
                Y = Pos.Bottom(msg) + 2
            };
            ok.Clicked += () => Application.RequestStop();

            dialog.Add(ok);

            Application.Run(dialog);
        }
    }
}
