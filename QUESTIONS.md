# QUESTIONS.md

Questions that require decisions before the next phase of development.
Captured during the initial v5 scaffold session.

---

## Q1 — Auth Server Choice

**Provisionally selected:** OpenIddict 5.4.0

OpenIddict was chosen because it is MIT-licensed, has no per-deployment fee, and integrates directly into the existing EF Core context. Duende IdentityServer requires a commercial license for production.

**Options:**
- (a) **Keep OpenIddict** — recommended, already scaffolded
- (b) Switch to Duende IdentityServer — requires commercial license (~$3,000/yr for production)
- (c) Custom JWT middleware — high maintenance, not recommended

**Decision needed:** Confirm (a) or specify alternative.

**Answer:** Confirming (a) is to be used. Also already scaffolded.

---

## Q2 — Blazor Render Mode

**Currently configured:** Server-side interactivity (`--interactivity Server`)

This means all UI state and rendering runs on the server over a SignalR connection. It simplifies auth and session handling but requires a persistent server connection per user.

**Options:**
- (a) **Server** (current) — recommended for initial build; simplest, no WASM download
- (b) WebAssembly — runs in browser, complicates auth cookie handling
- (c) Auto — server first, then switches to WASM; most complex

**Decision needed:** Confirm (a) or specify alternative.

**Answer:** Confirming (a) - simplest, no WASM download

---

## Q3 — System.Drawing.Common CVE (Critical)

