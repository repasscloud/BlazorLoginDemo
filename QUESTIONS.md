# QUESTIONS.md

Questions that require decisions before the next phase of development.
Captured during the initial v5 scaffold session.

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

Recommended UI direction by area:

- Public website: Tailwind. Brand, marketing, modern visual identity.
- Login / onboarding: Tailwind. First impression matters.
- Traveller / end-user pages: Tailwind. Needs to feel modern, guided, consumer-grade.
- Booking/search flow: Tailwind. Core product differentiation.
- Itinerary / trip timeline: Tailwind. Needs custom travel-specific UX.
- AI recommendation panels: Tailwind. Needs product-specific visual treatment.
- Admin console: Fluent UI Blazor. Enterprise controls, tables, command bars.
- Support desk: Fluent UI Blazor. Queues, cases, notes, status workflows.
- Finance team: Fluent UI Blazor. Tables, reconciliation, exports, audit trails.
- Travel agent queue: Hybrid. Fluent for queues/tables, Tailwind for booking/itinerary experience.
- Reporting dashboards: Hybrid. Tailwind layout, charts/tables as needed.

---

## Q16 — License v2 Canonical Model (Billing + Access)

### Reference Documents

Primary reference:

```text
docs/architecture/cinturon360-v5-billing-licensing-entitlements-accounting-reference.md
```

Supporting previous reference:

```text
docs/architecture/cinturon360-billing-stripe-architecture-reference.md
```


### Decision Needed

Stripe and provider-neutral billing foundations are now in place, but the final license model is still pending.

Confirm the canonical License v2 shape and ownership rules used by billing execution.

Proposed minimum fields:

- `SellerOrgId`
- `BuyerOrgId`
- `BillingMode`
- `BillingFrequency`
- `CollectionMode`
- `PaymentTermsDays`
- `CreditLimit`
- `RequirePaymentBeforeTicketing`
- Access package/features
- `EffectiveFrom`
- `EffectiveTo`

Options:

- (a) seller-owned contract object
- (b) buyer-owned plan object
- (c) hybrid with both seller and buyer segments


### Answer

Use **(a) seller-owned contract object**.

License v2 should be the canonical seller-owned commercial contract that defines the billing, access, entitlement, credit, collection, and effective-date rules between a `SellerOrgId` and a `BuyerOrgId`.

This model should be treated as the source of truth for billing execution.

The buyer may have billing profiles, payment methods, policy-specific payment rules, and UI-facing plan summaries, but those are derived from or linked to the seller-owned License v2 / billing relationship. They are not the canonical commercial contract.


### Ownership Rule

The seller organisation owns the licence because the seller is the party:

- granting access;
- setting commercial terms;
- defining billing model;
- setting credit limits;
- deciding payment collection rules;
- extending credit;
- collecting payment;
- carrying commercial risk.

Examples:

```text
Avanōa Technology sells/licences platform access to a TMC.
A TMC sells/licences travel platform access and billing terms to a Client organisation.
A Vendor may sell/licence access to a child Vendor or TMC.
```

The organisation hierarchy must not automatically determine billing responsibility.

Billing responsibility is defined by the seller/buyer licence relationship.

### Final Decision

License v2 is a:

```text
seller-owned, seller-issued, buyer-targeted commercial contract
```

Buyer-side billing profiles, payment methods, policy billing rules, and plan summaries may exist, but they must be linked to the seller-owned licence/billing relationship and must not override it unless an explicit seller-approved override exists.


### Canonical License v2 Minimum Shape

```csharp
public sealed class LicenseAgreement
{
    public required string Id { get; init; }

    // Commercial parties
    public required string SellerOrgId { get; init; }
    public required string BuyerOrgId { get; init; }

    // Billing execution model
    public BillingModel BillingModel { get; init; }
    public BillingPeriod BillingPeriod { get; init; }
    public CollectionMode CollectionMode { get; init; }

    // Credit and collection rules
    public int PaymentTermsDays { get; init; }
    public decimal? CreditLimitAmount { get; init; }
    public string CurrencyCode { get; init; } = "AUD";
    public bool RequirePaymentBeforeTicketing { get; init; }

    // Access and entitlements
    public required string AccessPackageCode { get; init; }
    public List<LicenseAgreementEntitlement> Entitlements { get; init; } = [];

    // Effective dating
    public DateOnly EffectiveFrom { get; init; }
    public DateOnly? EffectiveTo { get; init; }

    // Lifecycle
    public LicenseAgreementStatus Status { get; set; } = LicenseAgreementStatus.Draft;

    // Versioning
    public int VersionNumber { get; init; } = 1;
    public string? PreviousLicenseAgreementId { get; init; }
    public string? SupersededByLicenseAgreementId { get; set; }

    // Audit
    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
```


### Billing Model

Use this split:

```csharp
public enum BillingModel
{
    Prepaid = 1,
    Postpaid = 2,
    PayAsYouGo = 3
}

public enum BillingAccountStatus
{
    Active = 1,
    Suspended = 2,
    Closed = 3,
    UnderReview = 4
}
```

`BillingModel` defines how spend is authorised.


- BillingModel: `Prepaid`; Meaning: Buyer can only spend from available loaded balance/credit.
- BillingModel: `Postpaid`; Meaning: Buyer can consume first and pay after invoice issue, subject to credit and suspension rules.
- BillingModel: `PayAsYouGo`; Meaning: Buyer is charged per transaction or near-immediately.


`BillingAccountStatus` controls whether the billing account can currently transact.

Suspension should not be treated as a billing model.

This allows combinations such as:

```text
Prepaid + Active
Prepaid + Suspended
Postpaid + Active
Postpaid + UnderReview
PayAsYouGo + Active
PayAsYouGo + Suspended
```


### Billing Period

`BillingPeriod` defines how often recurring billing is actioned, especially for:

