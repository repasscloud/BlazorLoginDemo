# ADR-0002: Use Blazor Server (no WebAssembly)

**Status:** Accepted  
**Date:** 2026-05-10  
**Deciders:** Cinturon360 engineering team

## Context

The Cinturon360 front-end must be built in a technology consistent with the .NET 10 stack. Three Blazor hosting models were available: Blazor Server, Blazor WebAssembly (WASM), and Blazor United (Auto mode, new in .NET 8). `QUESTIONS.md Q2` captured this decision point.

The platform targets corporate back-office users and travel managers on desktop browsers behind a stable corporate network. Offline support, sub-second cold-start, and public CDN-distributed assets are not requirements for v5.

Evidence of the current implementation:
- `src/Cinturon360.Web/Program.cs:11–14` — `AddRazorComponents().AddInteractiveServerComponents()` with no WASM or Auto registration
- No `wwwroot/` WASM bootstrapper, no `_framework/` asset pipeline
- `Microsoft.FluentUI.AspNetCore.Components` is registered on the server side

## Decision

Blazor Server with interactive server-side rendering is the UI hosting model for `Cinturon360.Web`. WebAssembly and Auto mode are explicitly deferred.

Supporting choices:
- Component library: Microsoft Fluent UI for Blazor (`AddFluentUIComponents()`)
- CSS: Tailwind CSS, built by an MSBuild `BeforeTargets="Build"` step (`npm run build:css`)
- Auth state: `BffAuthStateProvider` (server-side cookie-backed; see ADR-0003)
- Data Protection keys: persisted to `/app/dp-keys` mounted volume (not EF Core store, despite `Microsoft.AspNetCore.DataProtection.EntityFrameworkCore` being referenced)

## Consequences

**Positive**
- Server-side rendering means the full .NET runtime and EF Core are always available in component code.
- No WASM download penalty; initial load is fast for authenticated users.
- SignalR circuit provides real-time update capability without a separate WebSocket layer.
- Simpler auth: the BFF cookie never leaves the server; the browser never sees the API JWT.

**Negative / Trade-offs**
- Each active user holds a server-side SignalR circuit; horizontal scaling requires sticky sessions or a distributed backplane.
- WASM cannot be added later without refactoring component render modes throughout the page tree.
- Blazor Server requires a reliable low-latency network connection; mobile users on 4G may experience circuit drops.

**Required follow-up**
- Decide on SignalR backplane (Azure SignalR Service or Redis) before the first production deployment.
- Remove the `Microsoft.AspNetCore.DataProtection.EntityFrameworkCore` NuGet reference if the file-system key storage approach is permanent (or wire it properly for a distributed deployment).
- Revisit WASM for the traveller-facing flight search (`Client/Travel/FlightSearch`) if mobile offline becomes a requirement.
