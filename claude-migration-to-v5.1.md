# Cinturon360 v5 to v5.1 Migration Discovery

> Discovery date: 2026-05-10
> Prepared from a read-only inspection of the working tree at the repository root.
> No source code, configuration, migrations, or documentation files were modified during this discovery.

## Executive Summary

Cinturon360 v5 is a multi-project .NET 10 platform that targets the Vendor → TMC → Client travel-management hierarchy. The repository is in a mid-build state: scaffolding, planning, and Phase 0–9 implementation are largely complete, while Phase 10 (real integrations) is partially complete and Phases 11–13 (deployment, observability, legacy cleanup) are not started.

The implementation is broadly faithful to the planning documents (`v5-plan.md`, `v5-solution-structure.md`, `QUESTIONS.md`), but several gaps and conflicts have accumulated:

- The full Application security model (`AppClaims`, `AppPolicies`, `AppRoles`, `OrgRoleRequirements`, `OrgRoles`, `PermissionEvaluation`, `TokenIssuance`) is empty; only `.gitkeep` files exist in those folders.
- Endpoints rely on `RequireAuthorization()` with no permission, role, or org-scope policies attached. The `AuthorizationBehavior<,>` MediatR pipeline is wired, but no command or query implements `IRequirePermission`.
- The billing domain now contains three concurrent generations of entities in the same `DbContext`: legacy (`OrgLicense`, `Invoice`, `Payment`, `PrepaidBalance`), provider-neutral (`PaymentProviderConnection`, `BillingRelationship`, etc.), and License v2 + accounting stubs (`LicenseAgreement`, `BillingAccount`, `JournalEntry`, etc.). The execution layer for any of the three is not yet implemented.
- The `Cinturon360.Jobs` container claims to be Supercronic-driven; the Dockerfile installs `supercronic` but the entrypoint is `dotnet Cinturon360.Jobs.dll`, and registered jobs are `IHostedService` background services, not crontab entries.
- All 7 test projects contain only `UnitTest1.Test1()` empty stubs. There are zero real unit, integration, architecture, or end-to-end tests.
- No GitHub Actions workflows exist in `.github/workflows/`. No production compose, no Caddy configuration, and no Azure Bicep are present in `deploy/`.
- Documentation is fragmented across ~10 Markdown files at the repository root. The intended `docs/` Obsidian structure is partially implemented (`docs/architecture`, `docs/runbooks`, `docs/api`, `docs/policies`) but nearly empty.
- A placeholder migration named `YourMigrationName` is committed and applied to local databases.

The code is generally well-organised and the layer boundaries are mostly respected. v5.1 should preserve the existing source tree structurally and prioritise (1) finishing the License v2 execution layer, (2) replacing the `AuthorizationBehavior` no-op with real permission enforcement, (3) tearing out the legacy billing model after migration, (4) adding real tests, (5) adding CI/CD and deployment scaffolding, and (6) consolidating the fragmented documentation into the planned Obsidian structure.

## Repository Snapshot

Top-level layout.

```text
cinturon360.dev/
├── .editorconfig
├── .github/                        # contains only an agent prompt; no workflows
├── .gitignore
├── BACKLOG.md
├── Cinturon360.slnx                # XML solution file, NOT Cinturon360.sln
├── Directory.Build.props
├── Directory.Packages.props
├── claude-code-v5-to-v5-1-migration-discovery-prompt.md
├── claude-migration-to-v5.1.md     # this file
├── deploy/
│   ├── azure/                      # empty
│   ├── caddy/                      # empty
│   ├── compose/compose.dev.yaml
│   └── docker/{Dockerfile.api,Dockerfile.web,Dockerfile.jobs}
├── dev-start.sh
├── docs/
│   ├── api/support-ticket-email-templates.md
│   ├── architecture/{stripe-payments.md, cinturon360-billing-stripe-architecture-reference.md,
│   │                  cinturon360-v5-billing-licensing-entitlements-accounting-reference.md}
│   ├── policies/                   # empty
│   └── runbooks/runbook-stripe-payments.md
├── ecb-data.json                   # sample ECB API response, root-level
├── ecb-exchange-rate-api-ingestion.md
├── github-ticketing-setup.md
├── global.json                     # SDK 10.0.105
├── legacy-reference/               # full v4 source archive
├── notes.txt                       # informal session notes
├── QUESTIONS.md                    # 25 Q&A items, 2 248 lines
├── README.md
├── REMAINING-PHASES.md
├── SESSION-NOTES-2026-04-27.md
├── src/                            # 10 projects
├── tests/                          # 7 projects, empty
├── tools/
│   ├── codegen/                    # empty
│   ├── db/migrations/              # generated SQL scripts
│   ├── db/system-account/          # working CLI
│   └── maintenance/                # empty
├── travel_platform_arch.svg
├── v5-plan.md
└── v5-solution-structure.md
```

Source-file counts (excluding `bin/`, `obj/`, `node_modules/`, `legacy-reference/`):

- `src/` C# files: 332
- `src/Cinturon360.Domain/` C# files: 60
- `src/Cinturon360.Data/` C# files: 70 (includes 8 migrations + designer files)
- `src/Cinturon360.Application/Features/**/*.cs`: 64 (15 `*Handler.cs` files)

Evidence:

- `Cinturon360.slnx`
- `Directory.Packages.props`
- `global.json`
- `README.md`

## Application Purpose

Cinturon360 is a multi-tenant SaaS platform for travel operations. The product covers:

- corporate travel booking (flights via Duffel; Amadeus and other providers planned),
- approvals and travel policy enforcement,
- billing, licensing, payment-method management, invoicing, and (planned) accounting,
- support ticketing backed by GitHub Issues,
- a Blazor Server back-office and traveller-facing UI,
- background jobs for FX rate ingestion, document cleanup, and (planned) more.

The intended commercial structure is a Vendor → TMC → Client hierarchy where the seller organisation owns the licence agreement that defines billing terms with the buyer organisation. Standard tenant users have exactly one home org and access flows downward only. Platform "sudo" users have no home org and bypass the hierarchy.

Evidence:

- `v5-plan.md` §2 "Core Platform Constraints", §5 "Org Hierarchy & Access Model", §8 "Billing & Payments"
- `docs/architecture/cinturon360-billing-stripe-architecture-reference.md`
- `docs/architecture/cinturon360-v5-billing-licensing-entitlements-accounting-reference.md`
- `notes.txt` (TMC chain/branch codes, search functions)
- `github-ticketing-setup.md`

## Current Technology Stack

| Concern                  | Choice                                                                 | Source                          |
| ------------------------ | ---------------------------------------------------------------------- | ------------------------------- |
| .NET SDK                 | 10.0.105 (rollForward latestPatch)                                     | `global.json`                   |
| Target framework         | net10.0                                                                | `Directory.Build.props`         |
| Solution file            | `Cinturon360.slnx` (XML) — README still says "Cinturon360.sln"        | repo root, `README.md`          |
| Central package mgmt     | `Directory.Packages.props` with transitive pinning                     | `Directory.Packages.props`      |
| Database                 | PostgreSQL (18-alpine3.22 in dev compose; plan/README say 16/17)       | `deploy/compose/compose.dev.yaml` |
| ORM                      | EF Core 10.0.4 + Npgsql 10.0.x + EFCore.NamingConventions (snake_case) | `src/Cinturon360.Data/*.csproj` |
| Application messaging    | MediatR 12.4.1                                                         | `Directory.Packages.props`      |
| Validation               | FluentValidation 11.11.0                                               | `Directory.Packages.props`      |
| Mapping                  | Mapster 10.0.7                                                         | `Directory.Packages.props`      |
| Auth (API)               | JwtBearer only at runtime; OpenIddict packages installed but unused   | `src/Cinturon360.Api/Program.cs` |
| Auth (Web)               | Cookie-based BFF (`c360.bff`) wrapping API JWT                         | `src/Cinturon360.Web/DependencyInjection/WebServiceRegistration.cs` |
| UI                       | Blazor Server (`AddInteractiveServerComponents`) + Tailwind + Fluent UI | `src/Cinturon360.Web/Program.cs`, `tailwind.config.js` |
| Logging                  | Serilog 4.2.0 + Console + Postgres alternative + correlation enricher  | `src/Cinturon360.Api/Program.cs`, `Directory.Packages.props` |
| Resilience               | Polly 8.5.2 + Microsoft.Extensions.Http.Resilience 9.6.0               | `Directory.Packages.props`      |
| PDF                      | QuestPDF 2025.4.0 (Community license)                                  | `src/Cinturon360.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs` |
| Object storage           | AWSSDK.S3 4.0.4 against Cloudflare R2 (S3-compatible)                  | `src/Cinturon360.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs` |
| Payments                 | Stripe.net 47.3.0                                                      | `Directory.Packages.props`      |
| Secrets                  | Azure.Security.KeyVault.Secrets 4.8.0 + in-memory dev fallback         | `src/Cinturon360.Infrastructure/Secrets/` |
| Architecture tests       | TngTech.ArchUnitNET 0.13.3                                             | `Directory.Packages.props`      |
| FX provider              | ECB SDMX (custom HTTP client)                                          | `src/Cinturon360.Integrations/ExchangeRates/EcbExchangeRateProvider.cs` |
| Email provider           | MailerSend (custom HTTP client; SDK not used)                          | `src/Cinturon360.Infrastructure/Email/MailerSendEmailService.cs` |
| Ticketing backend        | GitHub Issues REST API                                                 | `src/Cinturon360.Integrations/GitHub/Services/GitHubIssuesClient.cs` |
| Auth library (planned)   | ITfoxtec.Identity.Saml2 4.17.0; OpenIddict 5.4.0                       | `Directory.Packages.props`      |

## Solution and Project Layout

The solution is declared in `Cinturon360.slnx` and contains 10 source projects, 7 test projects, and 1 tool project.

| Project                        | SDK                                | References                                                            |
| ------------------------------ | ---------------------------------- | --------------------------------------------------------------------- |
| `Cinturon360.Api`              | `Microsoft.NET.Sdk.Web`            | Contracts, Application, Data, Infrastructure, Domain                  |
| `Cinturon360.Web`              | `Microsoft.NET.Sdk.Web`            | Contracts, Domain                                                     |
| `Cinturon360.Application`      | `Microsoft.NET.Sdk`                | Common, Contracts, Domain, Integrations                               |
| `Cinturon360.Domain`           | `Microsoft.NET.Sdk`                | Common                                                                |
| `Cinturon360.Contracts`        | `Microsoft.NET.Sdk`                | Common, Domain                                                        |
| `Cinturon360.Common`           | `Microsoft.NET.Sdk`                | (none)                                                                |
| `Cinturon360.Data`             | `Microsoft.NET.Sdk`                | Common, Domain, Application                                           |
| `Cinturon360.Integrations`     | `Microsoft.NET.Sdk`                | Common, Contracts                                                     |
| `Cinturon360.Infrastructure`   | `Microsoft.NET.Sdk`                | Common, Application, Integrations                                     |
| `Cinturon360.Jobs`             | `Microsoft.NET.Sdk.Worker`         | Application, Data, Infrastructure, Integrations                       |
| `tools/db/system-account`      | `Microsoft.NET.Sdk` (CLI)          | Cinturon360.Common (referenced inline)                                |

### Reference-graph deviations from `v5-solution-structure.md`

| Plan rule                                                | Actual                                                           | Impact                                            |
| -------------------------------------------------------- | ---------------------------------------------------------------- | ------------------------------------------------- |
| "Web references Contracts only"                          | `Cinturon360.Web` references **Contracts and Domain**            | Domain entities can leak into UI                  |
| "Contracts: stable request/response, no business logic"  | `Cinturon360.Contracts` references **Domain**                    | Contracts can leak `Entity` types                 |
| "Application references Domain and Contracts"            | `Cinturon360.Application` also references **Integrations**       | Provider models can leak into use-case code       |
| "Data implements Application abstractions"               | `Cinturon360.Data` references **Application** directly           | Ok per inversion via abstractions, but tight      |
| "Architecture tests enforce the above"                   | `Cinturon360.Architecture.Tests` is empty                        | No automated layer-boundary enforcement           |

Evidence: each project's `*.csproj`, `Cinturon360.slnx`, `v5-solution-structure.md` lines 476–488.

## Application Runtime Layout

Three runnable hosts, three Docker images.

| Host                  | Type                | Internal port | Compose port | Auth scheme                                                             |
| --------------------- | ------------------- | ------------- | ------------ | ----------------------------------------------------------------------- |
| `Cinturon360.Api`     | ASP.NET Core Web    | 8080          | 5100         | `JwtBearerDefaults.AuthenticationScheme` (HMAC-SHA256, 30–60 min token) |
| `Cinturon360.Web`     | Blazor Server       | 8080          | 5001         | `CookieAuthenticationDefaults` (BFF) wrapping API JWT                   |
| `Cinturon360.Jobs`    | .NET Worker host    | n/a           | n/a          | (none — runs without HTTP)                                              |

Evidence:

- `src/Cinturon360.Api/Program.cs:43–62`
- `src/Cinturon360.Web/Program.cs:11–66`
- `src/Cinturon360.Jobs/Program.cs:1–17`
- `deploy/compose/compose.dev.yaml`

## Infrastructure and Deployment Layout