- access fees;
- recurring platform fees;
- subscription-style charges;
- usage aggregation;
- minimum-spend checks;
- invoice generation.

Recommended enum:

```csharp
public enum BillingPeriod
{
    None = 0,
    PerTransaction = 1,
    Daily = 2,
    Weekly = 3,
    Fortnightly = 4,
    Monthly = 5,
    Quarterly = 6,
    BiAnnual = 7,
    Annual = 8,
    Manual = 9
}
```

Example:

```text
AccessFeeAmount = 500
CurrencyCode = AUD
BillingPeriod = Monthly
```

Meaning:

```text
Invoice or charge AUD 500 per month for system access.
```


### Payment Terms

`PaymentTermsDays` defines how many days after invoice issue the buyer has to pay before the invoice becomes overdue.

Collection and suspension behaviour should be controlled separately through collection rules.

Example:

```csharp
public sealed class LicenseCollectionPolicy
{
    public required string LicenseAgreementId { get; init; }

    public int PaymentTermsDays { get; init; }
    public int GracePeriodDays { get; init; }

    public bool BlockBookingsWhenOverdue { get; init; }
    public bool RequirePaymentBeforeTicketing { get; init; }

    public CollectionActionAfterGrace ActionAfterGrace { get; init; }
}
```

Example timeline:

```text
Invoice issued: 1 May 2026
PaymentTermsDays: 14
Due date: 15 May 2026
GracePeriodDays: 3
Collection action may begin: 18 May 2026
```


### Collection Mode

`CollectionMode` defines how payment is collected.

```csharp
public enum CollectionMode
{
    None = 0,
    Manual = 1,
    Automatic = 2,
    ExternalReferenceOnly = 3
}
```


- CollectionMode: `Automatic`; Meaning: Stripe or another payment provider collects automatically.
- CollectionMode: `Manual`; Meaning: Invoice is issued but payment is manually reconciled.
- CollectionMode: `ExternalReferenceOnly`; Meaning: Cinturon360 records billing but external systems collect payment.
- CollectionMode: `None`; Meaning: No payable billing; useful for trial, internal, shadow, or no-charge arrangements.



### Access Package and Features

The licence should not store a random flat list of booleans.

It should reference:

```text
AccessPackageCode
```

and contain explicit entitlement rows.

Use grouped entitlement IDs:

```text
100–199    Organisation / tenant capacity
200–299    User / identity / access
300–399    Traveller profile / personal travel data
400–499    Booking / travel operations
500–599    Public / search / marketplace
600–699    API / integration / automation
700–799    Reporting / analytics / finance visibility
800–899    Support / service desk / operational tools
900–999    Security / compliance / audit
1000–1099  Billing / accountancy / settlement
1100–1199  AI / automation / assistant features
1200–1299  Data retention / storage / documents
9000–9999  Internal / experimental / migration
```

Use the agreed `EntitlementType` enum from:

```text
docs/architecture/cinturon360-v5-billing-licensing-entitlements-accounting-reference.md
```

as the initial canonical entitlement catalogue.


### Entitlement Instance Shape

```csharp
public sealed class LicenseAgreementEntitlement
{
    public required string Id { get; init; }

    public required string LicenseAgreementId { get; init; }

    public EntitlementType Type { get; init; }

    public EntitlementValueKind ValueKind { get; init; }

    public bool? BooleanValue { get; init; }

    public decimal? NumericValue { get; init; }

    public string? TextValue { get; init; }

    public bool IsUnlimited { get; init; }

    public DateOnly EffectiveFrom { get; init; }

    public DateOnly? EffectiveTo { get; init; }

    public bool IsActive { get; set; } = true;

    public string? Notes { get; init; }
}
```

```csharp
public enum EntitlementValueKind
{
    Boolean = 1,
    Quantity = 2,
    Money = 3,
    Percentage = 4,
    DurationDays = 5,
    DurationMonths = 6,
    StorageGb = 7,
    RequestsPerPeriod = 8
}
```


### Billing Execution Rules

Billing execution should resolve from the seller-owned licence.

### 1. Find active licence

Find active licence where:

```text
SellerOrgId = seller
BuyerOrgId = buyer
EffectiveFrom <= today
EffectiveTo == null || EffectiveTo >= today
Status = Active
```

### 2. Resolve billing model

```text
Prepaid:
  Check available balance before allowing chargeable activity.

Postpaid:
  Allow usage subject to credit limit, collection state, overdue state, and billing account status.

PayAsYouGo:
  Require immediate or near-immediate charge.
```

### 3. Resolve collection mode

```text
Automatic:
  Execute collection through configured provider connection.

Manual:
  Generate invoice and await manual reconciliation.

ExternalReferenceOnly:
  Record/export billing but do not collect inside Cinturon360.

None:
  Calculate only if required for shadow/reporting.
```

### 4. Resolve entitlements

Resolve:

- base access package;
- explicit entitlement overrides;
- quantity limits;
- feature/module access;
- expiry/effective dates;
- inherited access where applicable.

Examples:

```text
User accounts
Travellers
API requests
Bookings
Storage
SSO
SCIM
Finance reporting
Support desk
AI assistant
```

### 5. Generate accounting-safe billing records

Billing execution must generate or update appropriate financial records:

- invoice;
- invoice lines;
- payment attempts;
- billing ledger entries;
- later, DR/CR journal entries.


### Accounting Compatibility

License v2 must not be the accounting ledger.

The licence defines the commercial rules.

Financial truth belongs to:

- invoices;
- invoice lines;
- payment attempts;
- billing ledger entries;
- journal entries;
- exports to external finance systems.

Do not store mutable financial truth directly on the licence, such as:

```text
PrepaidBalance
CurrentOutstandingAmount
LatestPaymentStatus
LastInvoiceStatus
```

Those should be derived from ledger, invoice, and payment records.


### Postpaid Accounting Treatment

For postpaid billing:

