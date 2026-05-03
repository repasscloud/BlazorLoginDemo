# Cinturon360 v5 — Feature Backlog & Deferred Items

> Last updated: 4 May 2026  
> This file tracks everything that is intentionally deferred, partially implemented, or out of scope for the current build phase.  
> Use this as the source of truth when raising GitHub Issues.  
> Format: `[area] short title — detail`

---

## How to use this file

Each section represents a feature area. Items marked **🔴 missing** have no implementation at all. Items marked **🟡 stub** have an interface/skeleton but no real logic. Items marked **🔵 partial** are implemented but have known gaps.

Raise one GitHub Issue per bullet point. Suggested labels are noted inline as `[label]`.

---

## Auth & Identity

### MFA (Multi-Factor Authentication)
> Domain entities (`UserMfaMethod`, `UserRecoveryCode`) and DB migration are in place. No command handlers or API endpoints exist.

- 🔴 `POST /api/v1/auth/mfa/setup` — enrol a TOTP or Email OTP device `[auth] [mfa]`
- 🔴 `POST /api/v1/auth/mfa/verify` — MFA challenge endpoint called after password check returns `mfa_required` `[auth] [mfa]`
- 🔴 `POST /api/v1/auth/mfa/disable` — remove MFA from account with re-authentication `[auth] [mfa]`
- 🔴 `POST /api/v1/auth/mfa/recovery` — validate a recovery code in lieu of MFA `[auth] [mfa]`
- 🔴 SMS OTP delivery via MailerSend or Twilio `[auth] [mfa] [email]`
- 🔴 Push MFA method (mobile only — deferred with Phase 8) `[auth] [mfa] [mobile]`
- 🔵 Login flow does not branch on `IsMfaEnabled` — login succeeds even if MFA is enrolled `[auth] [mfa]`
  - `LoginCommandHandler.cs:67` has `// TODO: resolve permissions from role assignments`
  - MFA check branch needs to be inserted before issuing the JWT

### SSO / External Identity Providers
> All IdP modules in `src/Cinturon360.Integrations/IdentityProviders/` are empty scaffolds.

- 🔴 OIDC tenant configuration + callback flow `[auth] [sso] [oidc]`
- 🔴 SAML 2.0 tenant configuration + assertion processing `[auth] [sso] [saml]`
- 🔴 SCIM 2.0 provisioning endpoint (user/group sync from IdP) `[auth] [scim] [provisioning]`
- 🔴 Google social login `[auth] [social]`
- 🔴 Microsoft social login `[auth] [social]`
- 🔴 Apple social login (deferred with Phase 8 per Q6) `[auth] [social] [mobile]`
- 🔴 Facebook social login (deferred with Phase 8 per Q6) `[auth] [social] [mobile]`

### Sessions & Devices
- 🔴 QR-based device linking — one-time-use, short-lived QR tied to authenticated session `[auth] [device]`
- 🔴 Mobile device session (90-day revocable, device-scoped token class) `[auth] [device] [mobile]`
- 🔵 `UserSession` and `UserApiToken` revocation endpoints exist, but session cleanup job is missing `[auth] [jobs]`

### Permission Engine
- 🔵 `AuthorizationBehavior.cs` is a no-op placeholder — permission codes are defined but not enforced in the MediatR pipeline `[auth] [permissions]`
- 🔵 JWT `permissions` claim is not populated from `RolePermissions` — only role name is in the token `[auth] [permissions]`
- 🔴 Permission code evaluation in API endpoints — currently all auth is role-string comparison `[auth] [permissions]`

---

## Flights — Amadeus

> No Amadeus integration module exists in `src/Cinturon360.Integrations/Flights/`. The legacy reference is in `legacy-reference/Cinturon360.Api/`.

