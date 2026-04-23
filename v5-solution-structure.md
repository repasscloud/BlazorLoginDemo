# Cinturon360 v5 — Solution & Project Structure

> Companion to `v5-plan.md`

---

## Solution Root Layout

```
cinturon360.dev/
├─ Cinturon360.sln
├─ Directory.Build.props          # Shared MSBuild properties (TFM, Nullable, etc.)
├─ Directory.Packages.props       # Central NuGet package version management
├─ global.json                    # Pinned SDK version
├─ .editorconfig
├─ .gitignore
│
├─ build/
│  ├─ versioning/                 # Version stamping scripts
│  ├─ scripts/                    # Build/test helper scripts
│  └─ pipelines/                  # CI/CD YAML (GitHub Actions / Azure Pipelines)
│
├─ docs/
│  ├─ architecture/               # ADRs, diagrams
│  ├─ api/                        # OpenAPI / Swagger exports
│  ├─ policies/                   # Travel policy documentation
│  └─ runbooks/                   # Operational runbooks
│
├─ deploy/
│  ├─ docker/                     # Per-service Dockerfiles
│  ├─ compose/                    # docker-compose files (dev, staging, prod)
│  ├─ caddy/                      # Caddy reverse proxy config
│  └─ azure/                      # Azure Container Apps / infrastructure-as-code
│
├─ tools/
│  ├─ db/                         # Migration scripts, seed scripts
│  ├─ codegen/                    # Any code generation tooling
│  └─ maintenance/                # Utility scripts
│
├─ src/
│  ├─ Cinturon360.Api/
│  ├─ Cinturon360.Web/
│  ├─ Cinturon360.Application/
│  ├─ Cinturon360.Domain/
│  ├─ Cinturon360.Contracts/
│  ├─ Cinturon360.Common/
│  ├─ Cinturon360.Data/
│  ├─ Cinturon360.Integrations/
│  ├─ Cinturon360.Infrastructure/
│  └─ Cinturon360.Jobs/
│
├─ tests/
│  ├─ Cinturon360.Api.Tests/
│  ├─ Cinturon360.Application.Tests/
│  ├─ Cinturon360.Domain.Tests/
│  ├─ Cinturon360.Data.Tests/
│  ├─ Cinturon360.Integration.Tests/
│  ├─ Cinturon360.Web.Tests/
│  └─ Cinturon360.Architecture.Tests/   # ArchUnitNET layer boundary tests
│
└─ legacy-reference/                    # All v4 and earlier files — READ ONLY
```

---

## Project Responsibilities

### `Cinturon360.Api`

Thin HTTP surface. Owns nothing except routing and request/response wiring.

```
Cinturon360.Api/
├─ Endpoints/
│  ├─ Auth/
│  ├─ Public/
│  ├─ Vendor/
│  ├─ Tmc/
│  ├─ Client/
│  ├─ Travel/
│  ├─ Booking/
│  ├─ Policy/
│  ├─ Approvals/
│  ├─ Ticketing/
│  ├─ Reports/
│  ├─ Storage/
│  ├─ Admin/
│  └─ Feeds/
├─ Filters/
├─ Middleware/
├─ OpenApi/
├─ Serialization/
├─ DependencyInjection/
├─ Configuration/
└─ Program.cs
```

**Must not** contain: domain logic, EF entities, business rules, validation beyond transport shape.

---

### `Cinturon360.Web`

Blazor UI (Server or WASM). Owns presentation only. **No auth of its own** — delegates entirely to the API.

```
Cinturon360.Web/
├─ Components/
│  ├─ Layout/
│  ├─ Navigation/
│  ├─ Forms/
│  ├─ Tables/
│  └─ Shared/
├─ Pages/
│  ├─ Public/
│  ├─ Auth/
│  ├─ Dashboard/
│  ├─ Vendor/
│  ├─ Tmc/
│  ├─ Client/
│  ├─ Travel/
│  ├─ Booking/
│  ├─ Policy/
│  ├─ Approvals/
│  ├─ Reports/
│  ├─ Ticketing/
│  ├─ Settings/
│  └─ Sudo/
├─ ViewModels/
│  ├─ Auth/
│  ├─ Booking/
│  ├─ Policy/
│  ├─ Reports/
│  └─ Shared/
├─ Services/
│  ├─ ApiClients/         # Typed HTTP clients for the API
│  ├─ State/
│  ├─ Session/
│  └─ Navigation/
├─ Security/              # Auth state provider wiring (reads API tokens, no owns auth)
├─ DependencyInjection/
├─ wwwroot/
│  ├─ css/
│  ├─ js/
│  ├─ img/
│  └─ lib/
├─ App.razor
├─ Routes.razor
└─ Program.cs
```

---

### `Cinturon360.Application`

**The most important project.** Translation and orchestration boundary between Web/API and Data.