```text
Invoice issued:
DR Accounts Receivable
CR Revenue
CR Tax Payable

Payment received:
DR Cash / Provider Clearing
CR Accounts Receivable
```

Example:

```text
Invoice total: AUD 110.00
Revenue: AUD 100.00
GST: AUD 10.00
```

Journal:


- Account: Accounts Receivable; DR: 110.00
- Account: Platform Revenue; CR: 100.00
- Account: GST Payable; CR: 10.00


Payment received:


- Account: Cash / Provider Clearing; DR: 110.00
- Account: Accounts Receivable; CR: 110.00



### Prepaid Accounting Treatment

For prepaid billing:

```text
Top-up received:
DR Cash / Provider Clearing
CR Customer Credits Liability

Service consumed:
DR Customer Credits Liability
CR Revenue
CR Tax Payable
```

Example top-up:

```text
Buyer loads AUD 1,100.00 credit.
```

Journal:


- Account: Cash / Provider Clearing; DR: 1,100.00
- Account: Customer Credits Liability; CR: 1,100.00


Example consumption:

```text
AUD 110.00 of service consumed.
Revenue: AUD 100.00
GST: AUD 10.00
```

Journal:


- Account: Customer Credits Liability; DR: 110.00
- Account: Platform Revenue; CR: 100.00
- Account: GST Payable; CR: 10.00



### Pay-As-You-Go Accounting Treatment

For pay-as-you-go billing, collection may happen immediately or near-immediately.

```text
Usage event occurs.
Payment is collected.
Invoice/receipt is generated.
Accounting entries are created.
```

Typical journal:

```text
DR Cash / Provider Clearing
CR Revenue
CR Tax Payable
```

### Why Not Buyer-Owned Plan Object

A buyer-owned plan object is not sufficient because the buyer does not own:

- commercial terms;
- credit exposure;
- payment collection mode;
- provider connection;
- billing risk;
- entitlement grant;
- seller-issued pricing;
- suspension/collection policy.

The same buyer may also be billed by different sellers in different contexts.

Example:

```text
Buyer organisation may be billed by:
- Avanōa Technology for platform-level access;
- a TMC for travel services;
- another partner/vendor for a different commercial arrangement.
```

The buyer cannot be the canonical owner of all those commercial contracts.

### Why Not Hybrid

A hybrid model creates ambiguity over which side is authoritative.

Buyer-side records are still useful for UI and operational defaults, but they should reference the seller-owned licence rather than becoming a competing source of truth.

Avoid:

```text
Seller licence says one thing.
Buyer plan says another thing.
Billing resolver has to guess which is authoritative.
```

Use:

```text
Seller-owned licence = source of truth.
Buyer-side profile = operational view/configuration linked to licence.
```

### Relationship to Provider-Neutral Billing

License v2 must remain provider-neutral.

The licence should not contain Stripe-specific fields such as:

```text
StripeCustomerId
StripePaymentMethodId
StripeAccountId
CardLast4
CardFingerprint
```

Those belong to provider-neutral payment/billing records such as:

```text
PaymentProviderConnection
ProviderCustomer
ProviderPaymentMethod
PaymentMethodAssignment
PaymentAttempt
```

Stripe is an execution channel only.

Cinturon360 owns the billing logic.

### Recommended Supporting Objects

License v2 should work alongside these records:

```text
LicenseAgreement
LicenseAgreementEntitlement
LicenseCollectionPolicy
BillingAccount
BillingLedgerEntry
BillingInvoice
BillingInvoiceLine
PaymentProviderConnection
ProviderCustomer
ProviderPaymentMethod
PaymentMethodAssignment
PaymentAttempt
JournalEntry
JournalLine
```

---

# Q17 — Stripe Webhook Endpoint Auto-Provisioning

## Question

Current implementation supports connection-scoped webhook handling and secret storage, but webhook endpoint creation is still manually completed in Stripe Dashboard.

## Decision Needed

Should API auto-create Stripe webhook endpoints on connection setup now?

Options:

```text
(a) Auto-create immediately during ConfigureStripeProviderConnection
(b) Keep manual creation for now and move auto-create to next phase
```

## Answer

Use **(b) keep manual creation for now and move auto-create to the next phase**.

Do not auto-create Stripe webhook endpoints during `ConfigureStripeProviderConnection` yet.

The current implementation already supports the important foundation:

```text
Connection-scoped webhook handling
Webhook secret storage
Provider connection records
Stripe provider configuration
Secret storage abstraction
```

That is enough for the current phase.

Auto-provisioning Stripe webhook endpoints should be added in the next phase once the provider onboarding lifecycle, environment handling, webhook URL generation, idempotency, rotation, and failure recovery rules are fully defined.


## Reference

This decision is aligned with the provider-neutral billing / Stripe architecture reference:

```text
docs/architecture/cinturon360-billing-stripe-architecture-reference.md
```

That document defines the future target state where webhook endpoint creation can happen during provider onboarding, but the current implementation can safely remain manual while the connection-scoped webhook handling and secret storage foundations are stabilised.


## Reasoning

Auto-creating Stripe webhook endpoints is useful, but it introduces additional operational complexity.

Before enabling automatic webhook creation, the system should have clear rules for:

```text
Webhook URL generation
Live vs test mode separation
Connection-specific endpoint selection
Webhook event selection
Idempotent setup retries
Failed setup recovery
Secret storage and rotation
Duplicate endpoint detection
Webhook endpoint disable/delete behaviour
Environment-specific API base URLs
Provider connection re-verification
Audit logging
```

If this is added too early, `ConfigureStripeProviderConnection` may become responsible for too much at once.

A safer phased approach is:

```text
Current phase:
  Configure provider connection.
  Store provider secrets.
  Support connection-scoped webhook endpoint.
  Manually create webhook endpoint in Stripe Dashboard.
  Store webhook secret reference.

Next phase:
  API creates Stripe webhook endpoint automatically.
  API stores returned webhook endpoint ID.
  API stores returned webhook signing secret in Key Vault.
  API marks webhook provisioning status.
  API supports rotation/recreate/disable operations.
```

