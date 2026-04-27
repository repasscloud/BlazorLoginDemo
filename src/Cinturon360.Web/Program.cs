using Cinturon360.Web.Components;
using Cinturon360.Web.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.FluentUI.AspNetCore.Components;
using Cinturon360.Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// ── Razor Components (Blazor Server) ──────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ── Fluent UI Blazor ───────────────────────────────────────────────────────
builder.Services.AddFluentUIComponents();

// ── Web services: auth, API clients, state, localisation ─────────────────
builder.Services.AddWebServices(builder.Configuration);

// ── Data Protection ────────────────────────────────────────────────────────
// Keys are persisted to a mounted volume so they survive container restarts.
// The path is overridden via DP_KEYS_PATH env var; defaults to /app/dp-keys.
var dpKeysPath = builder.Configuration["DP_KEYS_PATH"] ?? "/app/dp-keys";
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new System.IO.DirectoryInfo(dpKeysPath))
    .SetApplicationName("Cinturon360.Web");

var app = builder.Build();

// ── Error handling ─────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// ── Auth ───────────────────────────────────────────────────────────────────
app.UseAuthentication();
app.UseAuthorization();

// ── Localisation ───────────────────────────────────────────────────────────
var supportedCultures = new[] { "en-AU", "en-NZ", "en-US", "en-GB" };
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("en-AU")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures));

app.UseAntiforgery();

// ── Static files ───────────────────────────────────────────────────────────
app.MapStaticAssets();

// ── API Endpoints ──────────────────────────────────────────────────────────
app.MapAuthEndpoints();

// ── Blazor ─────────────────────────────────────────────────────────────────
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