- 🔴 Scaffold `src/Cinturon360.Integrations/Flights/Amadeus/` integration module `[flights] [amadeus]`
- 🔴 Amadeus OAuth2 client credential flow (token refresh) `[flights] [amadeus]`
- 🔴 Flight search — one-way, return, multi-city `[flights] [amadeus]`
- 🔴 Fare pricing / repricing `[flights] [amadeus]`
- 🔴 Booking / ticketing (order creation) `[flights] [amadeus]`
- 🔴 PNR management (retrieve, cancel, modify) `[flights] [amadeus]`
- 🔴 `AmadeusConfigResolver` — same pattern as `DuffelConfigResolver`, reads from org config `[flights] [amadeus]`
- 🔴 Amadeus org configuration domain entity + EF config + API endpoints `[flights] [amadeus] [config]`

---

## Flights — Duffel (deferred items)

> Core flows (search, create, cancel) are implemented. The following are intentionally deferred.

- 🔴 Order change / rebooking — `PATCH /air/orders/{id}` or Duffel order change flow `[flights] [duffel]`
- 🔴 Seat selection add-ons — `POST /air/seat_maps` and add-on services `[flights] [duffel]`
- 🔴 Baggage upsell — `GET /air/offers/{id}/available_services`, add services to order `[flights] [duffel]`
- 🔴 Webhooks from Duffel — order status push events (`order.updated`, `order_cancellation.succeeded`, etc.) `[flights] [duffel] [webhooks]`
- 🟡 Quote snapshot currently stores the full offers JSON blob — a structured `DuffelOfferSnapshot` table would improve query performance for large result sets `[flights] [duffel] [data-model]`

---

## Hotels

> Module directory `src/Cinturon360.Integrations/Hotels/` is an empty scaffold.

- 🔴 Choose hotel provider (Duffel Hotels, Amadeus, other TBD) `[hotels] [decision]`
- 🔴 Hotel search integration `[hotels]`
- 🔴 Hotel booking / order creation `[hotels]`
- 🔴 Hotel cancellation `[hotels]`
- 🔴 Hotel org configuration entity + API endpoints `[hotels] [config]`

---

## Cars

> Module directory `src/Cinturon360.Integrations/Cars/` is an empty scaffold.

- 🔴 Choose car rental provider (TBD) `[cars] [decision]`
- 🔴 Car search, booking, cancellation `[cars]`

---

## Rail

> Module directory `src/Cinturon360.Integrations/Rail/` is an empty scaffold.

- 🔴 Choose rail provider (TBD) `[rail] [decision]`
- 🔴 Rail search, booking, cancellation `[rail]`

---

## Email — MailerSend

> `MailerSendEmailService` is implemented and wired. `SendAsync` (raw HTML) and `SendTemplatedAsync` are functional. No handlers call `SendTemplatedAsync` except password reset (plain send) and ticket notifications. Real template IDs are not configured.

- 🔴 Booking confirmation email — call `SendTemplatedAsync` from `ConfirmBookingHandler` `[email] [bookings]`
- 🔴 Approval request email — notify approver when booking enters `PendingApproval` `[email] [approvals]`
- 🔴 Approval decision email — notify traveller/booker when approved or rejected `[email] [approvals]`
- 🔴 Invoice email — send invoice PDF link when invoice is generated `[email] [billing]`
- 🔴 MFA OTP email — send OTP code during MFA challenge `[email] [auth] [mfa]`
- 🔴 Invite email — send invite link when user is created by admin `[email] [auth]`
- 🔴 MailerSend template IDs configuration — document and wire up real template IDs from MailerSend dashboard `[email] [config]`
- 🔴 Email delivery job — async email queue via Jobs container (currently all email is synchronous inline) `[email] [jobs]`

---

## Stripe / Payments

> Provider-neutral billing architecture is implemented. `PaymentProviderConnection`, `BillingRelationship`, `ProviderCustomer`, `ProviderPaymentMethod`, setup-link flow, connection-scoped webhooks, and webhook idempotency are all in place. Stripe webhook signature verification is implemented in the new connection-scoped handlers. The following gaps remain.