## Recommended Current Behaviour

During the current phase:

```text
1. Admin configures Stripe provider connection.
2. API stores Stripe credentials in secret storage.
3. API verifies the connection.
4. Admin manually creates webhook endpoint in Stripe Dashboard.
5. Admin enters or stores webhook signing secret through Cinturon360.
6. API stores webhook secret in Key Vault or equivalent.
7. Connection-scoped webhook handler receives events.
```

Recommended webhook endpoint format:

```text
/api/v1/webhooks/payment-providers/stripe/{connectionId}
```

The connection-specific endpoint is preferred because it is deterministic and audit-safe.

## Future Auto-Provisioning Shape

When this is moved to the next phase, the flow should be:

```text
1. Create PaymentProviderConnection in PendingSetup.
2. Store Stripe API credentials in Key Vault.
3. Verify Stripe key.
4. Generate connection-specific webhook URL.
5. Create Stripe webhook endpoint using the seller organisation's Stripe key.
6. Store Stripe webhook endpoint ID on PaymentProviderConnection.
7. Store returned webhook signing secret in Key Vault.
8. Mark webhook provisioning as complete.
9. Mark provider connection as Verified.
```

Recommended future endpoint creation target:

```text
https://api.cinturon360.com/api/v1/webhooks/payment-providers/stripe/{connectionId}
```


## Future Provider Connection Fields

```csharp
public sealed class PaymentProviderConnection
{
    public required string Id { get; init; }

    public required string OwnerOrgId { get; init; }

    public PaymentProviderType ProviderType { get; init; }

    public bool IsLiveMode { get; init; }

    public bool IsEnabled { get; set; }

    public bool IsPrimary { get; set; }

    public ProviderConnectionStatus Status { get; set; }

    public string SecretBundleReference { get; set; } = "";

    public string? ProviderAccountId { get; set; }

    public string? ProviderAccountName { get; set; }

    public string? WebhookEndpointId { get; set; }

    public string? WebhookSecretReference { get; set; }

    public ProviderUsageScope UsageScope { get; set; }

    public DateTimeOffset? VerifiedAtUtc { get; set; }

    public DateTimeOffset? WebhookProvisionedAtUtc { get; set; }

    public DateTimeOffset? LastWebhookReceivedAtUtc { get; set; }

    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
```


**Answer:** Use **(b) keep manual creation for now and move auto-create to the next phase**.

Do not auto-create Stripe webhook endpoints during `ConfigureStripeProviderConnection` yet.

The current implementation already supports the required foundation: connection-scoped webhook handling and webhook secret storage. Keep webhook endpoint creation manual for the current phase while the provider connection lifecycle, environment handling, webhook URL generation, idempotency, secret rotation, and failure recovery rules are stabilised.

The next phase should add automatic Stripe webhook endpoint creation during provider onboarding. At that point the API should create the Stripe webhook endpoint, store the returned webhook endpoint ID, store the webhook signing secret in Key Vault, and mark webhook provisioning as complete.

Reference:

```text
docs/architecture/cinturon360-billing-stripe-architecture-reference.md
```

---

# Q18 — Expense Policy Billing Rule Model

## Question

Travel policy billing rule foundation exists:

```text
TravelPolicyBillingRule
```

Expense policies are not yet modeled.

## Decision Needed

Should Expense Policy billing rules reuse the same table/model with a `PolicyType` discriminator, or be a separate entity?

Options:

```text
(a) Single shared billing-rule model with discriminator
(b) Separate entity per policy domain
```

## Answer

Use **(a) single shared billing-rule model with discriminator**.

Expense policy billing rules should reuse the same billing-rule foundation as travel policy billing rules, with a policy discriminator such as `PolicyType`.

This keeps billing resolution consistent across policy domains and avoids duplicating payment method selection, billing model overrides, collection mode overrides, effective dating, provider customer resolution, and provider connection resolution.

Domain-specific behaviour should remain in the travel or expense policy engines.

Billing-specific behaviour should remain in the shared billing-rule resolver.


## Reference

This decision aligns with the canonical License v2 / billing architecture reference:

```text
docs/architecture/cinturon360-v5-billing-licensing-entitlements-accounting-reference.md
```

It also aligns with the provider-neutral billing reference:

```text
docs/architecture/cinturon360-billing-stripe-architecture-reference.md
```

The billing resolver should remain provider-neutral and policy-domain aware, without duplicating the same payment resolution model for each domain.


## Reasoning

Travel policies and expense policies are different policy domains, but their billing resolution requirements are mostly the same.

Both need to answer:

```text
Which organisation owns this policy?
Which seller/buyer licence applies?
Which payment provider connection should be used?
Which provider customer should be used?
Which payment method should be used?
Should the default organisation billing setup be used?
Is there a policy-specific override?
Is the billing model overridden?
Is the collection mode overridden?
What effective dates apply?
Is this billing rule currently enabled?
```

Because those concerns are shared, they should not be duplicated into separate tables unless the billing semantics become materially different later.

A shared model also allows the billing resolver to operate consistently:

```text
Organisation default billing
        ↓
Policy billing rule override
        ↓
Transaction override
        ↓
Resolved billing instruction
        ↓
Billing execution
```


## Recommended Model

Use a generic model:

```csharp
public sealed class PolicyBillingRule
{
    public required string Id { get; init; }

    public required string OrganisationId { get; init; }

    public PolicyType PolicyType { get; init; }

    public required string PolicyId { get; init; }

    public BillingResolutionMode ResolutionMode { get; init; }

    public string? PaymentProviderConnectionId { get; init; }

    public string? ProviderCustomerId { get; init; }

    public string? ProviderPaymentMethodId { get; init; }

    public BillingModel? BillingModelOverride { get; init; }

    public CollectionMode? CollectionModeOverride { get; init; }

    public bool RequirePaymentBeforeExecution { get; init; }

    public DateOnly EffectiveFrom { get; init; }

    public DateOnly? EffectiveTo { get; init; }

    public bool IsEnabled { get; set; } = true;

    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
```


