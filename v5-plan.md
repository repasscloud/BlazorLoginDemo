# Cinturon360 v5 — End-to-End Architecture & System Plan

> Reference material: `legacy-reference/_notes.txt`  
> Date: April 2026

---

## 1. What Changed from Previous Versions

| Removed | Replaced with / Reason |
|---|---|
| Migrator Docker container | SQL migration scripts generated per release, applied to PostgreSQL directly on deploy |
| Redis cache | Not required; PostgreSQL handles caching needs at current scale; revisit if horizontal session state becomes an issue |
| `Cinturon360.Shared` monolith | Split into `Common`, `Contracts`, `Domain`, `Application` (see §7) |
| Single `Cinturon360.sln` with 3 projects | Multi-project solution with clear layer ownership |

---

## 2. Core Platform Constraints

- **Multi-tenant SaaS** — every entity, query, and token is scoped to a tenant
- **Multi-level org hierarchy** — Vendor → TMC → Client
- **Auth is in the API** — the Blazor front-end has no authentication of its own
- **Authorization is internal** — external IdPs authenticate; Cinturon360 authorises
- **Horizontal scaling** — front-end 1–3 instances, API 1–2 instances, zero-downtime staged upgrades via Docker on Azure
- **No migrator container** — migrations are SQL scripts applied on deploy
- **Supercronic job container** — replaces any ad-hoc background task

---

## 3. Architectural Layers

```
┌─────────────────────────────────────────┐
│  Cinturon360.Web  (Blazor WASM/Server)  │  Presentation only
└────────────────────┬────────────────────┘
                     │ HTTP / REST
┌────────────────────▼────────────────────┐
│  Cinturon360.Api                        │  Thin HTTP surface, auth endpoints
└────────────────────┬────────────────────┘
                     │
┌────────────────────▼────────────────────┐
│  Cinturon360.Application                │  Use-cases, orchestration, mapping,
│                                         │  validation, permission evaluation
└──────┬─────────────┬────────────────────┘
       │             │
┌──────▼──────┐ ┌────▼────────────────────┐
│ .Domain     │ │ .Integrations           │  External providers (flights, hotels,
│             │ │                         │  Stripe, IdPs, S3, GitHub ticketing)
└──────┬──────┘ └─────────────────────────┘
       │
┌──────▼──────────────────────────────────┐
│  Cinturon360.Data                       │  EF Core, PostgreSQL, migrations,
│                                         │  repositories, entity config
└─────────────────────────────────────────┘
       │
┌──────▼──────────────────────────────────┐
│  Cinturon360.Infrastructure             │  Jobs, Storage, Email/SMS, Logging
│  Cinturon360.Jobs  (Supercronic host)   │  Recurring scheduled jobs
└─────────────────────────────────────────┘
```

**Key rule**: Web does not touch Data. API does not own business logic. Application layer is the translator boundary.

---

## 4. Authentication & Identity Design

### 4.1 Boundary

| Concern | Owner |
|---|---|
| Who is the user | External IdP or internal credential |
| Does the user belong to this tenant | External IdP / tenant config |
| Is the user allowed in the platform | Cinturon360 |
| What can the user do | Cinturon360 (internal roles/permissions) |
| API/service token issuance & revocation | Cinturon360 |

### 4.2 Identity Types

**HumanUser** — interactive principal, browser/mobile/API access  
**ServiceAccount** — non-human, API-only, token-based, no interactive login, no password, no MFA, no browser, no mobile

These are **not the same entity**. They must not share a weak flag distinction.

### 4.3 Supported Login Methods (for human users, subject to tenant policy)

- Local username + password
- Local username + password + MFA (TOTP, SMS, Email OTP, Push)
- OIDC SSO
- SAML 2.0 SSO
- Social (Google, Microsoft, Apple, Facebook) — where enabled
- Mobile app login (OIDC path or internal path)
- QR-based device linking / sign-in flow

### 4.4 Tenant SSO Tiers

| Tier | Description |
|---|---|
| 1 | SSO only, JIT user creation on first login |
| 2 | SSO + coarse external claims mapping |
| 3 | SSO + SCIM lifecycle sync |
| 4 | Batch CSV / FTP / SFTP import (legacy) |

### 4.5 Token Classes

| Class | Lifetime | Refresh | Notes |
|---|---|---|---|
| Interactive web session | Standard | Yes | Browser-based |
| Mobile device session | 90 days | Revocable | Device-scoped |
| Short-lived API login token | ~30 min | No | Human API access |
| User PAT | Explicit expiry | No | User-created automation token |
| Service-account token | Explicit expiry | No | Non-human API access |

QR codes must be one-time-use, short-lived, tied to an authenticated session, server-validated, and revocable. They **must not** be reusable long-lived bearer tokens.