- ✅ `ConfigureStripeProviderConnectionCommand` — done
- ✅ Stripe webhook endpoint — done (connection-scoped + org-scoped routes with idempotency)
- ✅ Stripe webhook signature verification — done in connection-scoped handlers
- ✅ Billing setup link (Stripe setup-mode checkout) — done
- ✅ Provider payment method sync — done
- ✅ Relationship-based prepaid top-up intent creation + confirmation — done
- ✅ `SetPrimaryProviderConnectionCommand` + `PUT .../provider-connections/{id}/primary` — done
- 🔴 Invoice/payment collection orchestration — no command creates charges from `BillingInvoice` records `[billing] [stripe]`
- 🔴 Stripe billing portal session — `POST /api/v1/billing/stripe-portal-session` for card management redirect `[billing] [stripe]`
- 🔴 Invoice sync — sync Stripe invoice status back to internal `BillingInvoice` entity `[billing] [stripe]`
- 🔴 Refund flow — trigger `RefundAsync` from a cancellation command and record refund on booking `[billing] [stripe]`
- 🔴 Idempotency key support on Stripe payment intent creation `[billing] [stripe]`

---

## S3 / Cloudflare R2 Storage

> `S3StorageService` is implemented with upload, download, delete, and signed URL generation. `IStorageService` is defined. **No Application layer commands use `IStorageService` yet.**

- 🔴 PDF generation — generate booking confirmation, invoice, and receipt PDFs server-side `[storage] [pdf]`
- 🔴 Upload PDF to S3 and create `StoredDocument` record `[storage] [pdf]`
- 🔴 `GET /api/v1/documents/{id}/download` — return a short-lived signed URL `[storage]`
- 🔴 Storage cleanup job — delete expired `StoredDocument` records and their S3 objects `[storage] [jobs]`
- 🔴 Bucket lifecycle policy — configure R2/S3 lifecycle rules for automatic expiry of temp objects `[storage] [infra]`
- 🔴 Ticket attachment upload — wire `IStorageService` for ticket attachment files `[storage] [ticketing]`

---

## Geography Reference Data

> `GeographyEntities.cs` (Airport, City, Country) domain entities exist. No import job or seed data.

- 🔴 Airport / city / country import job from IATA or OAG dataset `[geography] [jobs]`
- 🔴 Initial seed migration with IATA airport data `[geography] [data]`
- 🔴 `GET /api/v1/geography/airports?search=SYD` — airport typeahead endpoint `[geography] [api]`
- 🔴 `GET /api/v1/geography/cities` — city list endpoint `[geography] [api]`
- 🔴 `GET /api/v1/geography/countries` — country list endpoint `[geography] [api]`
- 🔴 Geography refresh job — scheduled periodic re-import from upstream source `[geography] [jobs]`

---

## GitHub Ticketing

> `GitHubIssuesClient` and `IGitHubTicketingService` are implemented and used in ticketing command handlers. The following are missing.

- 🔴 Webhook receiver — `POST /api/v1/webhooks/github` for status sync back from GitHub `[ticketing] [github] [webhooks]`
- 🔴 Webhook HMAC-SHA256 signature verification on the receiver endpoint `[ticketing] [github] [security]`
- 🔵 GitHub issue creation is called on ticket create/comment/escalate — but there is no retry job if the GitHub call fails transiently `[ticketing] [github] [resilience]`
- 🔴 Sync job — periodic reconciliation between internal ticket status and GitHub issue state `[ticketing] [github] [jobs]`

---

## Background Jobs

> `ExchangeRateSyncJob` and `ExchangeRateCleanupJob` are implemented. All other job slots are empty.