## Policy Type

```csharp
public enum PolicyType
{
    Travel = 1,
    Expense = 2
}
```

Future policy domains can be added without introducing new billing-rule tables:

```csharp
public enum PolicyType
{
    Travel = 1,
    Expense = 2,
    Procurement = 3,
    CorporateCard = 4,
    Reimbursement = 5
}
```


## Billing Resolution Mode

```csharp
public enum BillingResolutionMode
{
    Default = 0,
    SpecificPaymentMethod = 1,
    SpecificBillingProfile = 2,
    Manual = 3,
    External = 4,
    NoCharge = 5
}
```

| Mode | Description |
|---|---|
| `Default` | Use organisation default billing setup. |
| `SpecificPaymentMethod` | Use a specific stored payment method. |
| `SpecificBillingProfile` | Use a specific billing profile or licence-linked billing setup. |
| `Manual` | Generate records/invoice but do not auto-collect. |
| `External` | Record billing but collection happens outside Cinturon360. |
| `NoCharge` | Used for internal, trial, waived, or shadow-only rules. |


## Billing Model

Use the agreed billing model split:

```csharp
public enum BillingModel
{
    Prepaid = 1,
    Postpaid = 2,
    PayAsYouGo = 3
}

public enum BillingAccountStatus
{
    Active = 1,
    Suspended = 2,
    Closed = 3,
    UnderReview = 4
}
```

`BillingModel` defines how spend is authorised.

`BillingAccountStatus` controls whether the billing account can currently transact.

Suspension should not be treated as a billing model.

## Collection Mode

```csharp
public enum CollectionMode
{
    None = 0,
    Manual = 1,
    Automatic = 2,
    ExternalReferenceOnly = 3
}
```


## Database Constraint Recommendation

The model should enforce deterministic policy billing rules.

Recommended unique constraint:

```text
OrganisationId
PolicyType
PolicyId
EffectiveFrom
```

Optional stricter rule:

```text
Only one active billing rule per OrganisationId + PolicyType + PolicyId for the same effective date window.
```

This prevents duplicate active rules for the same policy.


## Resolver Behaviour

The resolver should follow this order:

```text
1. Resolve organisation default billing setup.
2. Look for active PolicyBillingRule by OrganisationId + PolicyType + PolicyId.
3. If no policy rule exists, use organisation default.
4. If policy rule exists, apply allowed overrides.
5. Return ResolvedBillingInstruction.
```

Example output:

```csharp
public sealed class ResolvedBillingInstruction
{
    public required string PaymentProviderConnectionId { get; init; }

    public string? ProviderCustomerId { get; init; }

    public string? ProviderPaymentMethodId { get; init; }

    public BillingModel BillingModel { get; init; }

    public CollectionMode CollectionMode { get; init; }

    public BillingResolutionSource Source { get; init; }
}
```

```csharp
public enum BillingResolutionSource
{
    OrganisationDefault = 0,
    PolicyOverride = 1,
    TransactionOverride = 2
}
```

**Answer:** Use **(a) single shared billing-rule model with discriminator**.

Travel and expense policy billing rules should use one shared `PolicyBillingRule` model with a `PolicyType` discriminator.

This keeps billing resolution consistent across policy domains and avoids duplicating payment method selection, billing model overrides, collection mode overrides, effective dating, provider customer resolution, and provider connection resolution.

Use `PolicyType = Travel` for travel policies and `PolicyType = Expense` for expense policies.

Domain-specific behaviour should remain in the travel or expense policy engines. Billing-specific behaviour should remain in the shared billing-rule resolver.

Reference:

```text
docs/architecture/cinturon360-v5-billing-licensing-entitlements-accounting-reference.md
```

---

# Q19 — Org-Scope Webhook Resolution

## Question

Route supported:

```text
/api/v1/webhooks/stripe/{vendor|tmc|client}/{orgId}
```

Current behavior resolves the active primary Stripe provider connection for that org.

## Decision Needed

If an org has multiple active Stripe connections, how should the scope route choose one?

Options:

```text
(a) Reject with 409 until caller uses connection-specific endpoint
(b) Use latest verified connection
(c) Use explicit Primary flag on provider connection
```

## Answer

Use **(c) explicit `Primary` flag on provider connection**.

Org-scoped webhook routes must resolve only the active primary provider connection for that organisation and provider.

If more than one active primary connection exists, reject with a `409 Conflict` because the configuration is invalid.

If no active primary connection exists, reject with a `404 Not Found` or equivalent provider-not-configured response.

Connection-specific webhook endpoints should remain available for cases where the caller knows the exact provider connection.


## Reference

This decision aligns with the provider-neutral billing / Stripe architecture reference:

```text
docs/architecture/cinturon360-billing-stripe-architecture-reference.md
```

That reference recommends connection-specific webhook endpoints for provider webhooks and deterministic provider connection resolution.

## Recommended Behaviour

| Situation | Result |
|---|---|
| One active primary Stripe connection | Use it |
| Multiple active Stripe connections, one primary | Use the primary connection |
| Multiple active primary Stripe connections | Return `409 Conflict` |
| Multiple active Stripe connections, none primary | Return `409 Conflict` or provider configuration error |
| No active Stripe connection | Return `404 Not Found` |
| Connection-specific endpoint used | Use specified connection directly |

## Provider Connection Shape

```csharp
public sealed class PaymentProviderConnection
{
    public required string Id { get; init; }

    public required string OwnerOrgId { get; init; }

    public PaymentProviderType ProviderType { get; init; }

    public ProviderConnectionStatus Status { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsPrimary { get; set; }

    public bool IsLiveMode { get; init; }

    public ProviderUsageScope UsageScope { get; init; }

    public DateTimeOffset? VerifiedAtUtc { get; set; }

    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
```


