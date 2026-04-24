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

*Last updated: initial v5 scaffold session*