```
Cinturon360.Application/
├─ Abstractions/
│  ├─ Persistence/        # Repository interfaces
│  ├─ Integrations/       # Integration service interfaces
│  ├─ Storage/
│  ├─ Jobs/
│  ├─ Logging/
│  └─ Security/
├─ Features/              # Vertical slices by domain feature
│  ├─ Auth/
│  │  ├─ Commands/
│  │  ├─ Queries/
│  │  ├─ Models/
│  │  ├─ Validators/
│  │  └─ Services/
│  ├─ Users/
│  ├─ Organizations/
│  ├─ Roles/
│  ├─ Travel/
│  ├─ Bookings/
│  ├─ Quotes/
│  ├─ Geography/
│  ├─ Policies/
│  ├─ Approvals/
│  ├─ Reporting/
│  ├─ Ticketing/
│  ├─ Storage/
│  ├─ Pdf/
│  ├─ Jobs/
│  ├─ DataFeeds/
│  └─ Billing/
├─ Behaviors/             # MediatR pipeline behaviors if used
│  ├─ Validation/
│  ├─ Logging/
│  ├─ Authorization/
│  └─ Transactions/
├─ Common/
│  ├─ Results/
│  ├─ Exceptions/
│  ├─ Paging/
│  ├─ Precision/
│  ├─ Dates/
│  └─ Mapping/
├─ Security/
│  ├─ AppClaims/
│  ├─ AppPolicies/
│  ├─ AppRoles/
│  ├─ OrgRoles/
│  ├─ OrgRoleRequirements/
│  ├─ PermissionEvaluation/
│  └─ TokenIssuance/
├─ SysLog/
│  ├─ Models/
│  ├─ Writers/
│  ├─ Enrichers/
│  └─ Events/
├─ IdGeneration/
│  ├─ Models/
│  ├─ Services/
│  └─ Helpers/
├─ Validation/
│  ├─ Attributes/
│  ├─ Rules/
│  └─ Validators/
├─ Mappers/
│  ├─ Contracts/
│  ├─ Entities/
│  ├─ Unified/
│  └─ External/
├─ UnifiedModels/
│  ├─ Travel/
│  ├─ Geography/
│  ├─ Policies/
│  └─ Pricing/
├─ Services/
│  ├─ Travel/
│  ├─ Booking/
│  ├─ Policy/
│  ├─ Pricing/
│  ├─ Notifications/
│  ├─ Ticketing/
│  └─ Documents/
└─ DependencyInjection/
```

---

### `Cinturon360.Domain`

Pure business concepts. No EF, no HTTP, no DI framework dependencies.

```
Cinturon360.Domain/
├─ Common/
│  ├─ Base/               # Entity<T>, AggregateRoot
│  ├─ Interfaces/
│  ├─ ValueObjects/       # Money, DateRange, OrgPath, etc.
│  └─ Exceptions/
├─ Entities/
│  ├─ Identity/
│  ├─ Organization/
│  ├─ Travel/
│  ├─ Booking/
│  ├─ Policy/
│  ├─ Pricing/
│  ├─ Ticketing/
│  ├─ Storage/
│  └─ Logging/
├─ Enums/
│  ├─ Security/
│  ├─ Travel/
│  ├─ Policy/
│  ├─ Booking/
│  ├─ Ticketing/
│  └─ System/
├─ Events/
├─ Services/              # Pure domain services (no infra dependencies)
└─ Specifications/
```

---

### `Cinturon360.Contracts`

Stable, versionable request/response shapes. Shared by API and Web. No EF, no business logic.

```
Cinturon360.Contracts/
├─ Common/
│  ├─ Paging/
│  ├─ Results/
│  ├─ Errors/
│  ├─ Precision/
│  └─ Metadata/
├─ Auth/
├─ Users/
├─ Organizations/
├─ Roles/
├─ Travel/
├─ Bookings/
├─ Quotes/
├─ Geography/
├─ Policies/
├─ Approvals/
├─ Reports/
├─ Ticketing/
├─ Storage/
├─ Billing/
├─ Public/
├─ Feeds/
└─ SysLog/
```

---

### `Cinturon360.Common`

Genuinely cross-project primitives only. Not a junk drawer.

```
Cinturon360.Common/
├─ Constants/
├─ Extensions/
├─ Precision/             # Money, rate, tax, qty precision conventions
├─ Serialization/
├─ Dates/
├─ IdGeneration/          # Prefixed domain IDs (e.g. usr_, org_, bkg_)
├─ Helpers/               # Focused only: formatting, normalization, value parsing
├─ Results/               # Result<T>, Error wrapper
└─ StaticData/
   ├─ Enums/
   ├─ Codes/
   └─ Lookups/
```

---

### `Cinturon360.Data`

PostgreSQL persistence. Entity configs, DbContext, repositories, migrations.