- 🔴 Session expiry cleanup job — delete expired `UserSession` and `UserApiToken` rows `[jobs] [auth]`
- 🔴 Booking expiry job — mark `Draft` bookings as `Expired` if not confirmed within TTL `[jobs] [bookings]`
- 🔴 Quote expiry job — mark `Active` quotes as `Expired` after `ExpiresAt` `[jobs] [bookings]`
- 🔴 PDF generation job — async queue for booking confirmation / invoice PDFs `[jobs] [pdf] [storage]`
- 🔴 Email delivery queue job — async retry for failed email sends `[jobs] [email]`
- 🔴 Geography refresh job — re-import airport/city/country data on schedule `[jobs] [geography]`
- 🔴 Invoice generation job — generate monthly/quarterly invoices per org billing cycle `[jobs] [billing]`
- 🔴 GitHub ticket sync job — reconcile local ticket status vs GitHub `[jobs] [ticketing] [github]`
- 🔴 Stripe invoice sync job — pull Stripe invoice status back to local records `[jobs] [billing] [stripe]`
- 🔴 Storage cleanup job — delete expired `StoredDocument` records and S3 objects `[jobs] [storage]`
- 🔴 Audit log archival job — archive old audit events to cold storage or S3 after retention window `[jobs] [audit]`

---

## Approvals

> Approval commands and queries are implemented. The following gaps remain.

- 🔵 Approval trigger — `CreateBookingHandler` does not call `RequireApproval` based on policy evaluation `[approvals] [policy]`
- 🔴 Policy engine integration — evaluate travel policy rules at booking creation to determine approval levels required `[approvals] [policy]`
- 🔴 Approval expiry — auto-reject pending approvals after a configurable timeout `[approvals]`
- 🔴 Notification to approver when an approval request is created `[approvals] [email]`

---

## Billing

> Provider-neutral billing entities, application commands, and Stripe SDK wiring exist. License model v2 domain layer (LicenseAgreement, LicenseAgreementEntitlement, LicenseCollectionPolicy, BillingAccount) and accounting stubs are complete. The execution layer is not yet implemented.

- ✅ Prepaid top-up flow — `InitiateRelationshipTopUpCommand` + `ConfirmRelationshipTopUpCommand` implemented
- ✅ `LicenseAgreement` CRUD commands — `CreateLicenseAgreementCommand`, `ActivateLicenseAgreementCommand`, `SupersedeLicenseAgreementCommand` done
- 🔴 `BillingAccount` activation — no command creates/activates a `BillingAccount` when a `LicenseAgreement` is activated `[billing]`
- 🔴 `LicenseAgreement` evaluation at booking time — resolve active agreement for org, check `BillingAccount.AccountStatus`, enforce prepaid balance or credit limit `[billing]`
- 🔴 `LicenseCollectionPolicy` enforcement — block bookings when org is overdue per `BlockBookingsWhenOverdue` flag `[billing]`
- 🔴 Invoice generation job — generate `BillingInvoice` records from billable events on billing cycle dates `[billing] [jobs]`
- 🔴 Billing ledger posting — post `BillingLedgerEntry` rows when charges or credits occur `[billing]`
- 🔴 Journal entry creation — double-entry `JournalEntry`/`JournalLine` posting from invoice/payment events `[billing]`
- 🔴 Credit note creation — command to create credit against org balance `[billing]`
- 🔴 Billing cycle enforcement — prepaid orgs must be blocked from bookings when balance is zero `[billing]`
- 🔴 License fee auto-invoicing — generate invoices on billing cycle dates per `LicenseAgreement.BillingPeriod` `[billing]`
- 🔴 `LicenseAgreementEntitlement` evaluation — resolve feature entitlements for an org at runtime `[billing]`

---

## Travel Policy Engine

> `PolicyEntities.cs` and `TravelPolicyRepository` exist. Policy CRUD endpoints exist. The following are not implemented.

- 🔴 Policy rule evaluation service — check a proposed booking against org/user policy rules `[policy]`
- 🔴 Cabin class enforcement — reject/flag bookings that exceed allowed cabin class `[policy]`
- 🔴 Spend cap enforcement — block bookings over per-trip or per-year spend limits `[policy]`
- 🔴 Advance booking window enforcement — flag bookings made too close to departure `[policy]`
- 🔴 Approval threshold rules — determine required approval levels based on policy `[policy] [approvals]`
- 🔴 Policy assignment to user (per-user override on top of org policy) `[policy]`

---

## Admin / Platform Operations