## Provider Type

```csharp
public enum PaymentProviderType
{
    None = 0,
    Stripe = 1,
    Airwallex = 2,
    Braintree = 3,
    PayPal = 4,
    Square = 5,
    Manual = 6,
    External = 7
}
```


## Provider Connection Status

```csharp
public enum ProviderConnectionStatus
{
    PendingSetup = 0,
    PendingVerification = 1,
    Verified = 2,
    FailedVerification = 3,
    Disabled = 4
}
```


## Provider Usage Scope

```csharp
public enum ProviderUsageScope
{
    OwnerOnly = 0,
    DirectChildren = 1,
    Descendants = 2
}
```


## Database Constraint Recommendation

Enforce one active primary connection per organisation/provider/environment/scope.

Recommended uniqueness rule:

```text
OwnerOrgId
ProviderType
IsLiveMode
UsageScope
IsPrimary = true
IsEnabled = true
```

For PostgreSQL, this should be a filtered/partial unique index.

Conceptual example:

```sql
CREATE UNIQUE INDEX ux_payment_provider_connection_primary
ON billing.payment_provider_connections
(
    owner_org_id,
    provider_type,
    is_live_mode,
    usage_scope
)
WHERE is_primary = true
  AND is_enabled = true
  AND status = 'Verified';
```

This prevents multiple verified primary Stripe connections for the same owner org, environment, and usage scope.


## Preferred Endpoint Strategy

The org-scope route is convenient:

```text
/api/v1/webhooks/stripe/{vendor|tmc|client}/{orgId}
```

But the connection-specific route is more deterministic and audit-safe:

```text
/api/v1/webhooks/payment-providers/stripe/{connectionId}
```

Recommended:

```text
Provider-created webhook endpoints should use the connection-specific endpoint where possible.
Org-scope endpoints may remain as a compatibility/convenience route.
Org-scope routes must use the explicit primary connection rule.
```

## Why Not Latest Verified Connection

Do not use:

```text
latest verified connection
```

Reason:

```text
Non-deterministic
Changes behaviour when a new connection is verified
Can silently route provider events to the wrong account
Makes audit history harder
Can break existing webhook registrations
Dangerous if live/test connections coexist
```

Webhook routing must be stable, explicit, and audit-safe.

## Why Not Always 409

Option (a) is safer than latest verified, but too restrictive if the organisation has deliberately marked one connection as primary.

A `409 Conflict` should be used when configuration is ambiguous:

```text
Multiple active connections and no primary
Multiple active primaries
```


**Answer:** Use **(c) explicit `Primary` flag on provider connection**.

Org-scoped webhook routes must resolve only the active primary provider connection for that organisation and provider.

If more than one active primary connection exists, reject with a `409 Conflict` because the configuration is invalid.

If no active primary connection exists, reject with a `404 Not Found` or equivalent provider-not-configured response.

Connection-specific webhook endpoints should remain available for cases where the caller knows the exact provider connection.

Do not use “latest verified connection” because webhook routing must be deterministic and audit-safe.

Reference:

```text
docs/architecture/cinturon360-billing-stripe-architecture-reference.md
```


---

## Q20 — Flight Search UI: Live Provider or Stub Data?

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

## Q21 — Role-Based Navigation Layout

The page folder structure has `Sudo/`, `Vendor/`, `Tmc/`, `Client/` sections. These represent different user role groups.

**Options:**
- (a) **Separate layouts per role group** — different nav/sidebar depending on the user's role, e.g. a TMC user sees a different shell than a Client user
- (b) **One shared layout** — nav items shown or hidden based on claims/permissions
- (c) **One layout with separate route subtrees** — e.g. `/vendor/...`, `/tmc/...`, `/client/...`; same shell, different route namespaces

**Decision needed:** Which navigation model?

**Answer:**
Decision: Option C — one shared layout with separate route subtrees.

Use one consistent application shell, but keep role-group areas separated by route namespace:

```text
/sudo/...
/vendor/...
/tmc/...
/client/...
```

Reason: The application should feel like one unified product, not separate applications for each role group. A shared shell keeps branding, account controls, notifications, tenant switching, search, help, and general layout behaviour consistent. Separate route subtrees keep each role group cleanly organised and easier to secure, test, document, and reason about.

Navigation items inside the shared shell should be shown or hidden based on claims and permissions. The route namespace helps organise the application, but it must not be treated as the only security boundary. Access must still be enforced through authorization policies.

Do not create separate layouts per role group unless there is a genuine product-level reason. Do not let Sudo, Vendor, TMC, or Client areas infer different Tailwind or Fluent UI styling. The role/route determines access and navigation visibility, not the CSS framework.

Tailwind should act as the global design system, while Fluent UI Blazor should be available for specific operational components or pages where enterprise controls are needed.

Final model: one shared shell, separate role-based route namespaces, permission-filtered navigation, policy-based authorization, and page/component-level choice of Tailwind or Fluent UI where appropriate.

---

## Q22 — Approval Assignment Semantics

Phase 9 approvals now has a working pending list, detail page, and history page, but the current repository implementation behind `ListPendingForApproverAsync` does **not** actually filter by approver. It currently returns all pending approvals.

This means the UI workflow works technically, but the business rule for "who is allowed to approve what" is still undefined in code.

**Options:**
- (a) Filter by explicit approver assignment table per approval level
- (b) Filter by organisation + role-based approver rules
- (c) Filter by manager chain / org hierarchy
- (d) Keep current broad queue temporarily for vendor/TMC operations, then tighten later

**Decision needed:** Which approval assignment rule should Phase 9 enforce for pending approvals and approval actions?

**Current temporary implementation:** Option (d) in effect, because no approver-assignment model exists yet.

---