```
Cinturon360.Data/
├─ Context/
│  ├─ ApplicationDbContext.cs
│  ├─ ModelBuilderExtensions.cs     # Extension-method based registration
│  └─ DbContextFactory.cs           # For design-time / migration tooling
├─ Entities/                        # Persistence entities (NOT domain models)
│  ├─ Identity/
│  ├─ Organization/
│  ├─ Travel/
│  ├─ Booking/
│  ├─ Policy/
│  ├─ Pricing/
│  ├─ Ticketing/
│  ├─ Storage/
│  ├─ Jobs/
│  ├─ Logging/
│  └─ Integrations/
├─ Configurations/                  # IEntityTypeConfiguration<T> per entity
│  ├─ Identity/
│  ├─ Organization/
│  ├─ Travel/
│  ├─ Booking/
│  ├─ Policy/
│  ├─ Pricing/
│  ├─ Ticketing/
│  ├─ Storage/
│  ├─ Jobs/
│  ├─ Logging/
│  └─ Integrations/
├─ Repositories/
├─ Queries/
├─ Migrations/                      # EF Core generated migrations
├─ Seeds/                           # Dev/staging seed data
├─ Precision/                       # EF decimal precision conventions
├─ Interceptors/                    # Audit interceptor, soft-delete interceptor
└─ DependencyInjection/
```

**Migration strategy**: `dotnet ef migrations script` generates SQL. Script is applied by the deploy pipeline before containers restart. No migrator container.

---

### `Cinturon360.Integrations`

All third-party API integrations. Fully isolated from internal contracts.

```
Cinturon360.Integrations/
├─ Common/
│  ├─ Http/               # Base HTTP client, retry policies (Polly)
│  ├─ Auth/               # Provider auth flows
│  ├─ Resilience/
│  ├─ Mapping/            # Base external → internal mapper
│  └─ Logging/
├─ Geography/             # Airport/city/country reference data provider
├─ Flights/               # Amadeus, Duffel, etc.
├─ Hotels/
├─ Cars/
├─ Rail/
├─ Payments/              # Stripe
├─ ExchangeRates/
├─ GitHub/
│  ├─ Ticketing/
│  ├─ Models/
│  ├─ Services/
│  └─ Mapping/
├─ Aws/
│  ├─ S3/
│  └─ Models/
└─ IdentityProviders/
   ├─ Oidc/
   ├─ Saml/
   ├─ Scim/
   ├─ Google/
   ├─ Microsoft/
   ├─ Apple/
   └─ Facebook/
```

Each provider has: its own models, a service, a mapper to internal unified model, retry/error handling.

---

### `Cinturon360.Infrastructure`

Cross-cutting infrastructure concerns that are not business logic.

```
Cinturon360.Infrastructure/
├─ Jobs/
│  ├─ Queue/
│  ├─ Recurring/
│  ├─ Workers/
│  ├─ Cron/
│  └─ Scheduling/
├─ Storage/
│  ├─ Documents/
│  ├─ Pdf/
│  ├─ S3/
│  └─ Cleanup/
├─ Logging/
│  ├─ Sinks/
│  ├─ Enrichers/         # Org, tenant, correlation enrichers
│  ├─ Formatters/
│  └─ Correlation/
├─ Email/                # MailerSend or similar
├─ Sms/
├─ Caching/              # Minimal in-process cache if ever needed
├─ Security/             # JWT signing, key management
└─ DependencyInjection/
```

---

### `Cinturon360.Jobs`

Standalone runnable host — executes inside the Supercronic container.

```
Cinturon360.Jobs/
├─ HostedServices/
├─ Recurring/
│  ├─ DataFeeds/
│  ├─ StorageCleanup/
│  ├─ PdfCleanup/
│  ├─ TicketSync/
│  ├─ ReferenceDataRefresh/
│  └─ BillingReconciliation/
├─ QueueProcessors/
├─ DependencyInjection/
└─ Program.cs
```

---

## Key Design Rules for the Solution

| Rule | Rationale |
|---|---|
| API references Application, not Domain or Data directly | Keeps API thin |
| Web references Contracts only, calls API via typed HTTP clients | No Web→Data leakage |
| Application references Domain and Contracts | Orchestration layer owns translation |
| Data implements Application abstractions (repositories) | Dependency inversion |
| Integrations referenced only by Application/Infrastructure | Provider models never leak to API contracts |
| Common and Contracts referenced by all | Safe — no business logic inside them |
| Architecture tests (`Cinturon360.Architecture.Tests`) enforce the above | Regressions caught in CI |

---

## NuGet Packages (Anticipated)

| Package | Used in |
|---|---|
| `Microsoft.EntityFrameworkCore` | Data |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | Data |
| `MediatR` | Application (optional CQRS) |
| `FluentValidation` | Application |
| `Polly` | Integrations |
| `Serilog` | Infrastructure.Logging |
| `StripeClient` | Integrations.Payments |
| `AWSSDK.S3` | Integrations.Aws |
| `Duende.IdentityServer` or `OpenIddict` | Api (auth server) |
| `ITfoxtec.Identity.Saml2` | Integrations.IdentityProviders.Saml |
| `ArchUnitNET` | Tests/Architecture |