### 4.6 Protocols

- OIDC preferred first for SSO
- SAML 2.0 supported second
- SCIM for provisioning only (not login)
- OAuth 2.0 as framework for delegated token flows

---

## 5. Org Hierarchy & Access Model

```
Vendor  (e.g. Avanoa Technology)
  └── TMC  (e.g. Avanoa Travel)
        └── Client  (e.g. Coca-Cola)
```

### 5.1 Core Rules

- A standard tenant user has **exactly one home org**
- Roles are assigned **only in the user's home org**
- The role's **Scope** controls traversal

| Scope | Meaning |
|---|---|
| `Self` | Acts only within own org |
| `SelfAndDescendants` | Acts within own org + all child orgs below |

- Access never flows **upward** (Client cannot see TMC data)
- Access never flows **sideways** (Client A cannot see Client B data)
- Multi-org membership is **not** the model; the hierarchy is

### 5.2 Sudo / Platform Access

Platform-level sudo accounts:
- Have no home org
- Use `PlatformRole` (Sudo, PlatformOps, PlatformAudit)
- Bypass org hierarchy entirely
- Are separate from all tenant users

### 5.3 Scope Example

| User | Home Org | Scope | Can Access |
|---|---|---|---|
| Bob (Coca-Cola) | Coca-Cola | Self | Coca-Cola only |
| Warren (Avanoa Travel) | Avanoa Travel | SelfAndDescendants | Avanoa Travel + Coca-Cola |
| Danijel (Avanoa Technology) | Avanoa Technology | SelfAndDescendants | All three |

---

## 6. Data Model — Core Tables

### Users (one row per human principal)

Key fields: `UserId`, `UserCategory` (Public/Client/Tmc/Vendor/Platform), `PlatformRole`, `HomeOrgId`, `IsActive`, `IsLocked`, `IsSuspended`, `LanguageCode`, `TimeZone`, `CurrencyCode`

### Supporting tables (never flattened into Users)

| Table | Purpose |
|---|---|
| `UserAuthMethods` | Per-method: Password, Google, Microsoft, OIDC, SAML, QrDeviceLink, etc. |
| `UserSecurity` | Password hash, MFA state, direct-login policy, concurrency stamp |
| `UserMfaMethods` | Per-method MFA: TOTP, SMS, EmailOtp, Push |
| `UserRecoveryCodes` | Hashed recovery codes |
| `UserSessions` | Web / API / Mobile / QrLinkedMobile sessions with JTI tracking |
| `UserApiTokens` | PATs & service-account tokens — hash only, never plaintext after creation |
| `UserRoleAssignments` | `UserId`, `OrgId`, `RoleId`, `ScopeMode` |
| `UserAuditEvents` | Append-only audit log per user action |
| `UserProvisioningSources` | JIT / SCIM / CsvImport / Manual / PublicSignup |
| `UserAccessOverrides` | Per-user policy exceptions (avoid 50-column bloat) |

### Traveller Profile (linked to user, but separate concern)

`TravellerProfile` → passport, DOB, seat/meal preference, nationality  
`TravellerLoyaltyPrograms` → Qantas FF, Velocity, Marriott Bonvoy, etc. (many per traveller)  
`TravellerServicingProfile` → VIP level, service tier, dedicated consultant  
`UserEmergencyContacts` → multiple per user  
`UserAddresses` → Home / Billing / Mailing  
`UserPreferences` → preferred airport, airline, hotel chain, notification settings

### Roles & Permissions

`Roles` → `OrgType`, `Name`, `IsSystemRole`  
`RolePermissions` → `RoleId`, `PermissionCode`  
Permission codes (examples): `bookings.read`, `bookings.manage`, `reports.finance.read`, `policy.manage`, `users.manage`, `approvals.manage`

Use **permission codes** not boolean columns like `IsFinanceUser`.

---

## 7. Approvals & Travel Policy

- **0 approvals** — in-policy / auto-approved
- **1 approval** — standard manager approval
- **2 approvals** — exceptions or higher spend threshold
- **3 approvals** — stricter enterprise / regulated cases

Approval levels are configured per org/policy. Routine in-policy bookings auto-approve. Threshold breaches route to manual approval flows.

Travel policy will be a rich, configurable module covering: included/excluded airlines & hotels, spend caps per trip type, cabin class rules, advance booking windows, approval thresholds, and more.

---

## 8. Billing & Payments

### 8.1 Stripe Integration