## Q23 — Web Auth State Management

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

## Q24 — Brand & Design Direction

Before building layouts and components, it helps to know what the UI should look and feel like.

**Options / information needed:**
- Existing logo, colour palette, and font choices
- A Figma file or design mockup link
- "Match the legacy v4 look" if there was a recognisable style
- No design yet — build functional first, style later

**Decision needed:** What is the brand/design direction for the Web UI?

**Answer:**

### Decision

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

### Logo & Typography

### Logo

The Cinturon360 wordmark is rendered as styled text using:

- **Font:** Plus Jakarta Sans
- **Weight:** 800 (extrabold)
- **Style:** `tracking-tight`, no letter-spacing expansion


- Segment: Cinturon; Colour: Sky Blue; Hex: `#38bdf8`; Tailwind Token: `sky-400`
- Segment: 360; Colour: Vibrant Orange; Hex: `#f97316`; Tailwind Token: `orange-500`


### Type Scale


- Role: Headings; Font: Plus Jakarta Sans; Weight: 700 / 800; Notes: `font-heading` Tailwind token
- Role: Body; Font: Inter; Weight: 400 / 500; Notes: `font-sans` Tailwind token
- Role: Mono; Font: System mono stack; Weight: 400; Notes: Code, data, IDs


Font weights:

```text
Inter: 400, 500, 600, 700
Plus Jakarta Sans: 600, 700, 800
```

Fonts should use an approved delivery strategy and should not create avoidable runtime dependency or compliance risk.

### Colour Palette

The application must support both light and dark themes. Colour tokens should be semantic, not hard-coded directly into components.

### Brand Primaries


- Name: Sky / Cinturon; Hex: `#38bdf8`; Usage: Primary CTA, links, active states, accents
- Name: Orange / 360; Hex: `#f97316`; Usage: Secondary accent, highlights, badges, brand moments


Primary actions should usually use Sky. Orange should be reserved for secondary emphasis, highlights, alerts, brand moments, or conversion-oriented CTAs.

### Light Theme Tokens


- Token: `background`; Suggested Value: `#f8fafc`; Usage: Main app background
- Token: `surface`; Suggested Value: `#ffffff`; Usage: Cards, panels, page sections
- Token: `surface-muted`; Suggested Value: `#f1f5f9`; Usage: Secondary sections, table headers
- Token: `surface-hover`; Suggested Value: `#e2e8f0`; Usage: Hover and selected row states
- Token: `border-subtle`; Suggested Value: `rgba(15,23,42,0.08)`; Usage: Card edges, dividers
- Token: `border-strong`; Suggested Value: `rgba(15,23,42,0.14)`; Usage: Inputs, menus, modals
- Token: `text-primary`; Suggested Value: `#0f172a`; Usage: Primary headings and body
- Token: `text-secondary`; Suggested Value: `#334155`; Usage: Secondary body text
- Token: `text-muted`; Suggested Value: `#64748b`; Usage: Hints, placeholders, metadata
- Token: `text-disabled`; Suggested Value: `#94a3b8`; Usage: Disabled and tertiary text


### Dark Theme Tokens


- Token: `background`; Suggested Value: `#05080f`; Usage: Main app background
- Token: `surface`; Suggested Value: `#0d1424`; Usage: Cards, panels, page sections
- Token: `surface-muted`; Suggested Value: `#111827`; Usage: Secondary sections, table headers
- Token: `surface-hover`; Suggested Value: `#1e2a3a`; Usage: Hover and selected row states
- Token: `border-subtle`; Suggested Value: `rgba(255,255,255,0.07)`; Usage: Card edges, dividers
- Token: `border-strong`; Suggested Value: `rgba(255,255,255,0.12)`; Usage: Inputs, menus, modals
- Token: `text-primary`; Suggested Value: `#f8fafc`; Usage: Primary headings and body
- Token: `text-secondary`; Suggested Value: `#cbd5e1`; Usage: Secondary body text
- Token: `text-muted`; Suggested Value: `#94a3b8`; Usage: Hints, placeholders, metadata
- Token: `text-disabled`; Suggested Value: `#64748b`; Usage: Disabled and tertiary text


### Semantic / Status Colours


- Name: Success; Hex: `#22c55e`; Usage: Approved, confirmed, on-policy
- Name: Warning; Hex: `#f59e0b`; Usage: Pending approval, near-limit policy breach
- Name: Danger; Hex: `#ef4444`; Usage: Out-of-policy, failed, rejected
- Name: Info; Hex: `#38bdf8`; Usage: Informational, in-progress, travel context hints
- Name: Purple; Hex: `#a855f7`; Usage: Premium, VIP tier, loyalty level indicators


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

### Theme Preference

The application must support a visible light/dark mode switch.

Theme behaviour:

- The logged-in user's saved theme preference is the source of truth.
- The selected theme is stored as part of the user's profile.
- The selected theme must be restored automatically on sign-in.
- When the user changes the theme, the profile preference should be updated.
- The UI should apply the theme consistently across Tailwind-styled components and Fluent UI Blazor components.
- Fluent UI Blazor components must be themed to align with the Cinturon360 design tokens and must not introduce a separate visual language.

Theme preference values:


- Value: `light`; Meaning: Force light mode
- Value: `dark`; Meaning: Force dark mode
- Value: `system`; Meaning: Follow the user's device/browser preference


Default:

```text
system
```

### Language & Localisation

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


- Preference: Theme; Example: `light`, `dark`, `system`
- Preference: Language; Example: `en-AU`, `en-NZ`, `en-US`, etc.
- Preference: Time zone; Example: `Australia/Sydney`, `Pacific/Auckland`, etc.
- Preference: Locale / culture; Example: `en-AU`, `en-NZ`, etc.


### Date, Time & Time Zone Behaviour

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