**Package:** `System.Drawing.Common` 5.0.0  
**CVE:** [GHSA-rxg9-xrhp-64gj](https://github.com/advisories/GHSA-rxg9-xrhp-64gj) — **Critical severity**  
**Source:** Transitive dependency from `ITfoxtec.Identity.Saml2` 4.7.0

**Options:**
- (a) Add an explicit `<PackageVersion>` override in `Directory.Packages.props` to force a patched version — investigate what patched version ITfoxtec is compatible with
- (b) **Switch SAML library** to `Sustainsys.Saml2` — actively maintained, may not have this transitive CVE
- (c) Defer — if SAML/SSO login is not in Phase 1 scope, remove ITfoxtec temporarily and add it back when needed

**Decision needed:** Which option? If (a), what is the acceptable patched version?

**Answer:** I have considered this carefully, and reviewed that ITfoxtec.Identity.Saml2.MvcCore or ITfoxtec.Identity.Saml2 v4.17.0 would be more beneficial as it explicitly states it supports dotnet 10. This is being built with dotnet 10 as the underlying framework.

---

## Q4 — AWSSDK.Core CVE (Low)

**Package:** `AWSSDK.Core` 4.0.0  
**CVE:** [GHSA-9cvc-h2w8-phrp](https://github.com/advisories/GHSA-9cvc-h2w8-phrp) — **Low severity**

This is a transitive dependency from `AWSSDK.S3`.

**Options:**
- (a) Accept the low-severity risk and continue with AWSSDK.S3 4.0.0
- (b) Monitor for a patched AWSSDK.Core — no action needed right now
- (c) Switch to Azure Blob Storage instead of AWS S3 — removes AWS dependency entirely (see Q12)

**Decision needed:** Accept (a/b) or switch to Azure Blob (c)?

**Answer:** Stay with S3-compatible storage, but do not accept AWSSDK.Core 4.0.0. Upgrade AWSSDK.S3 so that the transitive AWSSDK.Core version resolves to at least 4.0.3.3 or later.

---

## Q5 — ArchUnitNET Version

**Currently configured:** `TngTech.ArchUnitNET` 0.13.3 (max stable on NuGet)  
**Pre-release available:** 2.1.0-draft  
**Risk:** 0.13.3 was built against .NET 8/9 — it may work on .NET 10 but is not guaranteed.

**Options:**
- (a) **Keep 0.13.3** — use it now, accept it may need upgrading when 0.14.x stable is released
- (b) Use `2.1.0-draft` pre-release — more risk but closer to .NET 10 compatibility
- (c) Defer architecture tests — remove ArchUnitNET for now and add it back in a later phase

**Decision needed:** Which option?

**Answer:** Keep option (a), architecture tests are build/test-only. They do not ship into the Linux container. A stable test dependency is safer than a draft pre-release.If it fails under .NET 10 test execution, remove or upgrade it later.

---

## Q6 — Social Login Providers at Launch

The following providers are scaffolded in `src/Cinturon360.Integrations/IdentityProviders/`:  
`Oidc`, `Saml`, `Scim`, `Google`, `Microsoft`, `Apple`, `Facebook`

**Decision needed:**  
Which of these should be **active at launch** vs. **deferred to a later phase**?  
Suggested minimum: Google + Microsoft (most common for B2B travel).

**Answer:**
Active at launch:
- Microsoft, primary B2B identity provider.
- Google, common business login provider
- OIDC, generic enterprise login foundation (Microsoft Entra ID, Google Workspace, Okta, Auth0, OneLogin, Ping Identity)

Deferred:
- SAML, enable with SSO phase (unless this is in early phases, enable immediately)
- SCIM, enable at later stage
- Apple, enable with mobile application (MAUI)
- Facebook, enable with Apple at later stage for consumer login/account creation

---

## Q7 — Jobs Container Base Image

The `deploy/docker/Dockerfile.jobs` currently uses `mcr.microsoft.com/dotnet/aspnet:10.0` (Debian-based) with supercronic installed at build time.

**Options:**
- (a) **Keep Debian-based** (`aspnet:10.0`) — larger image but familiar and well-supported
- (b) Alpine-based (`aspnet:10.0-alpine`) — smaller image, but musl libc may cause issues with some NuGet packages
- (c) Custom base with supercronic pre-installed

**Decision needed:** Preferred base image for the jobs container?

**Answer:** (a) Keep Debian-based mcr.microsoft.com/dotnet/aspnet:10.0 - for a jobs container, reliability matters more than image size

---

## Q8 — PostgreSQL: Self-Hosted vs. Managed

**Development:** PostgreSQL 17 in Docker Compose (already configured in `deploy/compose/compose.dev.yaml`)  
**Production options:**
- (a) Self-hosted in containers (e.g., inside ACA or AKS)
- (b) **Azure Database for PostgreSQL Flexible Server** — managed, backups, HA, recommended for production
- (c) AWS RDS PostgreSQL — if AWS is preferred cloud

**Decision needed:** What is the production Postgres strategy? This affects connection string config and secrets management.

**Answer:**
- Dev: PostgreSQL 17 in Docker Compose
- Prod: Managed PostgreSQL
- Preferred prod provider: PlanetScale Postgres
- Fallback: Azure Database for PostgreSQL Flexible Server

---

## Q9 — Azure Deployment Target

**Options:**
- (a) **Azure Container Apps (ACA)** — serverless containers, scales to zero, recommended for a multi-service SaaS
- (b) Azure Container Instances (ACI) — simpler but no auto-scaling
- (c) AKS (Kubernetes) — most powerful but operational overhead
- (d) App Service + Web Jobs — traditional but less flexible

**Decision needed:** Which deployment target for production?

**Answer:** Option A — Azure Container Apps.

Reason: The production application should be deployed as containerized .NET 10 services. ACA is the best fit because it supports independent API, web, jobs, and worker containers with managed ingress, secrets, scaling, revisions, and lower operational overhead than AKS. ACI is too limited for production SaaS hosting, AKS is unnecessary operational complexity at this stage, and App Service + Web Jobs is less flexible for a multi-service container-based architecture.

---

## Q10 — MAUI Mobile App Scope

The v5-plan.md includes a `src/Cinturon360.Maui/` project in the long-term plan.

**Decision needed:**  
Is the MAUI mobile app in scope for the **initial v5 build** or a **Phase 5+ deliverable** to be tackled later?

**Answer:**
The MAUI mobile app is for a **Phase 5+ deliverable** to be tackled later.

---

## Q11 — Email Provider

**Observed in legacy code:** MailerSend was used in v4.

**Decision needed:**  
Is MailerSend confirmed for v5? If yes, the `Cinturon360.Infrastructure/Email/` folder should be wired with the MailerSend SDK. If switching providers (Postmark, SendGrid, Resend), specify preference.

**Answer:**
We will proceed using MailerSend, this is the preferred partner for Cinturon360 for transactional email.

---

## Q12 — Object Storage: AWS S3 vs. Azure Blob

**Currently scaffolded:** `AWSSDK.S3` 4.0.0

**Options:**
- (a) **Keep AWS S3** — stay with current scaffold
- (b) Switch to Azure Blob Storage (`Azure.Storage.Blobs`) — aligns with Azure deployment target if Q9 = ACA

**Decision needed:** AWS S3 or Azure Blob Storage?

**Answer:** Per Q4, we will continue with AWS S3, a compatible service (eg Cloudflare R2) will be used in-place of AWS services for cost effectiveness.

---

## Q13 — Serilog PostgreSQL Sink — Same DB or Dedicated?

`Serilog.Sinks.PostgreSQL.Alternative` is in `Directory.Packages.props`.

**Options:**
- (a) Log to a **dedicated `logs` schema** within the application database
- (b) Log to a **separate dedicated logging database** — better isolation, but adds operational complexity
- (c) Log to stdout only (Console sink) and aggregate via a container log collector (e.g., Azure Monitor, Datadog)

**Decision needed:** Where should structured Serilog logs be persisted in production?

**Answer:**

Option C — log to stdout using Serilog Console sink and aggregate logs through the container platform.

Reason: Production logs should not be written into the application PostgreSQL database by default. In a containerized deployment, each service should emit structured logs to stdout/stderr and let Azure Container Apps collect them into Azure Monitor / Log Analytics, or another observability platform such as Datadog. This avoids coupling app database performance and retention to log volume.

The PostgreSQL sink will remain available for development or temporary diagnostics, but it should not be the default production logging target.

---

## Q14 — Legacy v4 Code Reference

All v4 code has been archived to `legacy-reference/` at the root of the repository. This folder is excluded from the v5 build.

**Decision needed:**  
How long should `legacy-reference/` be retained? Options:
- (a) Keep indefinitely as a reference
- (b) Delete once v5 reaches feature parity
- (c) Move to a separate git branch and remove from main

**Answer:**
Decision: Option C — move the v4 legacy reference code to a separate git branch and remove `legacy-reference/` from `main`.

Reason: The v4 code should remain available for reference during the v5 rebuild, but it should not live indefinitely in the active v5 codebase. Keeping it in `main` adds repository noise, pollutes search results, and increases the risk of copying obsolete patterns. A dedicated legacy branch preserves the code while keeping the v5 branch clean.

Once v5 reaches full feature parity and the legacy branch has not been needed for a defined period, the branch can be archived or deleted later if required.

---

---

## Q15 — CSS / UI Framework

The Web project scaffold has Bootstrap 5 loaded by default.

**Options:**
- (a) **Keep Bootstrap 5** — familiar, minimal setup, already present
- (b) **Tailwind CSS** — utility-first, popular with modern Blazor
- (c) **MudBlazor** — Blazor-native Material Design component library with built-in data grids, forms, and dialogs
- (d) **Radzen Blazor** — free component set with data grid, charts, and forms
- (e) Other — specify

**Note:** This must be settled before writing any component, as it affects every page.

**Decision needed:** Which UI framework?

**Answer:**
Use Tailwind CSS as the primary design system, with Fluent UI Blazor used selectively for back-office and enterprise operational screens.

Tailwind should drive the branded product experience, including public pages, onboarding, traveller-facing pages, booking flows, itinerary views, recommendation cards, and modern dashboard surfaces.

Fluent UI Blazor should be used for operational back-office workflows such as admin, support, finance, identity/security settings, audit logs, command bars, dense tables, queues, and case management.

Travel-agent screens should be hybrid: Fluent UI for queue/workbench operations, and Tailwind/custom components for search, booking, itinerary, policy, and traveller-facing workflows.

Area	Primary UI direction	Reason
Public website	Tailwind	Brand, marketing, modern visual identity
Login / onboarding	Tailwind	First impression matters
Traveller / end-user pages	Tailwind	Needs to feel modern, guided, consumer-grade
Booking/search flow	Tailwind	This is core product differentiation
Itinerary / trip timeline	Tailwind	Needs custom travel-specific UX
AI recommendation panels	Tailwind	Needs product-specific visual treatment
Admin console	Fluent UI Blazor	Enterprise controls, tables, command bars
Support desk	Fluent UI Blazor	Queues, cases, notes, status workflows
Finance team	Fluent UI Blazor	Tables, reconciliation, exports, audit trails
Travel agent queue	Hybrid	Fluent for queues/tables, Tailwind for booking/itinerary experience
Reporting dashboards	Hybrid	Tailwind layout, charts/tables as needed

---

## Q16 — Flight Search UI: Live Provider or Stub Data?

The booking pages need a flight search form. The real integrations (Amadeus, Duffel) are planned for Phase 10.

**Options:**
- (a) **Build UI against mock/stub data** — no live provider yet; wire up real calls in Phase 10
- (b) **Wire directly to Amadeus now** — legacy integration can be adapted
- (c) **Wire directly to Duffel now**

**Decision needed:** Should Phase 9 use stubs or connect to a live flight provider?

**Answer:**
Use stubs in Phase 9.

Build the UI against internal flight search contracts, not Amadeus or Duffel directly. Live provider integrations belong in Phase 10.

Phase 9:
- Build the flight search UI
- Use realistic stub data
- Define internal contracts for flight search requests/responses
- Keep provider-specific models out of the UI
- Support search, filters, result cards, fare rules, policy flags, and itinerary preview

Phase 10:
- Add Amadeus provider implementation
- Add Duffel provider implementation if required
- Map provider responses into the same internal flight search contracts

---

## Q17 — Role-Based Navigation Layout

The page folder structure has `Sudo/`, `Vendor/`, `Tmc/`, `Client/` sections. These represent different user role groups.

**Options:**
- (a) **Separate layouts per role group** — different nav/sidebar depending on the user's role (e.g. a TMC user sees a different shell than a Client user)

---

## Q18 — Approval Assignment Semantics

Phase 9 approvals now has a working pending list, detail page, and history page, but the current repository implementation behind `ListPendingForApproverAsync` does **not** actually filter by approver. It currently returns all pending approvals.

This means the UI workflow works technically, but the business rule for "who is allowed to approve what" is still undefined in code.

**Options:**
- (a) Filter by explicit approver assignment table per approval level
- (b) Filter by organisation + role-based approver rules
- (c) Filter by manager chain / org hierarchy
- (d) Keep current broad queue temporarily for vendor/TMC operations, then tighten later

**Decision needed:** Which approval assignment rule should Phase 9 enforce for pending approvals and approval actions?

**Current temporary implementation:** Option (d) in effect, because no approver-assignment model exists yet.
- (b) **One shared layout**, with nav items shown or hidden based on claims/permissions
- (c) **One layout with separate route subtrees** — e.g. `/vendor/…`, `/tmc/…`, `/client/…` — same shell, different route namespaces

**Decision needed:** Which navigation model?

**Answer:**
Decision: Option C — one shared layout with separate route subtrees.

Use one consistent application shell, but keep role-group areas separated by route namespace:

/sudo/...
/vendor/...
/tmc/...
/client/...

Reason: The application should feel like one unified product, not separate applications for each role group. A shared shell keeps branding, account controls, notifications, tenant switching, search, help, and general layout behaviour consistent. Separate route subtrees keep each role group cleanly organised and easier to secure, test, document, and reason about.

Navigation items inside the shared shell should be shown or hidden based on claims and permissions. The route namespace helps organise the application, but it must not be treated as the only security boundary. Access must still be enforced through authorization policies.

Do not create separate layouts per role group unless there is a genuine product-level reason. Do not let Sudo, Vendor, TMC, or Client areas infer different Tailwind or Fluent UI styling. The role/route determines access and navigation visibility, not the CSS framework.

Tailwind should act as the global design system, while Fluent UI Blazor should be available for specific operational components or pages where enterprise controls are needed.

Final model: one shared shell, separate role-based route namespaces, permission-filtered navigation, policy-based authorization, and page/component-level choice of Tailwind or Fluent UI where appropriate.

---

## Q18 — Web Auth State Management

The API owns authentication (JWT). The Blazor app is server-side. How should the Web project maintain auth state between the browser and the API?

**Options:**
- (a) **Cookie-based session** — web app calls the API to log in, stores the JWT in an encrypted server-side cookie, uses a custom `AuthenticationStateProvider` backed by that cookie
- (b) **BFF pattern (Backend for Frontend)** — Web project acts as a thin BFF, proxies API calls server-side, manages the token server-side, never exposes the JWT to the browser
- (c) **Bearer token in Blazor circuit memory** — JWT stored in server memory per SignalR circuit; disappears on reconnect/refresh

**Decision needed:** Which auth state model?

**Answer:**
Option B — BFF pattern.

The Blazor Server Web project should act as a Backend for Frontend. The browser should authenticate to the Web project using a secure, HttpOnly, SameSite cookie. The Web project should manage API access server-side and call the API using server-side token handling.

The JWT must not be exposed to the browser.

The BFF implementation must support production hosting requirements:
- Auth state must survive Web app restarts.
- Auth state must work across multiple Web app instances.
- Token/session storage must therefore use a durable or distributed server-side store, not single-instance memory.
- Data Protection keys must be persisted/shared across instances if cookies or protected tickets are used.
- Logout, token expiry, refresh, and revocation must be handled server-side.

Reason: Blazor Server already runs application logic on the server, so the browser does not need direct access to API bearer tokens. The BFF model gives a cleaner production security posture by keeping API tokens server-side while the browser only holds a secure Web session cookie.

Final model: browser uses a secure cookie to the Blazor Server Web app; Web app resolves the user session from durable/distributed server-side state; Web app calls the API using server-side token handling; API remains the authority for authentication and authorization.

---

## Q19 — Brand & Design Direction

Before building layouts and components, it helps to know what the UI should look and feel like.

**Options / information needed:**
- Existing logo, colour palette, and font choices
- A Figma file or design mockup link
- "Match the legacy v4 look" if there was a recognisable style
- No design yet — build functional first, style later

**Decision needed:** What is the brand/design direction for the Web UI?

**Answer:**
# Q19 — Brand & Design Direction

## Decision

**New modern product design direction with production-ready theming, localisation, and user preferences.**

Do not match the legacy v4 look. The v5 Web UI should move away from legacy WinForms-style business software and should feel like a modern travel SaaS platform competing with products such as Navan and Serko Zeno.

There is no final Figma design yet. The initial direction should be implemented as a functional design system rather than waiting for full static mockups.

The design system must support:

- Light and dark mode
- User-controlled theme switching
- Persisted theme preference per logged-in user
- Language preference per logged-in user
- Application-wide localisation
- User-local date/time entry
- UTC storage for persisted date/time values
- Clear separation between editable application data and read-only provider/search result data

## Logo & Typography

### Logo

The Cinturon360 wordmark is rendered as styled text using:

- **Font:** Plus Jakarta Sans
- **Weight:** 800 (extrabold)
- **Style:** `tracking-tight`, no letter-spacing expansion

| Segment | Colour | Hex | Tailwind Token |
|---|---|---|---|
| Cinturon | Sky Blue | `#38bdf8` | `sky-400` |
| 360 | Vibrant Orange | `#f97316` | `orange-500` |

### Type Scale

| Role | Font | Weight | Notes |
|---|---|---|---|
| Headings | Plus Jakarta Sans | 700 / 800 | `font-heading` Tailwind token |
| Body | Inter | 400 / 500 | `font-sans` Tailwind token |
| Mono | System mono stack | 400 | Code, data, IDs |

Font weights:

```text
Inter: 400, 500, 600, 700
Plus Jakarta Sans: 600, 700, 800
```

Fonts should use an approved delivery strategy and should not create avoidable runtime dependency or compliance risk.

## Colour Palette

The application must support both light and dark themes. Colour tokens should be semantic, not hard-coded directly into components.

### Brand Primaries

| Name | Hex | Usage |
|---|---|---|
| Sky / Cinturon | `#38bdf8` | Primary CTA, links, active states, accents |
| Orange / 360 | `#f97316` | Secondary accent, highlights, badges, brand moments |

Primary actions should usually use Sky. Orange should be reserved for secondary emphasis, highlights, alerts, brand moments, or conversion-oriented CTAs.

### Light Theme Tokens

| Token | Suggested Value | Usage |
|---|---:|---|
| `background` | `#f8fafc` | Main app background |
| `surface` | `#ffffff` | Cards, panels, page sections |
| `surface-muted` | `#f1f5f9` | Secondary sections, table headers |
| `surface-hover` | `#e2e8f0` | Hover and selected row states |
| `border-subtle` | `rgba(15,23,42,0.08)` | Card edges, dividers |
| `border-strong` | `rgba(15,23,42,0.14)` | Inputs, menus, modals |
| `text-primary` | `#0f172a` | Primary headings and body |
| `text-secondary` | `#334155` | Secondary body text |
| `text-muted` | `#64748b` | Hints, placeholders, metadata |
| `text-disabled` | `#94a3b8` | Disabled and tertiary text |

### Dark Theme Tokens

| Token | Suggested Value | Usage |
|---|---:|---|
| `background` | `#05080f` | Main app background |
| `surface` | `#0d1424` | Cards, panels, page sections |
| `surface-muted` | `#111827` | Secondary sections, table headers |
| `surface-hover` | `#1e2a3a` | Hover and selected row states |
| `border-subtle` | `rgba(255,255,255,0.07)` | Card edges, dividers |
| `border-strong` | `rgba(255,255,255,0.12)` | Inputs, menus, modals |
| `text-primary` | `#f8fafc` | Primary headings and body |
| `text-secondary` | `#cbd5e1` | Secondary body text |
| `text-muted` | `#94a3b8` | Hints, placeholders, metadata |
| `text-disabled` | `#64748b` | Disabled and tertiary text |

### Semantic / Status Colours

| Name | Hex | Usage |
|---|---|---|
| Success | `#22c55e` | Approved, confirmed, on-policy |
| Warning | `#f59e0b` | Pending approval, near-limit policy breach |
| Danger | `#ef4444` | Out-of-policy, failed, rejected |
| Info | `#38bdf8` | Informational, in-progress, travel context hints |
| Purple | `#a855f7` | Premium, VIP tier, loyalty level indicators |

### Gradient Usage

The brand gradient combines the two logo primaries and should be used sparingly for feature callouts, hero accents, and selected CTAs:

```css
/* Sky to Orange — brand gradient */
background: linear-gradient(135deg, #38bdf8 0%, #f97316 100%);

/* Light theme sky glow */
background: radial-gradient(ellipse at top, rgba(56,189,248,0.16) 0%, transparent 60%);

/* Dark theme sky glow */
background: radial-gradient(ellipse at top, rgba(56,189,248,0.12) 0%, transparent 60%);
```

## Theme Preference

The application must support a visible light/dark mode switch.

Theme behaviour:

- The logged-in user's saved theme preference is the source of truth.
- The selected theme is stored as part of the user's profile.
- The selected theme must be restored automatically on sign-in.
- When the user changes the theme, the profile preference should be updated.
- The UI should apply the theme consistently across Tailwind-styled components and Fluent UI Blazor components.
- Fluent UI Blazor components must be themed to align with the Cinturon360 design tokens and must not introduce a separate visual language.

Theme preference values:

| Value | Meaning |
|---|---|
| `light` | Force light mode |
| `dark` | Force dark mode |
| `system` | Follow the user's device/browser preference |

Default:

```text
system
```

## Language & Localisation

Language support is required across the application.

The logged-in user's language preference must form part of their user profile.

Language behaviour:

- The user's saved language preference is the source of truth.
- The selected language must be restored automatically on sign-in.
- The user must be able to change language from their profile or account settings.
- Language changes should apply consistently across navigation, labels, validation messages, forms, tables, dialogs, notifications, empty states, errors, and help text.
- UI text must not be hard-coded directly into components where localisation is required.
- Dates, times, numbers, currencies, and formatted values should respect the user's locale where appropriate.
- Provider/search result data should preserve the meaning and context returned by the source provider.

User profile should include:

| Preference | Example |
|---|---|
| Theme | `light`, `dark`, `system` |
| Language | `en-AU`, `en-NZ`, `en-US`, etc. |
| Time zone | `Australia/Sydney`, `Pacific/Auckland`, etc. |
| Locale / culture | `en-AU`, `en-NZ`, etc. |

## Date, Time & Time Zone Behaviour

All editable application date/time entry must be localised from the user's perspective and stored in UTC.

### Editable Application Data

For user-entered or application-owned data:

- Display date/time fields in the user's preferred/local time zone.
- Accept date/time input in the user's preferred/local time zone.
- Convert entered values to UTC before persistence.
- Store persisted date/time values in UTC.
- Convert UTC values back to the user's preferred/local time zone when displaying editable application data.
- Preserve the user's time zone preference as part of their profile.
- Avoid storing ambiguous local-only timestamps for application-owned workflow data.

Examples:

| Scenario | UI Behaviour | Storage Behaviour |
|---|---|---|
| Create approval deadline | User enters local date/time | Store UTC |
| Schedule report | User selects local date/time | Store UTC |
| Add reminder | User enters local date/time | Store UTC |
| Update support SLA due time | User sees local date/time | Store UTC |
| Create internal task | User enters local date/time | Store UTC |

### Read-only Provider/Search Result Data

Flight, hotel, car, itinerary, and booking-provider search results are different.

For provider/search result data:

- Display the local date/time as supplied by the provider or booking context.
- Do not convert provider-local travel times into the user's profile time zone if that would change the travel meaning.
- Treat flight departure/arrival times, hotel check-in/check-out times, and itinerary segment times as location-contextual read-only data.
- Preserve provider/local travel context in the UI.
- Clearly label time zones or location context where ambiguity may exist.

Examples:

| Scenario | UI Behaviour |
|---|---|
| Flight departs Sydney at 10:25 | Show Sydney-local departure time |
| Flight arrives Singapore at 16:40 | Show Singapore-local arrival time |
| Hotel check-in at 15:00 | Show hotel-local check-in time |
| Car pickup at airport local time | Show pickup-location local time |
| Search result fare expiry | Show provider/context time with clear label |

Rule:

```text
Application-owned editable data: user-local input/output, UTC storage.
Provider/search result data: preserve and display travel/provider-local context as read-only.
```

## Design System Direction

### Stack

- **Primary design system:** Tailwind CSS
- **Enterprise / back-office controls:** Fluent UI Blazor, selectively used
- **No Bootstrap**
- **No stale Material admin portal styles**

Any Fluent UI Blazor components must be themed to align with the Cinturon360 tokens and must not introduce a separate visual language.

### UI Principles

- Modern, clean, fast, premium, and travel-focused
- Light mode must be first-class, not an afterthought
- Dark mode must be fully supported and visually complete
- Clear spacing and strong visual hierarchy
- Rounded surfaces: `rounded-2xl`, `rounded-full` for pills
- Cards, timelines, trip summaries, policy badges, guided workflows
- Colour used intentionally, not decoratively, for status, policy state, approval state, alert severity, and travel context
- Accessibility must be considered across contrast, focus states, keyboard navigation, and screen-reader text
- Localisation must be considered when designing component widths, labels, validation messages, and table layouts

### Screen Tiers

| Screen Type | Tone & Feel |
|---|---|
| End-user / Traveller | Guided, consumer-grade, warm, clear, step-by-step |
| Back-office / Admin | Professional, structured, data-dense, enterprise |

### What to Avoid

- Legacy v4 WinForms visual style
- Generic Bootstrap styling
- Stale Material-style admin portal aesthetics
- CRUD-screen-first thinking
- Dark-only design assumptions
- Hard-coded colours inside components
- Hard-coded UI strings that bypass localisation
- Un-themed Fluent UI components that visually clash with the Tailwind design system
- Storing user-entered local date/time values without UTC normalisation

## Brand Voice in UI

- Prioritise clarity and confidence
- Avoid jargon without context
- Labels should describe action outcomes, not technical mechanics
- Policy, approval, and compliance messaging should be firm but not punitive
- Travel context should feel aspirational, clean, and practical
- Localised text should preserve the intended meaning, not just literal wording

## Implementation Path

Build the design system in this order:

1. **Tokens first** — define colours, light/dark theme tokens, type scale, spacing, shadow, radius, status colours, and focus states
2. **User preferences** — persist theme, language, locale, and time zone on the user profile
3. **Localisation foundation** — ensure UI text, validation messages, navigation, notifications, and formatted values are localisable
4. **Date/time foundation** — implement user-local entry and display for editable application data, with UTC persistence
5. **Component primitives** — Button, Badge, Card, Input, Select, DateTimeInput, Modal, Toast, Table
6. **Theme-aware components** — ensure all components support light and dark mode
7. **Layout shells** — App shell, sidebar, top nav, page containers
8. **Page patterns** — Trip summary, policy badge row, approval workflow, reporting card, itinerary timeline
9. **Design documentation** — document the implemented design system in Figma once the first component set stabilises

---

## Q20 — API Communication from Web: HttpClient or Typed Client?

The Web project calls the API over HTTP. The `Services/ApiClients/` folder is scaffolded but empty.

**Options:**
- (a) **Typed `HttpClient` wrappers** — one typed client class per domain (e.g. `BookingApiClient`, `TravellerApiClient`), registered with `IHttpClientFactory`
- (b) **Refit** — interface-based HTTP client generation (`IBookingApi`, `ITravellerApi`) — less boilerplate
- (c) **Generated OpenAPI client** — auto-generate from the API's Swagger spec using NSwag or Kiota

**Decision needed:** How should the Web project communicate with the API?

**Answer:**
Decision: Option A — typed HttpClient wrappers registered with IHttpClientFactory.

The Web project should communicate with the API through explicit typed client classes, grouped by domain, such as BookingApiClient, TravellerApiClient, IdentityApiClient, PolicyApiClient, and ReportingApiClient.

Reason: Typed HttpClient wrappers provide the best balance of clarity, control, testability, and production readiness. They make API communication explicit, allow domain-specific error handling, support BFF server-side token handling, and avoid coupling the Web project too tightly to generated clients or external interface-generation libraries.

The typed clients should be registered with IHttpClientFactory and configured centrally for:

- API base address
- authentication/token forwarding from the BFF layer
- correlation/request IDs
- retries where safe
- timeout policy
- structured logging
- consistent error mapping
- JSON serialisation settings

Refit should not be used as the default because it hides too much of the HTTP behaviour behind generated interface calls and can become awkward when custom error handling, BFF token handling, retries, logging, and response mapping are required.

Generated OpenAPI clients should not be the primary Web-to-API integration model at this stage. They can be useful later for external consumers or SDK generation, but they often introduce noisy generated code and can make the Web project feel coupled to the API surface instead of to stable application-level contracts.

Final model: use one typed API client per domain, registered through IHttpClientFactory, with shared infrastructure for auth, error handling, logging, timeout behaviour, and response mapping.

---

## Q21 — Error & Validation UX

When API calls fail or form validation errors occur, how should errors surface to the user?

**Options:**
- (a) **Inline field errors + toast notifications** for non-critical errors, full error page for unhandled exceptions
- (b) **Modal dialogs** for errors
- (c) **Inline only** — no toasts, all errors shown in context

**Decision needed:** Preferred error/validation UX pattern?

**Answer:**
Decision: Option A — inline field errors + toast notifications for non-critical errors, full error page for unhandled exceptions.

Validation errors should appear inline, next to the relevant field or section, so the user can fix the problem without losing context.

Non-critical API failures should show a toast notification with a clear message and, where appropriate, an action such as Retry, Refresh, or View details.

Unhandled exceptions and unrecoverable failures should show a full error page or error boundary with a friendly explanation, correlation ID, and a safe recovery action.

Reason: This gives the best balance between clarity and usability. Field-level issues are handled exactly where the user needs to act. Temporary or non-blocking failures are visible without interrupting the workflow. Serious failures are separated into a dedicated error experience instead of appearing as confusing inline noise.

Preferred pattern:
- Field validation errors: inline beside the field
- Form-level validation errors: inline summary at the top of the form
- Non-critical API failures: toast notification
- Recoverable API failures: toast with Retry action where possible
- Permission/access failures: dedicated access denied state
- Not found errors: dedicated not found state
- Unhandled exceptions: full error page/error boundary with correlation ID
- Background job/process failures: status banner or notification centre entry

Do not use modal dialogs as the default error pattern. Modals should be reserved for destructive action confirmation, blocking decisions, or cases where the user must choose before continuing.

Do not use inline-only errors for everything, because global API failures, connectivity issues, background failures, and unexpected errors can be missed or placed too far away from the user's current focus.

Final model: inline for validation and context-specific errors, toast notifications for non-critical/recoverable API failures, dedicated states for access/not-found, and full error pages/error boundaries for unhandled exceptions.
---

*Last updated: 25 April 2026 — Phase 9 Web UI questions added (Q15–Q21)*
