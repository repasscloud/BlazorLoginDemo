# Cinturon360 v5 — Remaining Phases

> Status as of 27 April 2026  
> Phases 0–7 are fully implemented. Build is passing.  
> Phase 9 is complete ✅ (all planned Web UI pages implemented and build passing).
> Phase 10 is in progress ⚙️ (MailerSend + Stripe + S3/R2 runtime wiring completed; ECB FX sync/cleanup and Duffel config backend implemented; provider transaction flows still pending).

---

## Execution Order

> Phase 8 is ON HOLD — do not start until explicitly re-prioritised.

| Order | Phase | Notes |
|-------|-------|-------|
| 1 | **Phase 10** — Real Integrations | IN PROGRESS — MailerSend, Stripe, and S3/R2 wiring completed; ECB FX sync/cleanup and Duffel config management implemented; Amadeus/Duffel transaction flows, GitHub ticketing hardening, and remaining data jobs pending. |
| 2 | **Phase 11** — Deployment & Infrastructure | ACA Bicep, CI/CD, secrets, environment matrix. |
| 3 | **Phase 12** — Observability & Logging | Serilog, Azure Monitor, alerts. |
| 4 | **Phase 13** — Legacy Reference Cleanup | Move v4 code to `legacy/v4-reference` branch, remove from main. |
| — | **Phase 9** — Web UI | **COMPLETE** — implemented end-to-end and validated with successful solution build. |
| — | **Phase 8** — Mobile (MAUI) | **ON HOLD** — one of the last things we do. |

### Phase 9 — Recommended Build Order (within Phase 9)

1. **Fix logout** ✅ — endpoint-based (`GET /auth/logout-handler`), revokes API session, clears BFF cookie
2. **Auth pages** ✅ — Register (`/auth/register`), ForgotPassword, ResetPassword all wired to API endpoints
   - MFA deferred — API doesn't support MFA challenge yet (no separate MFA endpoint in Phase 0–7)
3. **Profile & account pages** ✅ — profile edit, PAT management, session management
4. **Traveller profile** ✅ — passport, preferences, loyalty, emergency contacts
5. **Organisations** ✅ — org list, create/edit, user assignment, settings
6. **Bookings** ✅ — flight search, results, confirmation, list, detail, cancel
7. **Approvals** ✅ — pending list, detail (approve/reject), history
8. **Travel Policy** ✅ — policy list, create/edit, assignment
9. **Billing** ✅ — dashboard, invoices, top-up, Stripe portal, credit notes
10. **Admin** ✅ — user/org/role management, job queue viewer, audit log, stored documents
11. **UI polish pass** ✅ — final pass complete for this phase scope

---

## Phase 8 — Mobile (MAUI)

> **ON HOLD** — do not start until mobile client is explicitly re-prioritised.  
> Originally deferred per Q10 decision.

- Scaffold `src/Cinturon360.Maui/` project (MAUI Blazor Hybrid or MAUI native)
- OIDC login path (for SSO tenants)
- Internal login path (for tenants that allow direct login)
- 90-day revocable device session (separate token class from web sessions)
- QR-based device linking from an authenticated web profile
- Server-managed session revocation (revocable from web at any time)
- Push MFA method (enabled with mobile)
- Social login: Apple + Facebook (deferred alongside mobile per Q6)
- SCIM provisioning integration (deferred per Q6)

---

## Phase 9 — Web UI (Blazor)

> Server-side Blazor (confirmed in Q2). No WASM.

Pages and components to build:

### Auth & Account
- Login page (password, SSO, MFA challenge, QR device login)
- Register / invite flow
- MFA setup (TOTP, SMS, Email OTP)
- Password reset / recovery codes
- Profile edit (name, timezone, language, currency)
- PAT management (create, list, revoke)
- Session management (view active sessions, revoke)

### Traveller Profile
- Passport details form
- Seat / meal preferences
- Loyalty programs (add, edit, remove)
- Emergency contacts
- Addresses

### Organisations
- Org list + hierarchy tree view
- Create / edit org
- Assign users to org
- Org settings (billing mode, policy assignment)

### Bookings
- Flight search form (Amadeus / Duffel)
- Search results + quote selection
- Booking confirmation flow
- Booking list (filtered by user / org / status)
- Booking detail view
- Cancel booking

### Approvals
- Pending approvals list
- Approval request detail (approve / reject with note)
- Approval history

### Travel Policy
- Policy list per org
- Create / edit policy (cabin class, spend caps, advance booking window, approval thresholds)
- Assign policy to user or org

### Billing
- Org billing dashboard (balance, invoices, payments)
- Invoice list + detail
- Prepaid top-up flow
- Stripe card-on-file management (portal redirect)
- Credit note list

### Admin (Platform / Vendor / TMC)
- User management (create, edit, suspend, lock, role assignment)
- Org management
- Role management
- Job queue viewer (`/admin/jobs`)
- Audit log viewer
- Stored documents list