- Scenario: Create approval deadline; UI Behaviour: User enters local date/time; Storage Behaviour: Store UTC
- Scenario: Schedule report; UI Behaviour: User selects local date/time; Storage Behaviour: Store UTC
- Scenario: Add reminder; UI Behaviour: User enters local date/time; Storage Behaviour: Store UTC
- Scenario: Update support SLA due time; UI Behaviour: User sees local date/time; Storage Behaviour: Store UTC
- Scenario: Create internal task; UI Behaviour: User enters local date/time; Storage Behaviour: Store UTC


### Read-only Provider/Search Result Data

Flight, hotel, car, itinerary, and booking-provider search results are different.

For provider/search result data:

- Display the local date/time as supplied by the provider or booking context.
- Do not convert provider-local travel times into the user's profile time zone if that would change the travel meaning.
- Treat flight departure/arrival times, hotel check-in/check-out times, and itinerary segment times as location-contextual read-only data.
- Preserve provider/local travel context in the UI.
- Clearly label time zones or location context where ambiguity may exist.

Examples:


- Scenario: Flight departs Sydney at 10:25; UI Behaviour: Show Sydney-local departure time
- Scenario: Flight arrives Singapore at 16:40; UI Behaviour: Show Singapore-local arrival time
- Scenario: Hotel check-in at 15:00; UI Behaviour: Show hotel-local check-in time
- Scenario: Car pickup at airport local time; UI Behaviour: Show pickup-location local time
- Scenario: Search result fare expiry; UI Behaviour: Show provider/context time with clear label


Rule:

```text
Application-owned editable data: user-local input/output, UTC storage.
Provider/search result data: preserve and display travel/provider-local context as read-only.
```

### Design System Direction

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


- Screen Type: End-user / Traveller; Tone & Feel: Guided, consumer-grade, warm, clear, step-by-step
- Screen Type: Back-office / Admin; Tone & Feel: Professional, structured, data-dense, enterprise


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

### Brand Voice in UI

- Prioritise clarity and confidence
- Avoid jargon without context
- Labels should describe action outcomes, not technical mechanics
- Policy, approval, and compliance messaging should be firm but not punitive
- Travel context should feel aspirational, clean, and practical
- Localised text should preserve the intended meaning, not just literal wording

### Implementation Path

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

## Q25 — API Communication from Web: HttpClient or Typed Client?

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

## Q26 — Error & Validation UX

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

## Q27 — Phase 10 Provider Credentials & Production Defaults

Phase 10 implementation has started and now includes real MailerSend and Stripe runtime integration (with safe fallback when keys are missing), plus S3/R2 env wiring.

To complete Phase 10 fully without stubs, we still need the final production decisions below.

**Decision needed:** Which provider/credential set should be treated as primary for each domain in production?

Please provide:
- MailerSend API token + sender identity (`FromEmail`, `FromName`)
- Stripe live/test mode policy + keys (`SecretKey`, `PublishableKey`, `WebhookSecret`)
- Cloudflare R2 bucket + endpoint + access credentials
- Amadeus credentials (self-service or enterprise)
- Duffel credentials (token + environment)
- FX provider choice (`OpenExchangeRates` / `Frankfurter` / `ECB`) and API key if required
- GitHub ticketing target (`owner/repo`) and PAT/App strategy

**Autopilot default applied for now:**
- Keep system operational with config-based fallback mode when provider keys are missing.
- Do not hard-fail auth or billing flows in dev when external providers are not configured.

---

## Q28 — Hotels/Cars/Rail Provider Selection

Phase 10 scope references Hotels/Cars/Rail, but no provider has been selected yet.

**Decision needed:** Which providers should be used for initial rollout?

Suggested baseline options:
- Hotels: Booking.com Demand API or Expedia Rapid
- Cars: Amadeus Cars or Rentalcars affiliate APIs
- Rail: Trainline Partner API or Rail Europe B2B

If no decision is made immediately, these modules should remain explicitly deferred to a later slice of Phase 10 while flights/email/payments/storage are finalized first.

---


---

## Implementation Status Update (2026-05-03)

### Q17 — Stripe Webhook Auto-Provisioning
Auto-provisioning was implemented in a prior session. The full flow (`VerifyConnection` → `RegisterWebhookEndpoint` → store webhook secret → `MarkVerified`) is complete and smoke-tested (12 scenarios passing). This question is superseded.

### Q16 — License Agreement Architecture
`LicenseAgreement`, `LicenseAgreementEntitlement`, `LicenseCollectionPolicy`, `BillingAccount`, and accounting stub entities (`BillingLedgerEntry`, `BillingInvoice`, `BillingInvoiceLine`, `PaymentAttempt`, `JournalEntry`, `JournalLine`) created in Domain. EF configurations added. Migration `RefactorBillingModelToLicenseAgreement` generated and applied. Application commands added: `CreateLicenseAgreementCommand`, `ActivateLicenseAgreementCommand`, `SupersedeLicenseAgreementCommand`.

### Q18 — Policy Billing Rule Discriminator
`TravelPolicyBillingRule` renamed to `PolicyBillingRule` with `PolicyType` enum discriminator + `PolicyId` string. `travel_policy_billing_rules` table dropped and replaced by `policy_billing_rules` (dev-only data, user confirmed DROP+CREATE). Migration applied.

### Q19 — IsPrimary on PaymentProviderConnection
`IsPrimary` (bool, default false) and `WebhookProvisionedAtUtc` (DateTimeOffset?) added to `PaymentProviderConnection`. Partial unique index `ux_payment_provider_connection_primary` enforces uniqueness on `(OwnerOrganisationId, ProviderType, IsLiveMode, UsageScope)` where `is_primary = true AND is_enabled = true AND status = 2`. `SetPrimaryProviderConnectionCommand` added. Org-scoped webhook handler (`/webhooks/stripe/{orgScope}/{orgId}`) now uses `IsPrimary`-aware query. New endpoint `PUT /api/v1/billing/provider-connections/{connectionId}/primary` added.