- Provider-neutral model with Stripe as one execution provider
- Seller-owned Stripe account per `PaymentProviderConnection` (no Stripe Connect marketplace split-routing)
- Seller/buyer `BillingRelationship` drives billing responsibility independently from org hierarchy
- Buyer payment setup uses dedicated Stripe setup links generated by API and optionally emailed during onboarding
- Provider customer/payment method references are stored per connection (`ProviderCustomer`, `ProviderPaymentMethod`)
- Webhooks are connection-scoped (`/api/v1/webhooks/payment-providers/stripe/{connectionId}`) with idempotent event persistence
- Legacy single-endpoint/single-customer model is retained temporarily for backward compatibility only

### 8.2 Payment Flexibility

- **Split payments** — multiple payment methods and amounts against a single invoice
- **Deferred payment** — booking made without immediate charge ("charge later")
- **Prepaid balance** — charge org's billing method upfront; draw down against travel spend
- **Postpaid** — draws prepaid balance first, then charges remainder to card on file
- **CR notes** — refund/credit against org balance

### 8.3 Billing Modes

| Mode | Description |
|---|---|
| `Prepaid` | Must not spend outside prepaid balance |
| `Postpaid` | Prepaid balance first, then charge on file |
| `PayAsYouGo` | Charge per transaction with no prepaid requirement |

Billing mode is set on the `LicenseAgreement` via the `BillingModel` enum (`Prepaid`, `Postpaid`, `PayAsYouGo`). A `BillingAccount` links an org to its active `LicenseAgreement` and carries runtime status (`Active`, `Suspended`, `Closed`, `UnderReview`).

All transactions are linked to an **org** and optionally attributed to a **user** (or system/null for system transactions).

### 8.4 Licensing & Fees

- License per org: controls user account cap, billing frequency, billing date, markup rules, booking fees
- Billing cycles: Annual, Bi-annual, Quarterly, Monthly, PAYG — all configurable including $0 annual
- Per-org markup: e.g. $5 per booking for Client A, $2 + 3% markup for Client B
- Invoice flag: license fees can auto-invoice or auto-charge
- License can restrict features/access as well as set user count limits

License model note (3 May 2026):
- The License v2 domain layer is complete. `LicenseAgreement` is the seller-owned commercial contract between a Seller org and a Buyer org. It carries `BillingModel`, `BillingPeriod`, `CollectionMode`, payment terms, credit limit, access package code, and effective dates.
- `LicenseAgreementEntitlement` stores per-feature entitlement values (boolean, quantity, money, percentage, duration).
- `LicenseCollectionPolicy` stores collection rules (grace period, block-on-overdue, action after grace).
- `BillingAccount` links an org to its active `LicenseAgreement` and carries runtime account status.
- Accounting stub tables exist: `BillingLedgerEntry`, `BillingInvoice`, `BillingInvoiceLine`, `PaymentAttempt`, `JournalEntry`, `JournalLine`.
- **Pending execution layer:** evaluating `LicenseAgreement` at booking time, generating invoices on billing cycle, enforcing `LicenseCollectionPolicy` (block bookings when overdue), activating `BillingAccount` on agreement activation.

---

## 9. Background Jobs (Supercronic)

A dedicated `Cinturon360.Jobs` container runs with a custom Supercronic image.

Supported schedules: monthly, fortnightly, weekly, daily, 12-hourly, 6-hourly, hourly, every 15 min, every 1 min, every 30 sec, every 15 sec, every 5 sec, every 1 sec.

Jobs are stored in a `Jobs` table (JSONB payload) and loaded via:
- API endpoint (`/admin/jobs`)
- Web UI (for authorised users)
- Direct DB seed for platform jobs

Job categories:
- Reference data refresh (geography, supplier data)
- PDF generation and S3 upload
- Storage cleanup (expired files)
- GitHub ticket sync
- Reconciliation reports
- Billing / invoice generation
- Data feed ingestion (flights, hotels, cars, rail)
- Email/SMS delivery queues

---

## 10. Logging Strategy

Logging is a **first-class platform subsystem**, not an afterthought.

### Log Categories

| Category | Captures |
|---|---|
| **Operational** | API requests, job runs, integration calls, errors, retries |
| **Audit** | Permission changes, role changes, booking updates, financial changes |
| **Security** | Login/logout, SSO events, MFA events, PAT creation/revocation, token misuse |
| **Domain Events** | Ticket escalations, booking lifecycle, approval events, sync events |

### Required Fields (all log entries)

`TimestampUtc`, `Level`, `EventType`, `Category`, `Action`, `Outcome`, `EntityType`, `EntityId`, `RequestId` (correlation), `TransactionId`, `UserId`, `OrgId`, `DurationMs`, `HttpMethod`, `HttpStatus`, `HttpPath`, `Note`, retry/cancel/timeout/fail flags

---

## 11. PDF, Storage & Document Generation

- Generate PDFs server-side
- Upload to S3 (AWS)
- Create `StoredDocument` DB record: metadata, storage key, source entity, generated-by, expires-at, deletion status
- Downloadable via signed URL
- Cleanup via scheduled job