- 🔴 Audit log export — `GET /api/v1/admin/audit-log/export` (CSV/JSON) `[admin] [audit]`
- 🔴 Impersonation — platform admin ability to act as any user with full audit trail `[admin] [auth]`
- 🔴 Bulk user import (CSV) — create users from uploaded CSV, send invite emails `[admin] [users]`
- 🔴 User suspend / unsuspend via API (endpoint exists in Web UI, needs API handler) `[admin] [users]`

---

## Reporting

> `Reporting/` directory and `Report` entity exist as scaffolds. No implementation.

- 🔴 Booking spend report per org / date range `[reporting]`
- 🔴 Approval SLA report `[reporting]`
- 🔴 User activity report `[reporting]`
- 🔴 Finance reconciliation report `[reporting]`

---

## Deployment & Infrastructure (Phase 11)

- 🔴 ACA Bicep definitions (`deploy/azure/`) for api, web, jobs containers `[infra] [azure]`
- 🔴 Azure Key Vault secrets wiring `[infra] [azure] [secrets]`
- 🔴 GitHub Actions CI/CD pipeline — build + test on PR, publish images, rolling deploy to ACA `[infra] [ci-cd]`
- 🔴 Caddyfile TLS + reverse proxy for dev / staging (`deploy/caddy/`) `[infra]`
- 🔴 Environment config matrix — Development, Staging, Production `[infra] [config]`
- 🔴 SQL migration script generation in CI pipeline `[infra] [ci-cd] [migrations]`

---

## Observability & Logging (Phase 12)

- 🔵 Serilog Console sink is active in dev; production config not finalised `[observability]`
- 🔴 Remove PostgreSQL Serilog sink from prod config (stdout only in prod) `[observability]`
- 🔴 Azure Monitor / Log Analytics workspace setup `[observability] [azure]`
- 🔴 Structured log field coverage — `RequestId`, `UserId`, `OrgId` not enriched on all log entries `[observability]`
- 🔴 Alert rules: error rate spike, auth failure spike, job failure `[observability] [azure]`

---

## Mobile (Phase 8 — ON HOLD)

> Do not raise issues for these until Phase 8 is re-prioritised.

- 🔴 `src/Cinturon360.Maui/` scaffold `[mobile] [on-hold]`
- 🔴 OIDC login path `[mobile] [on-hold]`
- 🔴 Internal login path `[mobile] [on-hold]`
- 🔴 90-day revocable device session `[mobile] [on-hold]`
- 🔴 QR-based device linking from web `[mobile] [on-hold]`
- 🔴 Push MFA `[mobile] [on-hold]`
- 🔴 Apple social login `[mobile] [on-hold]`
- 🔴 Facebook social login `[mobile] [on-hold]`

---

## Legacy Reference Cleanup (Phase 13)

- 🔴 Move `legacy-reference/` contents to `legacy/v4-reference` branch `[cleanup]`
- 🔴 Remove `legacy-reference/` from `main` `[cleanup]`

---

## Technical Debt & Code Quality

- 🔵 `AuthorizationBehavior.cs` — no-op placeholder, permission enforcement missing in MediatR pipeline `[tech-debt] [auth]`
- 🔵 `LoginCommandHandler.cs:67` — `// TODO: resolve permissions from role assignments` — JWT does not include permission codes `[tech-debt] [auth]`
- 🔵 `Class1.cs` stub files in multiple projects (`Common`, `Domain`, `Application`, `Integrations`, `Infrastructure`) — harmless but should be removed `[tech-debt] [cleanup]`
- 🔵 `src/Cinturon360.Integrations/Common/` subdirectories (Auth, Http, Logging, Mapping, Resilience) are empty scaffolds — shared HTTP resilience policy (Polly retry/circuit breaker) not implemented `[tech-debt] [integrations]`
- 🔴 Polly retry + circuit breaker on all external HTTP calls (Duffel, ECB, MailerSend, Stripe, GitHub) `[tech-debt] [resilience]`
- 🔴 Idempotency key support on Stripe payment intent creation `[tech-debt] [billing] [stripe]`