The `deploy/` tree contains:

```text
deploy/
├── azure/                       # empty (Phase 11 not started)
├── caddy/                       # empty (Phase 11 not started)
├── compose/compose.dev.yaml     # dev-only postgres + api + web + jobs
└── docker/
    ├── Dockerfile.api           # multi-stage SDK 10 → aspnet:10.0 runtime, non-root
    ├── Dockerfile.jobs          # installs supercronic but ENTRYPOINT runs the .NET worker
    └── Dockerfile.web           # adds Node.js for Tailwind, persists DataProtection keys
```

Notable points:

- `compose.dev.yaml` declares Postgres image `postgres:18-alpine3.22`. The `README.md` says Postgres 17. `v5-plan.md §13` says `postgres:16`. Three values, three sources.
- The compose Postgres volume is mounted at `/var/lib/postgresql` (per `SESSION-NOTES-2026-04-27.md`, this was changed from `/var/lib/postgresql/data` for Postgres 18).
- API container does **not** run EF migrations on start. Migrations are generated by `dotnet ef migrations script --idempotent` and applied with `psql` via `dev-start.sh`.
- `Dockerfile.jobs` installs `supercronic` (`v0.2.33`) into the image but does not invoke it — the entrypoint is `dotnet Cinturon360.Jobs.dll` and recurring work runs as `IHostedService` background services. The `# QUESTION: confirm base image — see QUESTIONS.md Q7` comment is left in place.
- All three Dockerfiles drop to a non-root `appuser`.
- No `Caddyfile`, no `compose.staging.yaml`, no `compose.prod.yaml`, no Bicep, no ARM, no Pulumi, no Terraform.

Evidence:

- `deploy/docker/Dockerfile.api:1–35`
- `deploy/docker/Dockerfile.jobs:25–43`
- `deploy/docker/Dockerfile.web:1–35`
- `deploy/compose/compose.dev.yaml`
- `dev-start.sh`
- `SESSION-NOTES-2026-04-27.md`

## Local Development Model

Single entry point: `dev-start.sh`.

The script does, in order:

1. Bring up only the `postgres` service from `compose.dev.yaml` and wait for `pg_isready`.
2. Run `dotnet ef migrations list` against `src/Cinturon360.Data` with startup project `src/Cinturon360.Api`.
3. For each migration, generate a numbered idempotent SQL file in `tools/db/migrations/` if not already present.
4. Apply each `tools/db/migrations/*.sql` file via `docker compose exec -T postgres psql -v ON_ERROR_STOP=1`.
5. Bring up `api`, `web`, `jobs` with `docker compose up --build -d`.

Side-effects of the current setup:

- The script uses `dotnet ef migrations script PREV NEXT` with a per-migration loop. This produces an SQL file per migration step, but if the EF model changes without a new migration the existing SQL files are not regenerated.
- The script keeps stale SQL files in `tools/db/migrations/`. The directory currently contains both `003_20260426142026_YourMigrationName.sql` and `003_20260427002017_YourMigrationName.sql` — two competing files for the same numbered slot.
- A standalone CLI at `tools/db/system-account/` (Cinturon360.SystemAccountTool, `osx-arm64` publish included in `bin/Release/.../publish/`) is documented in the README as the only way to create the platform Sudo account.

Evidence:

- `dev-start.sh:1–76`
- `tools/db/migrations/`
- `tools/db/system-account/README.md`
- `README.md` lines 41–143
- `SESSION-NOTES-2026-04-27.md`

## Configuration Model

Configuration lives in per-host `appsettings.json` + `appsettings.Development.json`.

Key sections used by the API today:

| Section            | Used by                                  | Example value                                                                     |
| ------------------ | ---------------------------------------- | --------------------------------------------------------------------------------- |
| `Jwt`              | `Cinturon360.Infrastructure.Security.JwtSettings` | secret key, issuer, audience, `AccessTokenExpiryMinutes`                          |
| `ConnectionStrings:Postgres` | `Cinturon360.Data.DependencyInjection`           | `Host=localhost;Port=5432;Database=cinturon360;Username=cinturon;Password=...`    |
| `MailerSend`       | `MailerSendEmailService`                 | `ApiToken`, `FromEmail`, `FromName`, `BaseUrl`                                    |
| `Stripe`           | `StripePaymentGateway`                   | `SecretKey`, `PublishableKey`, `WebhookSecret`                                    |
| `Storage`          | `S3StorageService`                       | `BucketName`, `Endpoint`, `AccessKeyId`, `SecretAccessKey`, `PublicBaseUrl`       |
| `KeyVault:VaultUri`| Picks Azure or in-memory secret store    | unset → `InMemorySecretStore`                                                     |
| `GitHub`           | `GitHubIssuesClient`                     | `TicketingPat`, `TicketingOwner`, `TicketingRepo`                                 |
| `Api:BaseUrl`      | Web → API typed clients                  | `http://api:8080` in compose                                                      |
| `Api:PublicBaseUrl`| API self-reference (link emails, etc.)   | `https://localhost:7206` in dev                                                   |
| `DP_KEYS_PATH`     | Web Data Protection                      | `/app/dp-keys` (mounted volume)                                                   |

Notable observations:

- `appsettings.json` for the API ships the placeholder JWT secret `REPLACE_WITH_SECRET_KEY_MIN_32_CHARS_LONG`. `appsettings.Development.json` ships the literal `dev-secret-key-change-me-in-production-32chars`. Production secret-management policy is documented but not enforced (no `KeyVault` references in current `appsettings.*.json`).
- There is no environment-specific `appsettings.Staging.json` or `appsettings.Production.json` in any host project.

Evidence:

- `src/Cinturon360.Api/appsettings.json:9–35`
- `src/Cinturon360.Api/appsettings.Development.json`
- `src/Cinturon360.Web/appsettings.json`
- `src/Cinturon360.Infrastructure/Security/JwtSettings.cs`

## Database Model

A single `AppDbContext` in `src/Cinturon360.Data/Context/AppDbContext.cs` registers approximately 50 `DbSet` properties grouped into nine logical sections:

- Identity (User + 11 satellite tables)
- Traveller profile (TravellerProfile + 4 satellites)
- Organisation (Organisation, Role, RolePermission)
- Geography (Country, City, Airport, DuffelOrgConfiguration)
- Bookings (Booking, BookingItem, Quote)
- Policy (TravelPolicy, PolicyRule, PolicyAssignment)
- Approvals (ApprovalRequest, ApprovalDecision)
- Billing (≈22 tables across legacy, provider-neutral, and License v2)
- System (Job, StoredDocument, ExchangeRate, SupportTicket + 4 ticketing satellites)

Naming conventions:

- snake_case (via `EFCore.NamingConventions.UseSnakeCaseNamingConvention()`).
- Soft-delete columns implemented via `SoftDeletableEntity` base (used by `User`, `Organisation`, `Booking`, `Invoice`, `Role`, `SupportTicket`).
- Audit columns (`CreatedAt`, `UpdatedAt` `DateTimeOffset`) on the `Entity` base.
- ID columns are `string`-typed with prefixed IDs (`usr_`, `org_`, `bkg_`, `ses_`, `inv_`, etc.) generated by `Cinturon360.Common.IdGeneration.IdGenerator`.

Migrations:

| Order | Migration                                                        | Notes                                                            |
| ----- | ---------------------------------------------------------------- | ---------------------------------------------------------------- |
| 1     | `20260424155855_InitialCreate`                                   | Identity, Org, Roles, OpenIddict tables                          |
| 2     | `20260424162508_Phase3To7_CoreSchema`                            | Bookings, Policy, Approvals, Billing v1, Geography, Storage      |
| 3     | `20260426142026_YourMigrationName`                               | **Empty Up/Down** — placeholder shipped to `main`                |
| 3 (alt SQL) | `003_20260427002017_YourMigrationName.sql`                  | Duplicate file with different timestamp present in `tools/db/migrations/` |
| 4     | `20260427020326_Phase9_PasswordResetToken`                       | Adds `password_reset_token_hash` and expiry to `user_security`   |
| 5     | `20260430130112_AddTicketingAndOrgSupportTeamName`               | SupportTicket, TicketComment, TicketEscalation, OrgSupportTeam   |
| 6     | `20260430133702_AddTicketAttachments`                            | TicketAttachment                                                 |
| 7     | `20260430143000_AddTicketEmailTemplatesAndNotifications`         | TicketEmailTemplate                                              |
| 8     | `20260430163725_AddDuffelConfigAndFxSnapshotRetention`           | DuffelOrgConfiguration, ExchangeRate retention                   |
| 9     | `20260501172954_AddProviderNeutralBillingArchitecture`           | PaymentProviderConnection, BillingRelationship, ProviderCustomer, etc. |
| 10    | `20260503100359_RefactorBillingModelToLicenseAgreement`          | LicenseAgreement, LicenseAgreementEntitlement, LicenseCollectionPolicy, BillingAccount, accounting stubs |

Seed data: `src/Cinturon360.Data/Seeds/` exists only as `.gitkeep`. There are no platform roles seeded, no permission codes seeded, no geography seeded, no ticket-template seed.

Interceptors: `src/Cinturon360.Data/Interceptors/` exists only as `.gitkeep`. No EF interceptors are registered. The plan documents an audit interceptor and a soft-delete interceptor; neither is implemented.

Repositories: hand-written repositories in `src/Cinturon360.Data/Repositories/` for Approval, Billing, Booking, DuffelOrgConfiguration, ExchangeRate, Job, Organisation, Permission, SupportTicket, TicketEmailTemplate, TravelPolicy, TravellerProfile, Quote, StoredDocument, User, UserApiToken, UserSecurity, UserSession, plus `UnitOfWork`.

Evidence:

- `src/Cinturon360.Data/Context/AppDbContext.cs`
- `src/Cinturon360.Data/Migrations/20260426142026_YourMigrationName.cs:11–22`
- `src/Cinturon360.Data/Migrations/AppDbContextModelSnapshot.cs`
- `src/Cinturon360.Data/Seeds/.gitkeep`
- `src/Cinturon360.Data/Interceptors/.gitkeep`
- `tools/db/migrations/`

## Entity Framework Core Usage

Configuration is loaded with `ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly)`. Configuration files are organised under `src/Cinturon360.Data/Configurations/{Approval,Billing,Booking,Identity,Integrations,Jobs,Logging,Organization,Policy,Pricing,Storage,System,Ticketing,Travel}/`.

Key behaviours:

- `services.AddDbContext<AppDbContext>(...)` uses `UseNpgsql(...).UseSnakeCaseNamingConvention().UseOpenIddict()` (Data DI).
- `npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)` keeps migrations co-located with `AppDbContext`.
- `npgsql.EnableRetryOnFailure(maxRetryCount: 5)` is set unconditionally.
- `OpenIddict.EntityFrameworkCore` tables are created by `UseOpenIddict()`, but no OpenIddict feature is wired in the API runtime — see Authentication Model.
- A design-time factory (`AppDbContextFactory.cs`) is present for `dotnet ef` tooling.

Evidence:

- `src/Cinturon360.Data/DependencyInjection/ServiceCollectionExtensions.cs:18–29`
- `src/Cinturon360.Data/Context/AppDbContext.cs:108–117`

## Domain Model Overview

The Domain project is structured by area. Highlights:

- `Cinturon360.Domain.Common.Base` defines `Entity` (immutable Id, audit timestamps) and `SoftDeletableEntity` (adds `IsDeleted`, `DeletedAt`).
- All entities use private setters and static factory methods (`User.Create(...)`, `Organisation.Create(...)`, `Booking.Create(...)`).
- Several files contain multiple entities together; some are very large:
  - `Domain/Entities/Billing/BillingEntities.cs` — 1 088 lines, 22 entity classes (legacy, provider-neutral, License v2, accounting stubs).
  - `Domain/Entities/Policy/PolicyEntities.cs`
  - `Domain/Entities/Approval/ApprovalEntities.cs`
  - `Domain/Entities/System/SystemEntities.cs`
  - `Domain/Entities/Travel/GeographyEntities.cs`
- `Domain/Common/ValueObjects/` is referenced by `Booking` (`ValueObjects.Money`-style). Detailed VO list not exhaustively confirmed.
- `Domain/Events/`, `Domain/Services/`, `Domain/Specifications/` exist but contain only `.gitkeep` placeholders.

## Organisation and Tenancy Model

The `Organisation` aggregate (`src/Cinturon360.Domain/Entities/Organization/Organisation.cs`) is the tenancy unit. Key fields:

- `Id`, `Name`, `Slug`, `OrgType` (`Vendor`/`Tmc`/`Client`/`Platform` per `Domain.Enums.System.OrgType`)
- `ParentOrgId` (nullable; root orgs have no parent)
- `IsActive`, locale (`LanguageCode`, `TimeZone`, `CurrencyCode`)
- TMC chain/branch fields: `ChainCode`, `ChainName`, `BranchCode` (per `notes.txt` requirement to support Flight Centre-style group codes)
- Support fields: `SupportTeamName`, `SupportTicketEmailTemplateCode`

Hierarchy and tenancy enforcement:

- The intended access model from `v5-plan.md §5` says access never flows upward or sideways; the role's `ScopeMode` (`Self` | `SelfAndDescendants`) determines descendant traversal.
- The actual implementation: there is **no** `HasQueryFilter` for tenancy on any entity, and **no** centralised query-scoped access evaluator. Repositories accept `OrgId` as a parameter.
- `IUserRepository`, `IBookingRepository`, etc. depend on the caller passing the correct `orgId`; cross-tenant data leakage is not prevented at the DbContext layer.

Evidence:

- `src/Cinturon360.Domain/Entities/Organization/Organisation.cs:1–152`
- `src/Cinturon360.Domain/Enums/Security/ScopeMode.cs`
- `src/Cinturon360.Domain/Enums/System/OrgType.cs` (not read in detail; inferred from references)
- absence of `HasQueryFilter` in `src/Cinturon360.Data/Configurations/`

## User, Identity, Role, Claim, and Permission Model

User identity tables in domain:

- `User` (per home org, never null for tenant users)
- `UserSecurity` (password hash, lockout, MFA flags, password reset token)
- `UserAuthMethod` (per method: Password, Google, Microsoft, OIDC, SAML, etc.)
- `UserMfaMethod` (TOTP/SMS/Email/Push)
- `UserRecoveryCode`
- `UserSession` (`TokenClass`: InteractiveWebSession / Mobile / API / etc.)
- `UserApiToken` (PAT / service-account)
- `UserRoleAssignment` (`UserId`, `OrgId`, `RoleId`, `ScopeMode`, `IsActive`, `ExpiresAt`)
- `UserAuditEvent`
- `UserProvisioningSource`
- `UserAccessOverride`
- traveller satellites: `TravellerProfile`, `TravellerLoyaltyProgram`, `UserAddress`, `UserPreferences`, `UserEmergencyContact`

Role / permission model:

- `Role` (org-typed, `IsSystemRole`, optional `OrgId` for custom roles)
- `RolePermission` (string `PermissionCode`)
- `Common/Constants/PermissionCodes.cs` defines ≈30 codes (`bookings.read`, `policy.manage`, `users.invite`, `platform.admin`, etc.)
- `Common/Constants/AppConstants.cs` declares parallel claim taxonomy (`c360:user_id`, `c360:org_id`, `c360:org_role`, `c360:app_role`, `c360:tenant_id`, `c360:service_account`) and `Roles` constants (`global_admin`, `support`, `finance`, `org_admin`, `approver`, `booker`, `traveller`, `read_only`).
- `Domain/Enums/Security/Roles.cs` defines `PlatformRole` (`Sudo`, `PlatformOps`, `PlatformAudit`) and `OrgRole` (`OrgAdmin`, `Approver`, `Booker`, `Traveller`, `ReadOnly`).

The role enum and the `Roles` static class are partial duplicates with non-matching naming conventions (`OrgAdmin` vs `org_admin`, `Sudo` vs `global_admin`).

The current `Cinturon360.Web/Security/ClaimTypes.cs` duplicates a third claim taxonomy specific to the Web BFF cookie (`UserId`, `OrgId`, `Email`, `DisplayName`, `UserCategory`, `PlatformRole`, `AccessToken`, `RefreshToken`, `TokenExpiry`, `SessionId`, `Theme`, `Language`, `TimeZone`).

Three coexisting taxonomies (`Common.Constants.ClaimTypes`, `Common.Constants.Roles`, `Domain.Enums.Security.{PlatformRole,OrgRole}`, `Web.Security.ClaimTypes`) are not aligned.

Evidence:

- `src/Cinturon360.Common/Constants/AppConstants.cs:11–46`
- `src/Cinturon360.Common/Constants/PermissionCodes.cs`
- `src/Cinturon360.Domain/Enums/Security/Roles.cs:7–24`
- `src/Cinturon360.Domain/Entities/Identity/UserRoleAssignment.cs`
- `src/Cinturon360.Web/Security/ClaimTypes.cs`

## Authentication Model

API authentication:

- A single scheme: `JwtBearerDefaults.AuthenticationScheme` (HMAC-SHA256 symmetric key).
- `Jwt:SecretKey`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:AccessTokenExpiryMinutes` from configuration.
- 30-second clock skew.
- Tokens are signed by `Cinturon360.Infrastructure.Security.TokenService` with claims `sub` (user id), `jti` (session id), `c360:user_id`, optional `c360:org_id`, and one `perm` claim per permission code. (See `TokenService.GenerateAccessToken`.)
- Login flow (`LoginCommandHandler`):
  1. Look up user by lowercased email.
  2. Verify lockout / suspension state.
  3. Check `UserSecurity.PasswordHash` via `IPasswordHasher`.
  4. Increment failed-login counter; lock after 5 attempts for 15 minutes.
  5. On success, fetch permissions via `IPermissionRepository.GetPermissionsForUserAsync`, generate JWT, persist `UserSession` with `TokenClass.InteractiveWebSession`.
  6. Return `AuthResponse(AccessToken, ExpiresIn=1800, SessionId, UserInfoResponse)`.

Web authentication (BFF):

- Cookie scheme `c360.bff` configured in `WebServiceRegistration.AddWebServices`.
- Cookie is `HttpOnly`, `SameSite=Strict`, `SecurePolicy=SameAsRequest`, sliding 1-day expiry.
- `BffAuthStateProvider` is the `AuthenticationStateProvider`; it pulls the auth ticket via `httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme)`.
- On login (`/auth/login-handler` POST), the Web project posts the form to itself, calls `IdentityApiClient.LoginAsync`, and writes the API JWT into the cookie's `Cinturon360.Web.Security.ClaimTypes.AccessToken` claim.
- `BffTokenHandler` (a `DelegatingHandler`) reads the cookie's access-token claim and adds `Authorization: Bearer ...` to outgoing API calls.
- The browser never sees the JWT. Refresh-token claim is empty (refresh flow not implemented).

Out-of-scope or unimplemented:

- OpenIddict packages are referenced but no `AddOpenIddict()` call exists in `Program.cs`. The `UseOpenIddict()` in `Cinturon360.Data` only registers OpenIddict EF Core tables.
- No SAML auth handler is registered. `ITfoxtec.Identity.Saml2` is referenced but unused.
- No OIDC / Microsoft / Google / Apple / Facebook external login is wired in the API. The `Cinturon360.Integrations/IdentityProviders/{Apple,Facebook,Google,Microsoft,Oidc,Saml,Scim}` folders are empty.
- No MFA challenge endpoint. The login flow does not branch on `IsMfaEnabled`.
- No QR-based device login. No mobile session token class flow.

Evidence:

- `src/Cinturon360.Api/Program.cs:43–62`
- `src/Cinturon360.Application/Features/Auth/Commands/LoginCommandHandler.cs:29–102`
- `src/Cinturon360.Infrastructure/Security/TokenService.cs:16–46`
- `src/Cinturon360.Web/DependencyInjection/WebServiceRegistration.cs:20–38`
- `src/Cinturon360.Web/Endpoints/AuthEndpoints.cs:15–183`
- `src/Cinturon360.Web/Security/BffAuthStateProvider.cs`
- `src/Cinturon360.Web/Security/BffTokenHandler.cs`

## Authorization Model

Three concurrent authorization mechanisms exist, none is fully wired:

1. ASP.NET Core Authorization on endpoints: every endpoint uses `RequireAuthorization()` with no policy, role, or permission argument. There are no `AddAuthorization(opts => opts.AddPolicy(...))` calls. There are no `[Authorize(Roles=...)]` or `[Authorize(Policy=...)]` attributes anywhere.
2. MediatR pipeline behaviour: `AuthorizationBehavior<TRequest,TResponse>` checks `currentUser.HasPermission(...)` for any request that implements `IRequirePermission`. Implementation is functional. **Zero requests in the codebase implement `IRequirePermission`** (verified by `Grep` over `src/`).
3. Custom permission-code claim: the JWT carries `perm` claims, which `CurrentUser.HasPermission(...)` reads, but they are never consulted by any production code path.

The intended `Cinturon360.Application/Security/{AppClaims, AppPolicies, AppRoles, OrgRoles, OrgRoleRequirements, PermissionEvaluation, TokenIssuance}` skeleton is empty (only `.gitkeep`).

`PolicyNames` constants exist (`RequireAuthenticated`, `RequireSudo`, `RequireVendor`, `RequireTmc`, `RequireClient`, `RequireGlobalAdmin`) but no `AuthorizationOptions` registers them.

Org-scope traversal (`Self` vs `SelfAndDescendants`) is captured on `UserRoleAssignment` but no service evaluates it. There is no resolver service that, given a user and a target `OrgId`, says "yes" or "no".

Evidence:

- `src/Cinturon360.Application/Behaviors/Authorization/AuthorizationBehavior.cs`
- `src/Cinturon360.Application/Behaviors/Authorization/IRequirePermission.cs`
- `Grep IRequirePermission src/` returned only the two definition files
- `src/Cinturon360.Application/Security/*/`.gitkeep
- `src/Cinturon360.Common/Constants/AppConstants.cs:21–30`
- `src/Cinturon360.Api/Endpoints/Auth/AuthEndpoints.cs:25–60`

## API Surface

The API is a Minimal API host (no controllers). All endpoints are mapped from `Program.cs`:

| Endpoint group              | Mapping function                      | Source file                                                          |
| --------------------------- | ------------------------------------- | -------------------------------------------------------------------- |
| Auth                        | `MapAuthEndpoints`                    | `Endpoints/Auth/AuthEndpoints.cs`                                    |
| Users                       | `MapUserEndpoints`                    | `Endpoints/UserEndpoints.cs`                                         |
| Organisations               | `MapOrganisationEndpoints`            | `Endpoints/Admin/OrganisationEndpoints.cs`                           |
| Travellers                  | `MapTravellerEndpoints`               | `Endpoints/Travellers/TravellerEndpoints.cs`                         |
| Bookings                    | `MapBookingEndpoints`                 | `Endpoints/Bookings/BookingEndpoints.cs`                             |
| Policies                    | `MapPolicyEndpoints`                  | `Endpoints/Policies/PolicyEndpoints.cs`                              |
| Approvals                   | `MapApprovalEndpoints`                | `Endpoints/Approvals/ApprovalEndpoints.cs`                           |
| Billing                     | `MapBillingEndpoints`                 | `Endpoints/Billing/BillingEndpoints.cs`                              |
| Jobs (admin)                | `MapJobEndpoints`                     | `Endpoints/System/JobEndpoints.cs`                                   |
| Exchange rates              | `MapExchangeRateEndpoints`            | `Endpoints/System/ExchangeRateEndpoints.cs`                          |
| Duffel configuration        | `MapDuffelConfigurationEndpoints`     | `Endpoints/System/DuffelConfigurationEndpoints.cs`                   |
| Ticket email templates      | `MapTicketEmailTemplateEndpoints`     | `Endpoints/System/TicketEmailTemplateEndpoints.cs`                   |
| Tickets                     | `MapTicketEndpoints`                  | `Endpoints/Ticketing/TicketEndpoints.cs`                             |
| Duffel flights              | `MapDuffelFlightEndpoints`            | `Endpoints/Flights/DuffelFlightEndpoints.cs`                         |

Empty endpoint area folders (per `v5-solution-structure.md §Cinturon360.Api`): `Endpoints/Public/`, `Endpoints/Vendor/`, `Endpoints/Tmc/`, `Endpoints/Client/`, `Endpoints/Reports/`, `Endpoints/Storage/`, `Endpoints/Feeds/`, `Endpoints/Travel/`, `Endpoints/Booking/` (singular), `Endpoints/Policy/` (singular).

API conventions:

- All endpoints group under `/api/v1/...`.
- Wrapper response type: `Cinturon360.Contracts.Common.Results.ApiResponse<T>` with `Ok(value)` / `Fail(error)` factory methods.
- Errors are `ApiError(string Code, string Description)`.
- Validation errors are mapped to RFC 7807 `ValidationProblemDetails` by `Cinturon360.Api.Infrastructure.GlobalExceptionHandler`.
- `UnauthorizedAccessException` → 403 with ProblemDetails. Other unhandled exceptions → 500 with ProblemDetails.
- `Microsoft.AspNetCore.OpenApi` is registered with `AddOpenApi()` and `MapOpenApi()` (in dev only). No Swagger UI is registered.
- Route conventions are mostly `POST /api/v1/{area}` for actions, `GET /api/v1/{area}` for list, `GET /api/v1/{area}/{id}` for detail, `DELETE /api/v1/{area}/{id}` for revoke/cancel. Webhooks live under `/api/v1/webhooks/...`.

Public-vs-internal API boundary: not formalised. There is no `/api/public/v1` prefix, no separate scheme, no public-API rate limit, and no anonymous API documentation surface.

Versioning: directly embedded in path (`/api/v1/...`). No version negotiation, no `Asp.Versioning` package, no version policies.

Evidence:

- `src/Cinturon360.Api/Program.cs:84–99`
- `src/Cinturon360.Api/Endpoints/`
- `src/Cinturon360.Api/Infrastructure/GlobalExceptionHandler.cs`

## Web/UI Surface

Blazor Server hosting:

- `AddRazorComponents().AddInteractiveServerComponents()` (no WASM, no Auto).
- `Microsoft.FluentUI.AspNetCore.Components` registered with `AddFluentUIComponents()`.
- Tailwind CSS built via MSBuild `BeforeTargets="Build"` step that runs `npm run build:css` from `src/Cinturon360.Web/`.
- `package.json`, `tailwind.config.js`, `postcss.config.js`, and `node_modules/` live inside the Web project. `Microsoft.AspNetCore.DataProtection.EntityFrameworkCore` is referenced but Data Protection is configured to write keys to the file system mount `/app/dp-keys` (not EF).

Pages tree (`src/Cinturon360.Web/Pages/`):

| Folder        | Pages                                                                                              |
| ------------- | -------------------------------------------------------------------------------------------------- |
| `Auth/`       | `Login`, `Register`, `ForgotPassword`, `ResetPassword`, `Logout`, `MfaChallenge`                   |
| `Settings/`   | `Profile`, `Preferences`, `PersonalAccessTokens`, `Sessions`, `Security`                           |
| `Sudo/`       | `Dashboard`, `Audit/AuditLog`, `Documents/DocumentList`, `Jobs/JobQueue`, `Users/UserList`         |
| `Vendor/`     | `Dashboard`, `Billing/BillingOverview`, `Organisations/{OrgList,OrgDetail}`, `TicketEmailTemplates`|
| `Tmc/`        | `Dashboard`, `Bookings/BookingQueue`, `Clients/{ClientList,ClientDetail}`, `Policy/{PolicyList,PolicyEditor}` |
| `Client/`     | `Dashboard`, `TravellerProfile`, `Approvals/{ApprovalsList,ApprovalDetail,ApprovalHistory}`, `Bookings/{BookingList,BookingDetail}`, `Travel/{FlightSearch,SearchResults}` |
| `Ticketing/`  | `MyTickets`, `RaiseTicket`, `SupportDashboard`, `TicketDetail`                                     |

Components tree (`src/Cinturon360.Web/Components/`): `App.razor`, `Routes.razor`, `_Imports.razor`, `Layout/{MainLayout,EmptyLayout,NavMenu,ReconnectModal}`, `Shared/{Toast, ConfirmDialog, ErrorBoundaryWrapper, LoadingSpinner, Nav*, PageHeader, PolicyBadge, RaiseSupportTicketButton, StatusBadge, ThemeSwitcher, TicketStatusBadge, ...}`.

API call pattern: nine typed `HttpClient` clients (`IdentityApiClient`, `TravellerApiClient`, `BookingApiClient`, `PolicyApiClient`, `ApprovalApiClient`, `BillingApiClient`, `OrgApiClient`, `SystemApiClient`, `FlightApiClient`, `TicketApiClient`) all attach `BffTokenHandler` and `AddStandardResilienceHandler`.

Localisation: `RequestLocalizationOptions` defaults to `en-AU`, supports `en-AU`, `en-NZ`, `en-US`, `en-GB`. Resources path `Resources/`.

`MfaChallenge.razor` is present as a placeholder — the API does not yet expose an MFA challenge endpoint, so this page is non-functional.

Evidence:

- `src/Cinturon360.Web/Program.cs:11–66`
- `src/Cinturon360.Web/DependencyInjection/WebServiceRegistration.cs:48–106`
- `src/Cinturon360.Web/Pages/`
- `src/Cinturon360.Web/Components/`
- `src/Cinturon360.Web/Cinturon360.Web.csproj:23–26`

## Background Jobs, Workers, and Scheduled Processing

`Cinturon360.Jobs` is a `Microsoft.NET.Sdk.Worker` host. Its `Program.cs` registers the application, data, infrastructure, integrations, and jobs DI extensions and runs `host.Run()`.

Registered hosted services (per `Cinturon360.Jobs.DependencyInjection.ServiceCollectionExtensions.AddJobServices`):

- `ExchangeRateSyncJob` — fetches ECB exchange rates every 5 minutes.
- `ExchangeRateCleanupJob` — daily UTC cleanup of snapshots older than 24 hours.

Empty / not-implemented job folders:

- `src/Cinturon360.Jobs/HostedServices/`
- `src/Cinturon360.Jobs/QueueProcessors/`
- `src/Cinturon360.Jobs/Recurring/{BillingReconciliation, DataFeeds, PdfCleanup, ReferenceDataRefresh, StorageCleanup, TicketSync}/`
- `src/Cinturon360.Infrastructure/Jobs/{Cron,Queue,Recurring,Scheduling,Workers}/`

The `v5-plan.md §9` says "A dedicated Cinturon360.Jobs container runs with a custom Supercronic image" with sub-second cron support. The Dockerfile installs `supercronic` but never invokes it; the actual scheduling mechanism is `IHostedService.ExecuteAsync` loops in-process.

Evidence:

- `src/Cinturon360.Jobs/Program.cs`
- `src/Cinturon360.Jobs/DependencyInjection/ServiceCollectionExtensions.cs:8–15`
- `src/Cinturon360.Jobs/Recurring/DataFeeds/` (the only populated subfolder)
- `deploy/docker/Dockerfile.jobs:25–43`
- `v5-plan.md` §9

## Integrations

Implemented:

- `Cinturon360.Integrations.ExchangeRates.EcbExchangeRateProvider` — ECB SDMX endpoint, registered HttpClient `EcbRates` with 30 s timeout.
- `Cinturon360.Integrations.Flights.Duffel.DuffelApiClient` — search offers, create order, cancel order. Used by Application command handlers (`DuffelSearchOffersCommand`, `DuffelCreateOrderCommand`, `DuffelCancelOrderCommand`). Org-scoped credentials resolved via `IDuffelConfigResolver` (`Application/Features/Integrations/Duffel/Services/DuffelConfigResolver.cs`).
- `Cinturon360.Integrations.GitHub.Services.GitHubIssuesClient` — REST API at `https://api.github.com/repos/{owner}/{repo}/`. Has dry-run mode when GitHub PAT is not configured.

Stripe integration sits in `Cinturon360.Infrastructure.Payments`, **not** in `Cinturon360.Integrations`:

- `StripePaymentGateway` — legacy gateway.
- `StripePaymentProviderGateway` — provider-neutral gateway used by the new `BillingRelationship`-based flows.

Empty / unimplemented:

- `Cinturon360.Integrations/Aws/{Models,S3}/` (S3 work happens in `Cinturon360.Infrastructure/Storage/`).
- `Cinturon360.Integrations/Cars/`, `Hotels/`, `Rail/` — no provider chosen.
- `Cinturon360.Integrations/Geography/` — no IATA/OAG ingestion implemented.
- `Cinturon360.Integrations/Payments/` — empty (Stripe is in Infrastructure).
- `Cinturon360.Integrations/IdentityProviders/{Apple, Facebook, Google, Microsoft, Oidc, Saml, Scim}/` — empty scaffolds.
- `Cinturon360.Integrations/Common/{Auth, Http, Logging, Mapping, Resilience}/` — shared HTTP/resilience layer not extracted.
- `Cinturon360.Integrations/GitHub/{Mapping,Ticketing}/` — only `Models/` and `Services/` are populated.

Polly retry/circuit-breaker policies are not registered for any HttpClient. Standard resilience is added on the Web project's typed clients (`AddStandardResilienceHandler`), not on the Integrations clients.

Evidence:

- `src/Cinturon360.Integrations/DependencyInjection/ServiceCollectionExtensions.cs`
- `src/Cinturon360.Infrastructure/Payments/Stripe*Gateway.cs`
- `src/Cinturon360.Application/Features/Integrations/Duffel/`
- empty Integrations subdirectories (verified via `find`)

## Billing, Licensing, Payments, Accounting, and Entitlements

This area is the most complex and currently has three coexisting models.

### Generation 1 — Legacy billing (Phase 6)

| Entity              | Purpose                                              |
| ------------------- | ---------------------------------------------------- |
| `OrgLicense`        | Org-scoped license, billing cycle, max users         |
| `OrgBillingConfig`  | Stripe customer ID, billing email                    |
| `Invoice`           | Org-scoped invoice                                   |
| `Payment`           | Org-scoped payment record (Stripe payment intent)    |
| `PrepaidBalance`    | Org-scoped prepaid credit balance                    |

### Generation 2 — Provider-neutral billing (2 May 2026)

| Entity                          | Purpose                                                   |
| ------------------------------- | --------------------------------------------------------- |
| `PaymentProviderConnection`     | Seller-owned Stripe (or future) connection                |
| `BillingRelationship`           | Seller→Buyer commercial relationship                      |
| `ProviderCustomer`              | Buyer org as customer inside seller provider              |
| `ProviderPaymentMethod`         | Saved payment method (`pm_...`)                           |
| `OrganisationBillingProfile`    | Default billing config for an org                         |
| `PolicyBillingRule`             | Per-policy payment-method override                        |
| `ProviderWebhookEvent`          | Idempotent webhook persistence                            |

### Generation 3 — License v2 + accounting stubs (3 May 2026)

| Entity                              | Purpose                                                        |
| ----------------------------------- | -------------------------------------------------------------- |
| `LicenseAgreement`                  | Seller-issued contract with billing model, period, terms, etc. |
| `LicenseAgreementEntitlement`       | Per-feature entitlement (boolean / numeric / unlimited)        |
| `LicenseCollectionPolicy`           | Grace period, block-on-overdue, action after grace             |
| `BillingAccount`                    | Runtime org-billing account (Active/Suspended/Closed)          |
| `BillingLedgerEntry`                | Stub: ledger row                                               |
| `BillingInvoice`                    | Stub: invoice scoped to seller+buyer with `LicenseAgreementId` |
| `BillingInvoiceLine`                | Stub: line item                                                |
| `PaymentAttempt`                    | Stub: per-attempt payment record                               |
| `JournalEntry`                      | Stub: double-entry header                                      |
| `JournalLine`                       | Stub: DR / CR line                                             |

All three generations are exposed as `DbSet<>` properties on `AppDbContext`. The new tables exist in migration `20260503100359_RefactorBillingModelToLicenseAgreement` but no application command issues invoices, posts ledger entries, evaluates `LicenseAgreement` at booking time, enforces `LicenseCollectionPolicy.BlockBookingsWhenOverdue`, or activates a `BillingAccount` when a `LicenseAgreement` is activated.

Implemented commands (per `BACKLOG.md` and `src/Cinturon360.Application/Features/Billing/Commands/`):

- `ConfigureStripeProviderConnectionCommand`
- `CreateBillingRelationshipCommand`
- `GenerateRelationshipBillingSetupLinkCommand`
- `InitiateRelationshipTopUpCommand` / `ConfirmRelationshipTopUpCommand`
- `SetPrimaryProviderConnectionCommand`
- `CreateLicenseAgreementCommand` / `ActivateLicenseAgreementCommand` / `SupersedeLicenseAgreementCommand`

Webhook routes implemented: `POST /api/v1/webhooks/payment-providers/stripe/{connectionId}` and `POST /api/v1/webhooks/stripe/{vendor|tmc|client}/{orgId}` plus a legacy single-endpoint route. Stripe signature verification is implemented in the connection-scoped handler. Webhook idempotency is via the unique `(connectionId, providerEventId)` index on `ProviderWebhookEvent`.

Pending (per `REMAINING-PHASES.md` §10 and `BACKLOG.md`):

- Invoice/payment collection orchestration.
- `LicenseAgreement` evaluation at booking time.
- `BillingAccount` activation workflow.
- `LicenseCollectionPolicy` enforcement.
- Credit notes.
- Stripe billing portal session.
- Stripe invoice sync back to internal records.
- Refund flow.
- Idempotency keys on Stripe payment intent creation.
- Ledger / journal posting from invoice/payment events.

Evidence:

- `src/Cinturon360.Domain/Entities/Billing/BillingEntities.cs:1–1088`
- `src/Cinturon360.Data/Context/AppDbContext.cs:71–93`
- `src/Cinturon360.Data/Migrations/20260501172954_AddProviderNeutralBillingArchitecture.cs`
- `src/Cinturon360.Data/Migrations/20260503100359_RefactorBillingModelToLicenseAgreement.cs`
- `docs/architecture/cinturon360-v5-billing-licensing-entitlements-accounting-reference.md`
- `docs/architecture/cinturon360-billing-stripe-architecture-reference.md`
- `docs/architecture/stripe-payments.md`
- `BACKLOG.md` lines 130–143, 215–229
- `REMAINING-PHASES.md` lines 159–180

## Booking, Quote, Order, Ticket, Traveller, and Policy Workflows

### Booking aggregate

`Booking` (`src/Cinturon360.Domain/Entities/Booking/Booking.cs:11–104`) is a soft-deletable aggregate with `BookingStatus` lifecycle: Draft → PendingApproval → Approved → Confirmed → Ticketed → Completed (with Cancelled side branch). Approval levels are tracked on the booking itself (`ApprovalLevelsRequired`, `ApprovalLevelsCompleted`).

### Quote aggregate

`Quote` (Domain/Entities/Booking/Quote.cs) currently stores the full Duffel offers JSON blob. `BACKLOG.md` notes a `DuffelOfferSnapshot` structured table is deferred.

### Travel policy

`TravelPolicy`, `PolicyRule`, `PolicyAssignment` (Domain/Entities/Policy/PolicyEntities.cs). CRUD endpoints are implemented in `PolicyEndpoints`. **Rule evaluation is not implemented.** Cabin class enforcement, spend caps, advance-booking-window enforcement, approval-threshold derivation, per-user policy override — all listed as 🔴 in `BACKLOG.md`.

### Approvals

`ApprovalRequest`, `ApprovalDecision` (Domain/Entities/Approval/ApprovalEntities.cs). Approval list, detail, approve/reject endpoints exist. Triggering logic is missing: `CreateBookingHandler` does not call `RequireApproval` based on policy evaluation. No approval-expiry job. No notification to approver.

### Travellers

Traveller commands (`UpdateTravellerProfileCommand`, etc.) exist in `Application/Features/Travellers/`. Loyalty programs, addresses, emergency contacts, and preferences are domain entities and have repositories. Web pages exist for traveller profile editing.

### Tickets

`SupportTicket`, `TicketComment`, `TicketEscalation`, `TicketAttachment`, `TicketEmailTemplate` (Domain/Entities/Ticketing/). The `Application/Features/Ticketing/Services/TicketUpdateNotificationService` exists. The `IGitHubTicketingService` is invoked on ticket create/comment/escalate. Webhook receiver, signature verification, and reconciliation job are 🔴 missing per `BACKLOG.md` lines 175–181.

Evidence:

- `src/Cinturon360.Domain/Entities/Booking/Booking.cs`
- `src/Cinturon360.Application/Features/{Bookings, Approvals, Policies, Travellers, Ticketing}/`
- `src/Cinturon360.Domain/Entities/Policy/PolicyEntities.cs`
- `src/Cinturon360.Domain/Entities/Approval/ApprovalEntities.cs`
- `BACKLOG.md` Approvals, Travel Policy Engine, GitHub Ticketing sections
- `REMAINING-PHASES.md` Phase 10 Flights status

## Notifications and Messaging

Email:

- `Cinturon360.Infrastructure.Email.MailerSendEmailService` is functional. Both `SendAsync` (raw HTML) and `SendTemplatedAsync` are implemented.
- Wired callers: password-reset (raw send) and ticket-update notifications.
- Real MailerSend template IDs are not wired. `BACKLOG.md` notes confirmation/approval/invoice/MFA/invite emails are 🔴.

SMS: `Cinturon360.Infrastructure.Sms/` is empty. No Twilio, no MailerSend SMS, no other provider.

Push: Not implemented (deferred with mobile).

Evidence:

- `src/Cinturon360.Infrastructure/Email/MailerSendEmailService.cs`
- `src/Cinturon360.Application/Features/Auth/Commands/RequestPasswordResetCommandHandler.cs` (referenced)
- `src/Cinturon360.Application/Services/Notifications/`
- `BACKLOG.md` Email — MailerSend section

## Logging, Observability, Diagnostics, and Audit

Logging:

- API uses Serilog read-from-configuration with Console sink and a custom output template `[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}`.
- `Serilog.Sinks.PostgreSQL.Alternative` package is in `Directory.Packages.props` but not wired into Program.cs (per Q13 decision: stdout only in prod).
- `Serilog.Enrichers.{Environment, Thread, CorrelationId}` are referenced. CorrelationId enricher is registered via `Enrich.FromLogContext()` only — no middleware sets a correlation id explicitly.
- `Cinturon360.Infrastructure/Logging/{Sinks, Enrichers, Formatters, Correlation}/` are empty.
- `Cinturon360.Application/SysLog/{Models, Writers, Enrichers, Events}/` are empty.

Audit:

- `UserAuditEvent` entity exists. Per `BACKLOG.md` audit log export, impersonation audit, and audit log archival are 🔴.
- No `AuditInterceptor` is implemented. Domain mutations do not emit audit rows automatically.

Diagnostics:

- `app.UseSerilogRequestLogging()` is wired (line 75 of API Program.cs notes the call but the formatting is collapsed onto a comment line — confirm at build time).
- No health-check endpoint (`/health` not registered).
- No metrics (`Prometheus`, OpenTelemetry, etc.).
- No Application Insights or Azure Monitor integration in code.

Evidence:

- `src/Cinturon360.Api/Program.cs:31–38, 75`
- `Directory.Packages.props` lines 24–30
- `src/Cinturon360.Infrastructure/Logging/`
- `src/Cinturon360.Application/SysLog/`
- `BACKLOG.md` Observability & Logging section

## Security and Compliance Posture

Implemented:

- Password hashing (`Cinturon360.Common.Security.CinturonPasswordHasher` and `Cinturon360.Infrastructure.Security.PasswordHasher`).
- JWT bearer signing with HMAC-SHA256.
- PAT generation (random 32-byte base64 + SHA256 hash storage).
- Account lockout after 5 failed login attempts for 15 minutes.
- BFF cookie auth keeps tokens off the browser.
- Data Protection keys persisted on a mounted volume in the Web container.
- Webhook idempotency for Stripe.
- Stripe webhook signature verification on connection-scoped routes.
- Soft-delete on user / org / booking / role / invoice / ticket aggregates.

Not implemented or weakly implemented:

- HMAC verification for the GitHub webhook (BACKLOG.md notes the receiver itself is missing).
- SCIM, SAML, OIDC external auth, MFA challenge, recovery codes.
- Tenant data isolation via global query filter.
- Permission enforcement at API endpoints.
- Audit interceptor and audit-log archival.
- ISO 27001 / SOC 2 / PCI-DSS-aligned controls (no documentation, no evidence).
- Threat model.

Evidence:

- `src/Cinturon360.Common/Security/CinturonPasswordHasher.cs`
- `src/Cinturon360.Infrastructure/Security/{TokenService, PasswordHasher}.cs`
- `src/Cinturon360.Application/Features/Auth/Commands/LoginCommandHandler.cs:24–60`
- `src/Cinturon360.Web/DependencyInjection/WebServiceRegistration.cs:20–38`
- `BACKLOG.md` Auth & Identity, GitHub Ticketing, Permission Engine sections

## Testing Model

All seven test projects exist but contain only the default xUnit stub:

```csharp
public class UnitTest1
{
    [Fact]
    public void Test1()
    {
    }
}
```

This applies to:

- `tests/Cinturon360.Api.Tests/`
- `tests/Cinturon360.Application.Tests/`
- `tests/Cinturon360.Architecture.Tests/`
- `tests/Cinturon360.Data.Tests/`
- `tests/Cinturon360.Domain.Tests/`
- `tests/Cinturon360.Integration.Tests/`
- `tests/Cinturon360.Web.Tests/`

The test packages registered in `Directory.Packages.props` are mature (xunit 2.9.3, Moq 4.20.72, FluentAssertions 8.3.0, Microsoft.EntityFrameworkCore.InMemory 10.0.4, Testcontainers.PostgreSql 4.4.0, TngTech.ArchUnitNET 0.13.3) but no test exercises any of them.

There is no end-to-end or UI test project. There is no Playwright/Selenium harness. There is no `dotnet test` configuration in CI (because there is no CI).

Evidence:

- `tests/Cinturon360.Architecture.Tests/UnitTest1.cs:1–10`
- `tests/Cinturon360.Domain.Tests/UnitTest1.cs:1–10`
- `Directory.Packages.props` testing section

## Existing Documentation Inventory

Top-level Markdown files (line counts):

| File                                                          | Lines | Type / role                                             |
| ------------------------------------------------------------- | ----- | ------------------------------------------------------- |
| `README.md`                                                   | 149   | Setup quick-start, system-account tool docs            |
| `v5-plan.md`                                                  | 451   | Architecture & system plan, hard rules, phase order    |
| `v5-solution-structure.md`                                    | 504   | Project layout, layer responsibilities, NuGet table    |
| `BACKLOG.md`                                                  | 315   | Deferred / partial / missing items by area             |
| `REMAINING-PHASES.md`                                         | 266   | Status of Phases 8–13                                  |
| `QUESTIONS.md`                                                | 2 248 | 25 Q&A items (Q1–Q16, Q20–Q28; Q17–Q19 missing)        |
| `SESSION-NOTES-2026-04-27.md`                                 | 68    | Postgres 18 migration / volume mount notes             |
| `notes.txt`                                                   | 71    | Informal: ECB choice, TMC chain code, GitHub ticketing |
| `github-ticketing-setup.md`                                   | 308   | Operational guide for GitHub-Issues backed ticketing   |
| `ecb-exchange-rate-api-ingestion.md`                          | 408   | Rationale and reference for ECB FX feed                |
| `claude-code-v5-to-v5-1-migration-discovery-prompt.md`        | 700+  | The prompt that generated this report                  |

`docs/` tree:

| File                                                                                       | Type             | Notes                                              |
| ------------------------------------------------------------------------------------------ | ---------------- | -------------------------------------------------- |
| `docs/api/support-ticket-email-templates.md`                                               | API ref          | How ticket emails resolve and render               |
| `docs/architecture/stripe-payments.md`                                                     | Architecture     | Provider-neutral Stripe summary                    |
| `docs/architecture/cinturon360-billing-stripe-architecture-reference.md`                   | Architecture     | Provider-neutral billing reference                 |
| `docs/architecture/cinturon360-v5-billing-licensing-entitlements-accounting-reference.md`  | Architecture     | License v2 / accounting reference                  |
| `docs/runbooks/runbook-stripe-payments.md`                                                 | Runbook          | Stripe ops                                         |
| `docs/policies/`                                                                           | (empty)          | No documents                                       |

Other docs:

- `tools/db/system-account/README.md` — operational doc for the platform admin CLI.
- `.github/agents/cinturon360-v5-planner.agent.md` — Claude agent prompt encoding the v5 design rules.

## Existing Markdown Documentation Review

| File                                                                                       | Current       | Conflicts                                                  | v5.1 action                                                              | Suggested target                                                          |
| ------------------------------------------------------------------------------------------ | ------------- | ---------------------------------------------------------- | ------------------------------------------------------------------------ | ------------------------------------------------------------------------- |
| `README.md`                                                                                | Mostly current | Mentions `Cinturon360.sln`, "PostgreSQL 17"; actual is `Cinturon360.slnx` and Postgres 18 in compose | Rewrite to point at v5.1 layout; trim system-account section into a runbook | `docs/00-start-here/index.md` + `docs/10-runbooks/local-dev/system-account.md` |
| `v5-plan.md`                                                                               | Authoritative | Says no migrator (still true); says supercronic (not implemented); says `postgres:16` | Split: domain → architecture overview; phase order → delivery roadmap; hard rules → standards | `docs/02-architecture/platform/platform-overview.md` + `docs/13-delivery/phases/` + `docs/09-standards/` |
| `v5-solution-structure.md`                                                                 | Mostly current | Several reference-graph deviations (see Solution and Project Layout) | Update reference rules; carry the table verbatim into architecture       | `docs/02-architecture/platform/solution-structure.md`                      |
| `BACKLOG.md`                                                                               | Living         | Some entries stale (e.g. "AuthorizationBehavior is no-op"; behavior is implemented but unused) | Convert each bullet into a delivery item; reset 🔴/🟡/🔵 against current code | `docs/13-delivery/backlog/backlog-2026-05.md` (snapshot) + GitHub Issues  |
| `REMAINING-PHASES.md`                                                                      | Living         | No major conflicts                                          | Convert to phase pages                                                  | `docs/13-delivery/phases/`                                                |
| `QUESTIONS.md`                                                                             | Living         | Some answers superseded (e.g. Q3 SAML lib upgrade, Q4 AWSSDK pin) | Promote each answered Q to an ADR; archive raw text                     | `docs/03-decisions/accepted/ADR-XXXX-*` + `docs/99-archive/QUESTIONS.md` |
| `SESSION-NOTES-2026-04-27.md`                                                              | Snapshot       | No conflicts                                                | Convert migration policy + commands into a runbook                       | `docs/10-runbooks/database/migrations.md`                                 |
| `notes.txt`                                                                                | Informal       | Loose; mixes ECB, TMC chain codes, GitHub ticketing rules   | Split: ECB → integration doc; TMC chain → domain doc; ticketing → runbook | `docs/05-integrations/ecb/`, `docs/04-domain/orgs/tmc-chain-codes.md`, `docs/10-runbooks/support/github-ticketing.md` |
| `github-ticketing-setup.md`                                                                | Mostly current | Instructions assume manual label seeding                    | Move into operations / runbook + link from architecture                  | `docs/10-runbooks/support/github-ticketing-setup.md`                      |
| `ecb-exchange-rate-api-ingestion.md`                                                       | Reference      | No major conflicts                                          | Move into integrations docs                                              | `docs/05-integrations/ecb/ecb-overview.md`                                |
| `claude-code-v5-to-v5-1-migration-discovery-prompt.md`                                     | Meta           | Self-referential                                            | Archive after v5.1 cut                                                   | `docs/99-archive/`                                                        |
| `docs/architecture/stripe-payments.md`                                                     | Current        | Says "Legacy entities remain for compatibility while migration completes" — true | Keep, refactor into shorter overview + sub-pages                        | `docs/02-architecture/payments/stripe-overview.md` + ADR cross-link       |
| `docs/architecture/cinturon360-billing-stripe-architecture-reference.md`                   | Authoritative  | Some duplication with the License v2 reference              | Promote to canonical billing architecture document                       | `docs/02-architecture/billing/billing-licensing-overview.md`              |
| `docs/architecture/cinturon360-v5-billing-licensing-entitlements-accounting-reference.md`  | Authoritative  | Some duplication with the Stripe billing reference          | Merge with the Stripe billing reference; keep entitlement detail         | `docs/02-architecture/billing/license-v2-reference.md`                    |
| `docs/runbooks/runbook-stripe-payments.md`                                                 | Current        | None major                                                  | Keep, expand operations playbook                                         | `docs/10-runbooks/billing/stripe-operations.md`                           |
| `docs/api/support-ticket-email-templates.md`                                               | Current        | None major                                                  | Move into domain docs                                                    | `docs/04-domain/support/ticket-email-templates.md`                        |
| `tools/db/system-account/README.md`                                                        | Current        | None major                                                  | Move into runbooks; keep `README.md` shim                                | `docs/10-runbooks/local-dev/system-account.md`                            |
| `.github/agents/cinturon360-v5-planner.agent.md`                                           | Current        | Encodes v5 rules                                            | Promote to AI-agent ops doc; rebase rules onto v5.1                      | `docs/14-ai-agents/claude-code/v51-planner.agent.md`                      |

## Existing ADR and Decision Review

There is no `docs/03-decisions/` folder yet. There is no ADR file. Decisions live in `QUESTIONS.md` (25 answered Qs, of which Q17–Q19 are skipped numerically, suggesting rolled-into-other items). Decisions also live in commit messages and architecture references.

Promote each `QUESTIONS.md` Q to an ADR. The first concrete ADR backlog appears under "Recommended ADR Backlog".

## Implementation vs Documentation Gaps

### Conflict: Solution file naming

Current documentation says:

- `v5-solution-structure.md` shows `Cinturon360.sln`
- `README.md` line 14 says "Cinturon360.slnx"

Current implementation suggests:

- `Cinturon360.slnx` is the actual solution file (XML format)

Impact:

- Tooling that follows the older docs will fail.

Recommended v5.1 action:

- Rewrite the solution-structure doc to reflect `.slnx` as the canonical solution file format choice.

### Conflict: Postgres version

Current documentation says:

- `v5-plan.md §13` table: `postgres:16`
- `README.md` line 21: "Database: PostgreSQL 17"

Current implementation suggests:

- `deploy/compose/compose.dev.yaml` uses `postgres:18-alpine3.22`
- `SESSION-NOTES-2026-04-27.md` describes the move to Postgres 18

Impact:

- A new contributor reading the README first will provision Postgres 17, then hit the Postgres 18 volume-path mismatch on first run.

Recommended v5.1 action:

- Settle on a single supported Postgres major version per environment (dev/staging/prod) via ADR. Update README, plan, and compose simultaneously.

### Conflict: Supercronic vs IHostedService

Current documentation says:

- `v5-plan.md §9`: "A dedicated Cinturon360.Jobs container runs with a custom Supercronic image" with sub-second cron support.

Current implementation suggests:

- `Dockerfile.jobs` installs supercronic but `ENTRYPOINT` is `dotnet Cinturon360.Jobs.dll`.
- Registered hosted services run as in-process loops (`ExchangeRateSyncJob`, `ExchangeRateCleanupJob`).

Impact:

- The v5 plan implies sub-second scheduling and a database-driven `Jobs` table. Neither exists today.

Recommended v5.1 action:

- Decide explicitly: (a) keep `IHostedService` and remove supercronic from the Dockerfile + revise the plan, or (b) restore supercronic and migrate jobs to crontab entries with a tiny CLI invocation. Author an ADR.

### Conflict: AuthorizationBehavior status

Current documentation says:

- `BACKLOG.md` line 50–52: `AuthorizationBehavior.cs` is a no-op placeholder.

Current implementation suggests:

- `AuthorizationBehavior<TRequest, TResponse>` actually performs `currentUser.HasPermission(...)` checks; it is wired through MediatR.
- However, no command or query implements `IRequirePermission`, and `Cinturon360.Application/Security/PermissionEvaluation/` is empty.

Impact:

- The status note is stale. The real gap is permission tagging on commands/queries, not the behaviour itself.

Recommended v5.1 action:

- Update the status note. Add `IRequirePermission` to every authoritative command/query as part of the permission-engine effort.

### Conflict: JWT permissions claim status

Current documentation says:

- `BACKLOG.md` line 51: "JWT `permissions` claim is not populated from `RolePermissions` — only role name is in the token".

Current implementation suggests:

- `LoginCommandHandler` calls `permissionRepo.GetPermissionsForUserAsync(user.Id)` and passes the resulting list into `tokenService.GenerateAccessToken(...)`, which writes one `perm` claim per code.

Impact:

- The note is out of date.

Recommended v5.1 action:

- Update the BACKLOG. Confirm the claim format (`perm` vs `permissions`). Decide whether to ship a single space-separated claim or per-code claims (current behaviour).

### Conflict: Reference-graph rules

Current documentation says:

- `v5-solution-structure.md` lines 477–488 declare strict layer boundaries.

Current implementation suggests:

- `Cinturon360.Web` references `Cinturon360.Domain`.
- `Cinturon360.Contracts` references `Cinturon360.Domain`.
- `Cinturon360.Application` references `Cinturon360.Integrations`.

Impact:

- Domain entities can leak into Contracts (and onward to Web). Provider models can leak into Application.

Recommended v5.1 action:

- Either tighten the reference graph and add ArchUnitNET tests to enforce it, or relax the documented rule with a justified ADR.

### Conflict: Documentation locations

Current documentation says:

- `claude-code-v5-to-v5-1-migration-discovery-prompt.md` (and the planner agent) call out an Obsidian-style `docs/` structure with 99-archive and 14-ai-agents.

Current implementation suggests:

- `docs/` only has `api/`, `architecture/`, `policies/` (empty), `runbooks/`. Most documentation lives at the repository root.

Impact:

- Docs are fragmented; some have to be discovered by scrolling through commit history.

Recommended v5.1 action:

- Migrate root-level Markdown into `docs/` per the proposed structure and remove root duplicates.

## Current Technical Debt

| Item                                                                                  | Evidence                                                                                                        | Category    |
| ------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------- | ----------- |
| `Class1.cs` stubs in `Application`, `Common`, `Contracts`, `Data`, `Infrastructure`, `Integrations` | `find ./src -name "Class1.cs"`                                                                                  | Development |
| Empty migration `20260426142026_YourMigrationName` shipped to `main`                  | `src/Cinturon360.Data/Migrations/20260426142026_YourMigrationName.cs:11–22`                                     | Development |
| Duplicate SQL files for migration slot 003                                            | `tools/db/migrations/003_20260426142026_YourMigrationName.sql` and `003_20260427002017_YourMigrationName.sql`   | Development |
| `Cinturon360.Application/Security/*/.gitkeep` — entire intended security skeleton empty | `src/Cinturon360.Application/Security/{AppClaims, AppPolicies, AppRoles, OrgRoles, OrgRoleRequirements, PermissionEvaluation, TokenIssuance}/.gitkeep` | Engineering |
| Three coexisting billing models in the same DbContext                                 | `src/Cinturon360.Data/Context/AppDbContext.cs:71–93`, `src/Cinturon360.Domain/Entities/Billing/BillingEntities.cs` | Engineering |
| Three claim taxonomies (`Common.Constants.ClaimTypes`, `Common.Constants.Roles`, `Web.Security.ClaimTypes`) | `src/Cinturon360.Common/Constants/AppConstants.cs`, `src/Cinturon360.Web/Security/ClaimTypes.cs`                 | Engineering |
| `BillingEntities.cs` is 1 088 lines with 22 entity classes                            | `src/Cinturon360.Domain/Entities/Billing/BillingEntities.cs`                                                    | Development |
| Supercronic installed but unused in jobs container                                    | `deploy/docker/Dockerfile.jobs:25–43`                                                                           | Engineering |
| All test projects empty                                                               | `tests/*/UnitTest1.cs`                                                                                          | Engineering |
| No CI/CD                                                                              | `.github/workflows/` does not exist                                                                             | Engineering |
| `OpenIddict.AspNetCore` referenced but never wired                                    | `src/Cinturon360.Api/Cinturon360.Api.csproj:16–17` + `Program.cs` does not call `AddOpenIddict()`               | Engineering |
| `ITfoxtec.Identity.Saml2` referenced but never wired                                  | `Directory.Packages.props:37`                                                                                   | Engineering |
| No tenant query filter; tenancy isolation depends on caller passing `OrgId`           | absence of `HasQueryFilter` in `src/Cinturon360.Data/Configurations/`                                           | Engineering |
| No correlation-ID middleware                                                          | `src/Cinturon360.Api/Program.cs` lacks `app.UseCorrelationId(...)`                                              | Engineering |
| No `health` endpoint                                                                  | `src/Cinturon360.Api/Program.cs`                                                                                | Engineering |

## Current Product/Engineering/Development Gaps

| Gap                                                                          | Category    | Evidence                                                                                                | Recommended v5.1 Action                                       |
| ---------------------------------------------------------------------------- | ----------- | ------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------- |
| Permission engine is not enforced at any layer                                | Engineering | `AuthorizationBehavior` + `IRequirePermission` exist; no command or endpoint uses them                  | Add ADR + delivery item; tag commands with permission codes   |
| Tenant isolation not enforced at the persistence layer                       | Engineering | No `HasQueryFilter`                                                                                     | Add `ITenantContext` + global query filter; ArchUnit test     |
| MFA challenge endpoint missing                                                | Engineering | `BACKLOG.md` Auth & Identity                                                                            | Spec MFA flow; build challenge endpoint + recovery codes      |
| External IdP (OIDC/SAML/Microsoft/Google) not implemented                    | Product     | `Cinturon360.Integrations/IdentityProviders/*` empty                                                    | Decide priority; build OIDC first per Q6                      |
| Booking does not invoke approval policy or billing checks                    | Product     | `BACKLOG.md` Approvals — first bullet                                                                  | Build policy evaluator + billing check service                |
| Travel policy rule evaluator missing                                         | Product     | `BACKLOG.md` Travel Policy Engine                                                                       | Build rule engine; per-rule unit tests                        |
| Geography reference data not seeded or refreshed                             | Product     | `BACKLOG.md` Geography                                                                                  | Decide IATA/OAG; ingestion job + seed migration               |
| Hotels, cars, rail providers not chosen                                      | Product     | `BACKLOG.md` Hotels / Cars / Rail / `REMAINING-PHASES.md` notes                                        | Provider decision ADR per category                            |
| TMC chain/branch concept partially modelled (string fields on Organisation)  | Product     | `Organisation.ChainCode/ChainName/BranchCode`; `notes.txt`                                              | Move to a `OrgChain` aggregate with a join table              |
| Vendor "search function enable per category" not modelled                    | Product     | `notes.txt` (hotel chain, tour operator, etc.)                                                          | Spec + entity + admin endpoints                               |
| No CI/CD                                                                     | Engineering | `.github/workflows` missing                                                                             | Add GitHub Actions: build, test, container publish, ACA deploy |
| No production compose                                                        | Engineering | `deploy/compose/compose.dev.yaml` only                                                                  | Author `compose.staging.yaml`, `compose.prod.yaml`, or skip in favour of ACA Bicep |
| No Caddy/TLS for staging                                                     | Engineering | `deploy/caddy/` empty                                                                                   | Author `Caddyfile` + ADR                                       |
| No Azure Bicep                                                               | Engineering | `deploy/azure/` empty                                                                                   | Author Bicep for ACA + Postgres + Key Vault                    |
| No tests                                                                     | Engineering | `tests/*/UnitTest1.cs`                                                                                  | Define test pyramid; seed first ArchUnit + first Domain unit tests |
| Audit interceptor missing                                                    | Engineering | `Cinturon360.Data/Interceptors/.gitkeep`                                                                | Author EF interceptor that emits `UserAuditEvent` rows         |
| Health endpoint missing                                                      | Engineering | API `Program.cs`                                                                                        | Add `MapHealthChecks` + DB liveness                            |
| Correlation ID missing                                                       | Engineering | API `Program.cs`                                                                                        | Add middleware to set `X-Correlation-Id` and enrich logs       |
| Documentation fragmented                                                     | Development | Many root-level Markdown files                                                                          | Migrate to `docs/` Obsidian structure                          |
| ECB sample data file in repo root                                            | Development | `ecb-data.json`                                                                                         | Move to `docs/05-integrations/ecb/samples/` or delete          |
| `legacy-reference/` still on `main`                                          | Development | `legacy-reference/`                                                                                     | Phase 13 — move to `legacy/v4-reference` branch and delete     |

## v5.1 Migration Risks

| Risk                                                                                     | Severity | Mitigation                                                                              |
| ---------------------------------------------------------------------------------------- | -------- | --------------------------------------------------------------------------------------- |
| Schema is partly migrated; Phase 6 legacy + Phase 10 provider-neutral + Phase 10b License v2 are concurrent | High     | Decide cutover strategy: drop legacy + back-fill, or write conversion job; authoritative ADR |
| No tests means refactors cannot be validated                                              | High     | Block major refactors until baseline tests exist; start with ArchUnit + Domain          |
| `JwtBearer` symmetric key in dev/prod; no Key Vault wiring at runtime                     | Medium   | Move JWT secret into `ISecretStore` lookup; rotate on every deploy                      |
| Multiple claim taxonomies confuse permission enforcement                                  | Medium   | Centralise `ClaimTypes` once; remove duplicates                                          |
| Postgres version drift across compose / README / plan                                     | Medium   | Single ADR pinning version + matching CI matrix                                          |
| Reference-graph leaks (Web→Domain, Contracts→Domain, App→Integrations)                    | Medium   | Tighten csproj or ADR + ArchUnit                                                         |
| Empty migration `YourMigrationName` is in production-bound migration history              | Medium   | Squash, rebuild, or document the no-op explicitly                                        |
| BFF cookie carries the raw JWT in claims                                                  | Medium   | Move access token to ASP.NET token store or a server-side cache keyed by session ID     |
| Stripe and webhook security depend on per-connection secret rotation                      | Medium   | Build rotation runbook + scheduled job alarms                                            |
| Supercronic installed but unused in image; future contributors will re-introduce         | Low      | Remove from Dockerfile or wire it; either way commit an ADR                              |

## v5.1 Refactoring Candidates

### Refactoring Candidate: Billing schema consolidation

Current state:

- 22 billing-related entity classes split across legacy, provider-neutral, and License v2 generations.
- All three exposed as DbSets on a single DbContext.

Evidence:

- `src/Cinturon360.Domain/Entities/Billing/BillingEntities.cs`
- `src/Cinturon360.Data/Context/AppDbContext.cs:71–93`

Risk:

- Diverging mental models for "what is the billing source of truth?" Onboarding pain. Code paths that read the legacy and v2 models differently.

Recommended v5.1 action:

- Author ADR pinning License v2 + accounting stubs as canonical.
- Convert legacy `OrgLicense`/`OrgBillingConfig`/`Invoice`/`Payment`/`PrepaidBalance` into a one-shot conversion migration into `LicenseAgreement` + `BillingAccount` + `BillingInvoice` rows.
- Remove legacy DbSets after conversion verified.

Suggested documentation target:

- `docs/02-architecture/billing/billing-licensing-overview.md`
- `docs/03-decisions/accepted/ADR-XXXX-license-v2-canonical.md`

### Refactoring Candidate: Permission enforcement

Current state:

- Permission codes defined; JWT carries `perm` claims; `AuthorizationBehavior` is wired.
- No command or endpoint uses `IRequirePermission`. All endpoints use bare `RequireAuthorization()`.

Evidence:

- `src/Cinturon360.Api/Endpoints/Auth/AuthEndpoints.cs:25–60` (and 13 other endpoint files)
- `Grep IRequirePermission src/` returns only definitions

Risk:

- Any authenticated user can hit any authenticated endpoint. Tenant isolation is not enforced.

Recommended v5.1 action:

- Author ADR for permission enforcement strategy (command-level vs endpoint-policy vs both).
- Tag every command/query with `IRequirePermission` and a constant from `PermissionCodes`.
- Add `AddAuthorization(opts => opts.AddPolicy(...))` for the `PolicyNames.*` constants and apply them on endpoints by org-type or sudo requirement.

Suggested documentation target:

- `docs/02-architecture/authorization/permission-engine.md`
- `docs/03-decisions/accepted/ADR-XXXX-permission-enforcement.md`

### Refactoring Candidate: Tenant query filter

Current state:

- No global query filter on tenant-scoped entities.
- Repositories accept `OrgId` parameters and rely on caller correctness.

Evidence:

- `src/Cinturon360.Data/Configurations/`
- absence of `HasQueryFilter` calls

Risk:

- A single missed `Where(x => x.OrgId == ...)` leaks cross-tenant data.

Recommended v5.1 action:

- Introduce `ITenantContext` resolved from `ICurrentUser` or HTTP context.
- Add `HasQueryFilter(e => e.OrgId == _tenantContext.OrgId || _tenantContext.IsSudo)` on tenant-scoped entities.
- Add ArchUnit test that flags any new `OrgId` column without a corresponding query filter.

Suggested documentation target:

- `docs/02-architecture/tenancy/org-hierarchy.md`
- `docs/03-decisions/accepted/ADR-XXXX-tenant-query-filter.md`

### Refactoring Candidate: Web project Domain reference

Current state:

- `Cinturon360.Web.csproj` references `Cinturon360.Domain` directly.

Evidence:

- `src/Cinturon360.Web/Cinturon360.Web.csproj:11–13`

Risk:

- Domain entities (with private setters and aggregate behaviour) leak into Razor components and ViewModels.

Recommended v5.1 action:

- Remove the Domain reference; convert any Domain types currently used by Razor components into Contracts.
- Add ArchUnit test that fails the build if `Cinturon360.Web` references `Cinturon360.Domain`.

Suggested documentation target:

- `docs/03-decisions/accepted/ADR-XXXX-web-only-references-contracts.md`

### Refactoring Candidate: Job scheduling model

Current state:

- Plan says Supercronic with sub-second schedules.
- Implementation uses `IHostedService` background loops.
- Dockerfile installs but never invokes supercronic.

Evidence:

- `deploy/docker/Dockerfile.jobs:25–43`
- `src/Cinturon360.Jobs/DependencyInjection/ServiceCollectionExtensions.cs:8–15`
- `v5-plan.md §9`

Risk:

- Future contributors will re-introduce supercronic and break the existing in-process scheduling.

Recommended v5.1 action:

- ADR: pick supercronic-backed crontab or `IHostedService` only, not both.
- If supercronic: introduce a tiny CLI in `Cinturon360.Jobs` that runs a single named job by id.
- If `IHostedService`: remove supercronic from Dockerfile and update the plan.

Suggested documentation target:

- `docs/02-architecture/workflows/jobs-and-scheduling.md`
- `docs/03-decisions/accepted/ADR-XXXX-job-scheduler.md`

### Refactoring Candidate: Claim type / role taxonomies

Current state:

- `Common.Constants.ClaimTypes`, `Common.Constants.Roles`, and `Web.Security.ClaimTypes` are not aligned.

Evidence:

- `src/Cinturon360.Common/Constants/AppConstants.cs:11–46`
- `src/Cinturon360.Web/Security/ClaimTypes.cs`

Risk:

- Cookie claims (BFF) and JWT claims have different keys; permission enforcement can silently miss.

Recommended v5.1 action:

- Move all claim names into `Cinturon360.Common.Constants.ClaimTypes`.
- Delete `Cinturon360.Web.Security.ClaimTypes`.
- Update `BffTokenHandler`, `BffAuthStateProvider`, and `CurrentUser` to use the same source.

Suggested documentation target:

- `docs/08-security/identity/claim-taxonomy.md`

## v5.1 Documentation Migration Plan

Phase A — preserve.

1. Copy the entire current `cinturon360.dev/` source tree, including `legacy-reference/`, into `cinturon360.dev-v5.1/`.
2. Tag the source tree at v5 final commit.
3. Continue all v5.1 work in the new tree.

Phase B — scaffold.

1. Create the `docs/` Obsidian skeleton (see "Recommended Obsidian Documentation Structure").
2. Move root-level Markdown into `docs/` per the table in "Existing Markdown Documentation Review".
3. Create `docs/03-decisions/accepted/ADR-0001` to `ADR-0004` (database, Blazor Server, BFF auth, markdownlint) per the canonical seed list in the prompt.

Phase C — first-pass authoring.

1. Author the architecture overview (`docs/02-architecture/platform/platform-overview.md`) by extracting the §3 diagram from `v5-plan.md` plus the table from `v5-solution-structure.md`.
2. Author the auth overview (`docs/02-architecture/auth/authentication-overview.md`) by combining sections of this report's Authentication Model with the Login flow.
3. Author the org hierarchy (`docs/02-architecture/tenancy/org-hierarchy.md`) by combining `v5-plan.md §5` with the TMC chain notes from `notes.txt`.
4. Author the billing canonical doc (`docs/02-architecture/billing/billing-licensing-overview.md`) by merging the two billing references.

Phase D — promote decisions.

1. Walk `QUESTIONS.md` Q1–Q28 (with Q17–Q19 omitted) and create one ADR per decision under `docs/03-decisions/accepted/`. Title format `ADR-XXXX-<kebab-title>.md`.
2. Move the original `QUESTIONS.md` into `docs/99-archive/`.

Phase E — runbooks and standards.

1. Author the local-dev runbook (Postgres start, migrations, system-account, hot reload) under `docs/10-runbooks/local-dev/`.
2. Author the .NET 10 / EF Core / Blazor / C# / logging / testing / git standards under `docs/09-standards/`.

Phase F — AI agent operating model.

1. Move `.github/agents/cinturon360-v5-planner.agent.md` into `docs/14-ai-agents/claude-code/v51-planner.agent.md`.
2. Author `docs/14-ai-agents/claude-code/claude-code-operating-model.md` covering: file paths Claude may write to, no-go zones (legacy-reference/, migrations history), expected CI checks, and code-review tone.

## Recommended Obsidian Documentation Structure

The target tree is the structure given in the migration prompt. The tree below also lists the seed files we should commit empty so links resolve:

```text
docs/
├── 00-start-here/index.md
├── 01-product/index.md
├── 02-architecture/
│   ├── index.md
│   ├── auth/authentication-overview.md
│   ├── authorization/permission-engine.md
│   ├── billing/billing-licensing-overview.md
│   ├── platform/platform-overview.md
│   ├── platform/solution-structure.md
│   ├── tenancy/org-hierarchy.md
│   ├── workflows/jobs-and-scheduling.md
│   └── (accounting/, ai-rag/, approvals/, audit/, data-retention/, deployment/,
│        licensing/, notifications/, observability/, payments/, reporting/,
│        search/) — start as empty index.md
├── 03-decisions/
│   ├── index.md
│   ├── accepted/
│   ├── proposed/
│   └── superseded/
├── 04-domain/
│   ├── index.md
│   ├── orgs/tmc-chain-codes.md
│   ├── support/ticket-email-templates.md
│   └── (bookings, invoices, ledger, orders, payments, policies, quotes, tickets,
│        travellers, users) — start as empty index.md
├── 05-integrations/
│   ├── index.md
│   ├── ecb/ecb-overview.md
│   └── (airplus, amadeus, conferma, email, github, oracle, pci-vault, sabre,
│        sms, stripe, travelport, uatp, xero) — start as empty index.md
├── 06-api/
│   └── index.md
├── 07-database/
│   ├── index.md
│   └── migrations/migration-policy.md
├── 08-security/
│   ├── index.md
│   └── identity/claim-taxonomy.md
├── 09-standards/
│   ├── index.md
│   └── dotnet/dotnet-10-standards.md
├── 10-runbooks/
│   ├── index.md
│   ├── billing/stripe-operations.md
│   ├── database/migrations.md
│   ├── local-dev/system-account.md
│   └── support/github-ticketing.md
├── 11-operations/index.md
├── 12-testing/index.md
├── 13-delivery/
│   ├── index.md
│   ├── backlog/backlog-2026-05.md
│   └── phases/
├── 14-ai-agents/
│   ├── index.md
│   └── claude-code/{claude-code-operating-model.md, v51-planner.agent.md}
├── 15-onboarding/index.md
├── 99-archive/
└── index.md
```

## Recommended ADR Backlog

| Proposed ADR                                                            | Current Evidence                                                                                                       | Reason                                                              | Suggested Status |
| ----------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------- | ---------------- |
| ADR-0001-use-postgresql-as-primary-database                             | `Directory.Packages.props`, `deploy/compose/compose.dev.yaml`                                                          | Codify Postgres choice + version pinning                            | accepted         |
| ADR-0002-use-blazor-server-no-wasm                                      | `src/Cinturon360.Web/Program.cs:11–14`, `QUESTIONS.md` Q2                                                              | Confirm Blazor Server, defer WASM                                   | accepted         |
| ADR-0003-use-bff-auth-pattern                                           | `src/Cinturon360.Web/Security/Bff*`                                                                                    | Cookie-only browser, JWT in claims                                  | accepted         |
| ADR-0004-use-markdownlint-cli2                                          | `claude-code-v5-to-v5-1-migration-discovery-prompt.md`                                                                 | Standardise Markdown linting                                        | accepted         |
| ADR-XXXX-tenant-query-filter                                            | absence of `HasQueryFilter`                                                                                            | Enforce tenant isolation at persistence layer                       | proposed         |
| ADR-XXXX-permission-enforcement-strategy                                | `AuthorizationBehavior` + `IRequirePermission` unused                                                                  | Choose between command tagging, endpoint policies, or both          | proposed         |
| ADR-XXXX-license-v2-canonical                                           | three-generation billing model                                                                                         | Pick License v2 as canonical and retire legacy                      | proposed         |
| ADR-XXXX-job-scheduler-supercronic-vs-ihostedservice                    | `Dockerfile.jobs` + `Cinturon360.Jobs.DependencyInjection`                                                             | End the supercronic-vs-IHostedService ambiguity                     | proposed         |
| ADR-XXXX-postgres-version-pin                                           | three different versions in three docs                                                                                 | Single supported Postgres major per env                             | proposed         |
| ADR-XXXX-claim-taxonomy-consolidation                                   | three claim/role taxonomies                                                                                            | One canonical claim source                                          | proposed         |
| ADR-XXXX-stripe-webhook-handling                                        | `docs/architecture/stripe-payments.md` + `runbook-stripe-payments.md`                                                  | Lock down idempotency, signature, and replay strategy               | proposed         |
| ADR-XXXX-pat-and-service-account-token-format                           | `TokenService.GeneratePat`                                                                                             | Standardise prefix, length, hashing, rotation                       | proposed         |
| ADR-XXXX-sso-and-scim-readiness                                         | `Integrations/IdentityProviders/*` empty                                                                               | Decide first SSO target and SCIM scope                              | proposed         |
| ADR-XXXX-audit-logging-model                                            | `UserAuditEvent`, `Application/SysLog/*` empty                                                                         | Settle audit interceptor + log retention                            | proposed         |
| ADR-XXXX-logging-and-observability                                      | Console-only sink, no metrics                                                                                          | Lock structured fields, sinks, and Azure Monitor wiring             | proposed         |
| ADR-XXXX-testing-pyramid                                                | empty test projects                                                                                                    | Define unit, integration, architecture, E2E coverage targets        | proposed         |
| ADR-XXXX-deployment-target-aca                                          | `QUESTIONS.md` Q9                                                                                                      | Codify ACA + Bicep approach                                         | proposed         |
| ADR-XXXX-backup-and-restore-strategy                                    | (none)                                                                                                                 | Cover Postgres backups + secret backup                              | proposed         |
| ADR-XXXX-ai-agent-operating-model                                       | `.github/agents/*`                                                                                                     | Codify which AI agent can do what, where                            | proposed         |
| ADR-XXXX-tmc-chain-and-branch-modelling                                 | `Organisation.ChainCode/BranchCode`, `notes.txt`                                                                        | Promote chain/branch concept to its own aggregate                   | proposed         |
| ADR-XXXX-vendor-search-function-modelling                               | `notes.txt` (hotel chain, tour operator, etc.)                                                                          | Capture the search-function category model                          | proposed         |

## Recommended Architecture Notes Backlog

- Platform overview (the §3 diagram + project responsibilities table) → `docs/02-architecture/platform/platform-overview.md`.
- Authentication overview (login flow, BFF, JWT structure, PAT generation) → `docs/02-architecture/auth/authentication-overview.md`.
- Authorization overview (permission codes, claim taxonomy, scope traversal) → `docs/02-architecture/authorization/authorization-overview.md`.
- Tenancy and org hierarchy → `docs/02-architecture/tenancy/org-hierarchy.md`.
- Billing-licensing canonical → `docs/02-architecture/billing/billing-licensing-overview.md`.
- Stripe integration → `docs/02-architecture/payments/stripe-overview.md`.
- Jobs and scheduling → `docs/02-architecture/workflows/jobs-and-scheduling.md`.
- Notifications → `docs/02-architecture/notifications/notifications-overview.md`.
- Observability → `docs/02-architecture/observability/observability-overview.md`.
- Search and reference data (geography) → `docs/02-architecture/search/search-and-reference-data.md`.
- Reporting (Phase 11+) → `docs/02-architecture/reporting/reporting-overview.md`.

## Recommended Domain Documentation Backlog

- Bookings lifecycle and status transitions → `docs/04-domain/bookings/booking-lifecycle.md`.
- Quotes (Duffel offer snapshot, expiry) → `docs/04-domain/quotes/quote-lifecycle.md`.
- Approvals (0–3 levels, expiry, notification) → `docs/04-domain/approvals/approvals-model.md`.
- Travel policies (rules, evaluation, assignment) → `docs/04-domain/policies/travel-policy-model.md`.
- Travellers (profile, loyalty, addresses, preferences, emergency contacts) → `docs/04-domain/travellers/traveller-profile.md`.
- Users (HumanUser, ServiceAccount, MFA, sessions, PATs) → `docs/04-domain/users/identity-model.md`.
- Orgs (Vendor / TMC / Client, chain/branch, sudo) → `docs/04-domain/orgs/org-model.md` + `docs/04-domain/orgs/tmc-chain-codes.md`.
- Invoices and ledger → `docs/04-domain/invoices/invoice-model.md`, `docs/04-domain/ledger/ledger-model.md`.
- Payments (relationships, customers, methods, attempts) → `docs/04-domain/payments/payments-model.md`.
- Tickets (queues, escalations, attachments, email templates) → `docs/04-domain/tickets/ticket-model.md` + the existing `support-ticket-email-templates.md` migrated.

## Recommended Runbook Backlog

- Local dev start and stop → `docs/10-runbooks/local-dev/local-dev-start.md`.
- Generate and apply migrations → `docs/10-runbooks/database/migrations.md`.
- Backup and restore Postgres → `docs/10-runbooks/database/backup-restore.md`.
- Create / disable system-account → `docs/10-runbooks/local-dev/system-account.md`.
- Stripe operations (connections, webhooks, secrets, troubleshooting) → `docs/10-runbooks/billing/stripe-operations.md`.
- GitHub ticketing (label seeding, PAT rotation, webhook setup) → `docs/10-runbooks/support/github-ticketing.md`.
- Incident response: auth failure spike, job failure spike, error-rate spike → `docs/10-runbooks/incidents/`.
- Maintenance mode (Caddy + compose) → `docs/10-runbooks/maintenance/maintenance-mode.md`.

## Recommended Engineering Standards Backlog

- .NET 10 standards (target framework, central package management, nullable, analysis level) → `docs/09-standards/dotnet/dotnet-10-standards.md`.
- C# coding standards (file-per-type, factory methods, immutable IDs) → `docs/09-standards/csharp/csharp-conventions.md`.
- Blazor standards (server-only, Tailwind + Fluent UI hybrid, Razor file conventions) → `docs/09-standards/blazor/blazor-conventions.md`.
- EF Core standards (snake_case, no shared DbContext per host, repository pattern, query filters) → `docs/09-standards/ef-core/ef-core-conventions.md`.
- Postgres standards (naming, indexes, constraints, partitioning) → `docs/09-standards/postgres/postgres-conventions.md`.
- Logging standards (structured fields, levels, redaction) → `docs/09-standards/logging/logging-standards.md`.
- Errors standards (RFC 7807, error codes, FluentValidation surface) → `docs/09-standards/errors/error-handling.md`.
- Testing standards (pyramid, naming, ArchUnit rules) → `docs/09-standards/testing/testing-standards.md`.
- Naming standards (project namespaces, class names, file names, Razor names) → `docs/09-standards/naming/naming-standards.md`.
- Git standards (branch naming, commit message format, PR template, signed commits) → `docs/09-standards/git/git-standards.md`.

## Recommended AI Agent Instructions

The current AI-agent footprint is `.github/agents/cinturon360-v5-planner.agent.md`. The v5.1 model should:

- Move that file into `docs/14-ai-agents/claude-code/v51-planner.agent.md` and update its references to `v51-plan.md` once the plan is rebased.
- Author `docs/14-ai-agents/claude-code/claude-code-operating-model.md` covering:
  - Where the agent may write: `docs/`, source under `src/` (with PR), tests, `tools/`. Never `legacy-reference/`, never historic migration files.
  - Behaviours to avoid: writing into `node_modules/`, regenerating `*.Designer.cs` migrations by hand, modifying `Directory.Packages.props` versions without ADR.
  - Required confirmations: every new ADR file must have a draft commit message starting with `docs(adr):`.
  - Pre-flight checks: agent must run `dotnet build` and `markdownlint-cli2` before reporting "done".
  - Prompt-engineering format: each agent task must include the file paths, the line numbers, and the contracted output type.
- Provide a Claude task library at `docs/14-ai-agents/claude-code/tasks/` covering common chores: add a new EF migration, add a new endpoint group, scaffold a new ADR, scaffold a new runbook.
- Add `docs/14-ai-agents/guardrails/guardrails.md` listing destructive-operation refusals (drop tables, force push, secret printing, mass file deletion).

## Unknowns and Follow-Up Questions

### Unknown: Production Postgres provider

What is unknown:

- `QUESTIONS.md` Q8 lists PlanetScale Postgres as preferred and Azure DB as fallback. PlanetScale Postgres availability for production .NET workloads is not validated in code.

Why it matters:

- Connection-string format, TLS, secret rotation, and migration tooling all hinge on this choice.

Evidence checked:

- `QUESTIONS.md` Q8
- `Directory.Packages.props` (Npgsql 10.0.x)

Recommended follow-up:

- Confirm provider before Phase 11 starts; author ADR.

### Unknown: SAML enable date

What is unknown:

- `REMAINING-PHASES.md` "Notes / Open Decisions" lists "SAML 2.0 — Enable early or with SSO phase?" as undecided.

Why it matters:

- ITfoxtec.Identity.Saml2 is referenced; its `System.Drawing.Common` transitive risk was mitigated in Q3.

Evidence checked:

- `REMAINING-PHASES.md` Notes
- `QUESTIONS.md` Q3, Q6

Recommended follow-up:

- Pick a calendar date or a phase. ADR.

### Unknown: Audit retention window

What is unknown:

- No ADR or BACKLOG entry pins a retention window for audit events, sessions, or webhook events.

Why it matters:

- Required for ISO 27001 / SOC 2 readiness and for the Audit log archival job.

Evidence checked:

- `BACKLOG.md` Background Jobs section ("Audit log archival job")
- absence of any `RetentionPolicy` document

Recommended follow-up:

- Choose 7 / 365 / 2555 days per category. ADR.

### Unknown: Public API surface

What is unknown:

- The current API has only an internal-style surface. The plan mentions "Public API vs internal API boundaries" but the boundary is not implemented.

Why it matters:

- v5.1 may need to expose a public partner API (TMC integrations, traveller-profile sync).

Evidence checked:

- `src/Cinturon360.Api/Endpoints/Public/` empty
- `src/Cinturon360.Contracts/Public/` exists but content not surveyed

Recommended follow-up:

- Decide if a `/public/v1/...` host is in scope. Spec the auth model (PAT vs OAuth client credentials) before building.

### Unknown: Search-function model

What is unknown:

- `notes.txt` introduces a vendor-controlled "search function" concept covering Hotel, Hotel Chain, Tour Operator, Car Company, Airline, Bus Operator, Cruise, Railway, Consolidator, Insurance, Charter, Correspondent, Hotel Package. There is no domain entity for this.

Why it matters:

- This is a wide product surface. Without an entity, vendor onboarding cannot configure category enablement.

Evidence checked:

- `notes.txt` lines 22–40
- absence of any matching entity in `src/Cinturon360.Domain/Entities/Travel/`

Recommended follow-up:

- Spec the entity (`SearchFunction` per org per category) and the chain-vs-branch enablement.

### Unknown: TMC chain-vs-branch first-class modelling

What is unknown:

- `Organisation.ChainCode`, `ChainName`, `BranchCode` are flat strings on `Organisation`. The notes describe a hierarchy of "Chain Group → Branch".

Why it matters:

- Rolling up data, billing, or merging search results across a chain requires queries by chain code today.

Evidence checked:

- `src/Cinturon360.Domain/Entities/Organization/Organisation.cs:33–38`
- `notes.txt` lines 9–20

Recommended follow-up:

- Promote chain to an aggregate (`OrgChain` with a `Code`, `Name`, `Owner`) and a many-to-one from `Organisation` to `OrgChain`.

## Final Recommendations

Top priorities for the v5.1 cut, ordered.

1. Decide the v5.1 source-tree strategy: copy `cinturon360.dev/` to `cinturon360-v5.1/` with the legacy-reference removed; cut a `legacy/v4-reference` branch from current `main`; tag the v5 commit so v5.1 can rebase from a fixed point.
2. Stand up the `docs/` Obsidian skeleton in v5.1 and migrate the existing root-level Markdown using the table in "Existing Markdown Documentation Review".
3. Author the first batch of ADRs (database, Blazor, BFF, markdownlint, license v2 canonical, permission enforcement, tenant query filter, job scheduler).
4. Replace empty test projects with seed coverage: ArchUnit rules for layer boundaries, two Domain unit tests, one Data integration test using Testcontainers.PostgreSql.
5. Wire CI: GitHub Actions workflow that runs `dotnet build`, `dotnet test`, markdownlint, and uploads test results.
6. Tighten reference graph: remove `Cinturon360.Web → Cinturon360.Domain` and `Cinturon360.Contracts → Cinturon360.Domain` references; relocate any Domain-typed shapes used in UI into Contracts.
7. Tag every command/query with `IRequirePermission`. Add `AddAuthorization` policies. Replace `RequireAuthorization()` on endpoints with a permission-aware policy attribute.
8. Add a tenant query filter (`ITenantContext`) and remove the legacy billing model after a one-shot conversion migration.
9. Add a correlation-id middleware, a `/health` endpoint, and the production Serilog console-only configuration.
10. Decide and either implement or remove supercronic in the jobs container.
11. Build the License v2 execution layer (booking-time evaluation, `BillingAccount` activation, invoice generation, ledger posting, collection-policy enforcement). This single body of work clears most of `BACKLOG.md` Billing.

Once those are complete, the platform will have:

- a single source of truth for documentation,
- enforced layer boundaries,
- enforced authentication and authorization,
- enforced tenancy isolation,
- a real testing baseline,
- a real CI/CD pipeline,
- a single canonical billing model.

The remaining product work (real Amadeus integration, hotels/cars/rail provider selection, SSO, MFA, mobile app) can then be sequenced against the cleaned foundation rather than against the current mid-build state.