---

## 12. External Integrations

Each integration has its own isolated module in `Cinturon360.Integrations`:

- **Flights** — Amadeus (existing), Duffel, others
- **Hotels** — TBD provider
- **Cars / Rail** — TBD
- **Payments** — Stripe
- **Exchange Rates** — external FX provider
- **Geography** — airport/city/country reference data
- **GitHub** — ticketing / incident management
- **AWS S3** — document storage
- **Identity Providers** — OIDC, SAML, SCIM, Google, Microsoft, Apple, Facebook

External provider models **never** become API contracts directly. Each integration has:
1. Provider-specific request/response models
2. Mapper to internal unified model
3. Retry/timeout/error handling
4. Log correlation
5. Provider auth handling

---

## 13. Docker & Deployment

### Containers

| Container | Image | Notes |
|---|---|---|
| `api` | `cinturon360-api` | 1–2 replicas |
| `web` | `cinturon360-web` | 1–3 replicas |
| `jobs` | `cinturon360-jobs` (Supercronic) | 1 replica |
| `postgres` | `postgres:16` | Managed / Azure DB in prod |
| `pgadmin` | Optional dev only | |

**No migrator container.** Migrations are SQL scripts applied before service start in the deploy pipeline.

**No Redis.** Session state is PostgreSQL-backed. Revisit if horizontal scaling reveals a contention problem.

### Zero-Downtime Deploys

- Staged rolling updates via Docker Compose / Azure Container Apps
- API instances updated one at a time
- Web instances updated one at a time
- Jobs container restarts cleanly (idempotent job design required)

---

## 14. MAUI Mobile App

Treated as a first-class client. Supports:
- OIDC login path where tenant uses SSO
- Internal login path where tenant/user policy allows
- 90-day revocable device session
- QR-based device linking from authenticated web profile
- Server-managed session (revocable at any time)

---

## 15. Hard Design Rules (never violate these)

1. Authentication and authorization are separate concerns
2. SSO does not own the platform's detailed permission model
3. SCIM is provisioning, not login
4. OIDC and SAML can both exist in the same app
5. Human users and service accounts must be modeled as separate identity types
6. PAT-only service accounts are valid even for SSO-enabled tenants
7. Alternate login methods must not accidentally bypass tenant security policy
8. All roles and permissions remain authoritative inside Cinturon360
9. Every auth/provisioning feature must be tenant-aware
10. Token lifetimes, revocation, and auditability are first-class concerns
11. A standard tenant user has exactly one home org
12. Access never flows upward or sideways through the org hierarchy
13. External provider models never become API contracts directly
14. No giant generic `Helpers`, `Models`, or `Shared` bucket — every namespace has a declared purpose
15. The `DbContext` must never become a 1000+ DbSet monolith

---

## 16. What to Build First (Suggested Phase Order)

### Phase 0 — Foundation
- Solution scaffold (see `v5-solution-structure.md`)
- `Directory.Build.props`, `Directory.Packages.props`, `global.json`, `.editorconfig`
- `Cinturon360.Common` — IdGeneration, Precision, Results, Extensions
- `Cinturon360.Domain` — core enums, base entity, value objects
- `Cinturon360.Data` — DbContext skeleton, modular entity config, migrations infra

### Phase 1 — Identity & Auth
- User, UserAuthMethod, UserSecurity, UserSessions, UserApiTokens, UserAuditEvents tables
- Auth endpoints (login, logout, refresh, PAT CRUD, QR linking)
- Tenant auth config (OIDC, SAML, local login policy)
- Internal role/permission engine (Roles, RolePermissions, UserRoleAssignments)

### Phase 2 — Org Hierarchy
- Organizations table (Vendor/TMC/Client, parent-child)
- Scope evaluation logic (Self / SelfAndDescendants)
- Admin endpoints for org management

### Phase 3 — Traveller & Profile
- TravellerProfile, LoyaltyPrograms, EmergencyContacts, Addresses, Preferences
- Web UI pages for profile management

### Phase 4 — Travel & Booking
- Geography (airports, cities, countries)
- Flight search integration (Amadeus / Duffel)
- Booking / Quote flow
- Travel policy engine

### Phase 5 — Approvals & Policy
- Approval workflow (0–3 levels)
- Policy rule engine
- Policy assignment per user/org

### Phase 6 — Billing & Payments
- License model, org billing config
- Stripe integration
- Split payments, prepaid/postpaid balance, CR notes

### Phase 7 — Jobs, Storage, Reporting
- Supercronic jobs container
- PDF generation + S3 storage
- Reports module
- GitHub ticketing integration

### Phase 8 — Mobile (MAUI)
- MAUI client
- OIDC/internal auth paths
- QR device linking
- 90-day session model