---

## Phase 10 — Real Integrations

> Stubs were scaffolded in Phase 7. These need real implementation.

### Flights
- **Amadeus** — wire up the existing legacy Amadeus models to the new integration module
  - Flight search (one-way, return, multi-city)
  - Fare pricing
  - Booking / ticketing
  - PNR management
- **Duffel** — secondary flight provider
  - Offer search
  - Order creation
  - Cancellation

Status update:
- Duffel org configuration backend is implemented (TMC-scoped credentials, chain-wide sharing option, capability/search-function flags, effective-config resolution for client orgs via assigned TMC)
- Remaining work is transactional provider API flows (search/order/cancel and related lifecycle operations)

### Email — MailerSend (confirmed Q11)
- Wire `Cinturon360.Infrastructure/Email/` with MailerSend SDK
- Template IDs for: booking confirmation, approval request, approval decision, invoice, password reset, MFA OTP, invite
- Delivery queue via Jobs container

### Stripe (confirmed Q6 / §8)
- Stripe SDK wiring in `Cinturon360.Integrations/Stripe/`
- Create/retrieve Stripe customer per org
- Payment intent creation
- Webhook handler (payment succeeded, failed, disputed)
- Stripe billing portal session (for card management)
- Invoice sync

### Exchange Rates
- Choose FX provider (e.g. Open Exchange Rates, Frankfurter, ECB)
- Scaffold `Cinturon360.Integrations/ExchangeRates/`
- Scheduled refresh job (daily or hourly)

Status update:
- ECB selected and integrated
- Exchange-rate sync job runs every 5 minutes in Jobs worker
- Daily UTC cleanup job removes snapshots older than 24 hours
- Conversion query uses latest DB-stored EUR-base rates

### Geography Reference Data
- Airports, cities, countries data import
- Scheduled refresh job
- Seed initial dataset from IATA or OAG source

### Hotels / Cars / Rail
- Providers TBD
- Scaffold integration modules once providers are chosen

### GitHub Ticketing
- Issue creation on booking error / support request
- Webhook for status sync back to platform

### AWS S3 / Cloudflare R2 (confirmed Q12)
- Wire `Cinturon360.Infrastructure/Storage/` with AWSSDK.S3 (S3-compatible endpoint for R2)
- Signed URL generation for document downloads
- Bucket lifecycle / cleanup job

---

## Phase 11 — Deployment & Infrastructure

> Azure Container Apps confirmed (Q9). PostgreSQL via PlanetScale or Azure DB (Q8).

- `deploy/azure/` — ACA Bicep / ARM / YAML definitions
  - `api` container (1–2 replicas)
  - `web` container (1–3 replicas)
  - `jobs` container (1 replica, Supercronic)
- `deploy/caddy/` — Caddyfile for TLS + reverse proxy (dev / staging)
- Secrets management — Azure Key Vault or ACA secrets
- CI/CD pipeline (GitHub Actions)
  - Build + test on PR
  - Publish Docker images to registry
  - Generate SQL migration scripts
  - Rolling deploy to ACA on merge to main
- `deploy/compose/compose.dev.yaml` — verify final dev stack (api + web + jobs + postgres + pgadmin)
- Environment config matrix: Development, Staging, Production

---

## Phase 12 — Observability & Logging

> Serilog Console sink to stdout, aggregated via Azure Monitor (confirmed Q13).

- Confirm Serilog Console sink is the only production sink (remove PostgreSQL sink from prod config)
- Structured log fields wired throughout: `RequestId`, `UserId`, `OrgId`, `EntityType`, `EntityId`, `DurationMs`
- Azure Monitor / Log Analytics workspace setup
- Alerts: error rate spike, auth failure spike, job failure
- Optional: Datadog integration if Azure Monitor is insufficient

---

## Phase 13 — Legacy Reference Cleanup

> Move v4 code to a separate git branch, remove from main (confirmed Q14).

- Create git branch `legacy/v4-reference`
- Copy `legacy-reference/` folder content to that branch
- Remove `legacy-reference/` from `main`
- Update `.gitignore` and solution to reflect removal

---

## Notes / Open Decisions

| # | Topic | Status |
|---|---|---|
| Hotels provider | No provider chosen yet | Decision needed before Phase 10 |
| Cars / Rail provider | No provider chosen yet | Decision needed before Phase 10 |
| FX provider | Not chosen | Decision needed before Phase 10 |
| SCIM implementation depth | Deferred | Revisit with Phase 8 / enterprise client onboarding |
| SAML 2.0 | Enable early or with SSO phase? | Decision needed — was flagged in Q6 as "enable with SSO phase (unless in early phases, enable immediately)" |
| Multi-region | Not in current plan | Revisit if ACA deployment spans multiple Azure regions |
| Datadog vs Azure Monitor | No decision | Revisit in Phase 12 |
