# Claude Code Prompt: Cinturon360 v5 to v5.1 Migration Discovery

## Purpose

You are running inside the existing Cinturon360 v5 repository.

Your job is **not** to write production code.

Your job is to perform a deep discovery pass over the current platform and produce a detailed Markdown knowledge capture file that will be used as the foundation for the Cinturon360 v5.1 migration.

The v5.1 migration will copy the existing source code into a new project folder, preserve the current v5 repository as-is, and then continue development with a stronger documentation, decision, architecture, engineering, and AI-agent operating model.

The current platform has grown with fragmented documentation and some implemented decisions that may no longer be suitable. The goal is to create a complete, useful, technically accurate foothold before refactoring or rebuilding anything.

## Output File

Create a new Markdown file at the repository root:

```text
claude-migration-to-v5.1.md
```

This file must be detailed enough that a future Claude Code session, developer, architect, contractor, or engineering lead can understand the current v5 platform before starting v5.1 work.

Do not overwrite existing documentation.

Do not modify source code.

Do not create migrations, tests, or refactors.

Only inspect, analyse, and document.

## Reasoning Mode

Use maximum available reasoning.

Think deeply before writing.

Do not rush to summarise.

Prioritise accuracy, completeness, and usefulness over brevity.

Where the repository contains conflicting information, document the conflict rather than trying to hide it.

Where the implementation differs from Markdown documentation, document both the stated design and the actual implemented behaviour.

Where something is unclear, mark it as unknown and explain what needs follow-up.

## Working Assumptions

Cinturon360 v5 is the existing platform.

Cinturon360 v5.1 will be the next working copy.

The current v5 repository should be treated as the source of implementation truth.

The existing Markdown files should be treated as documentation evidence, but not automatically assumed to be correct.

The new v5.1 documentation model will use an Obsidian-style documentation structure under `docs/`.

The purpose of the v5.1 documentation model is to reduce fragmentation, capture decisions clearly, support AI-assisted development, and onboard future developers or contractors.

## What You Must Review

Review the entire repository.

Include, at minimum:

- Solution files
- Project files
- Source code
- API projects
- Web/UI projects
- Shared projects
- Domain models
- DTOs
- Enums
- Services
- Data access
- EF Core DbContexts
- Migrations
- Seed data
- Authentication and authorization code
- Configuration files
- Docker files
- Docker Compose files
- Caddy files
- CI/CD files
- GitHub workflows
- Scripts
- Tests
- Existing Markdown documentation
- README files
- CLAUDE.md files
- Agent prompt files
- Architecture notes
- ADRs
- Runbooks
- Deployment notes
- Local development notes
- Any docs that appear stale, duplicated, or inconsistent

## Directories and Files to Skip

Skip generated, build, cache, and dependency folders unless they contain handwritten project documentation.

Skip:

```text
.git/
bin/
obj/
node_modules/
.vs/
.vscode/.ropeproject/
packages/
artifacts/
coverage/
TestResults/
dist/
build/
.tmp/
.cache/
```

Also skip binary files unless the filename or location strongly suggests it contains architectural information.

Do not spend time reading compiled outputs.

## Core Deliverable

Write `claude-migration-to-v5.1.md` as a structured discovery report.

The report must explain:

1. What the current application is
2. What the repository contains
3. How the application is laid out
4. How the projects relate to each other
5. How the domain model currently works
6. How authentication currently works
7. How authorization currently works
8. How tenancy and organisation hierarchy currently work
9. How users, roles, claims, and permissions are represented
10. How billing, licensing, payments, accounting, invoices, ledger, and entitlements are represented
11. How bookings, quotes, orders, tickets, travellers, policies, and workflows are represented
12. How integrations are represented
13. How APIs are structured
14. How the database is structured
15. How EF Core is used
16. How migrations are managed
17. How local development is run
18. How Docker and reverse proxying are configured
19. How deployment appears to be intended to work
20. How logging, monitoring, and diagnostics appear to work
21. How testing is structured
22. How existing documentation is structured
23. Which Markdown files are useful
24. Which Markdown files are stale, incomplete, duplicated, or misleading
25. Which implementation decisions appear suitable for v5.1
26. Which implementation decisions may need refactoring in v5.1
27. Which parts of the system are well understood
28. Which parts require follow-up investigation
29. Which areas should become ADRs
30. Which areas should become architecture notes
31. Which areas should become runbooks
32. Which areas should become domain documentation
33. Which areas should become engineering standards
34. Which areas should become onboarding documentation
35. How the current v5 platform should be carried forward into v5.1

## Required Report Structure

Use this structure in `claude-migration-to-v5.1.md`.

Do not use multiple top-level H1 headings.

Use one H1 only.

```markdown
# Cinturon360 v5 to v5.1 Migration Discovery

## Executive Summary

## Repository Snapshot

## Application Purpose

## Current Technology Stack

## Solution and Project Layout

## Application Runtime Layout

## Infrastructure and Deployment Layout

## Local Development Model

## Configuration Model

## Database Model

## Entity Framework Core Usage

## Domain Model Overview

## Organisation and Tenancy Model

## User, Identity, Role, Claim, and Permission Model

## Authentication Model

## Authorization Model

## API Surface

## Web/UI Surface

## Background Jobs, Workers, and Scheduled Processing

## Integrations

## Billing, Licensing, Payments, Accounting, and Entitlements

## Booking, Quote, Order, Ticket, Traveller, and Policy Workflows

## Notifications and Messaging

## Logging, Observability, Diagnostics, and Audit

## Security and Compliance Posture

## Testing Model

## Existing Documentation Inventory

## Existing Markdown Documentation Review

## Existing ADR and Decision Review

## Implementation vs Documentation Gaps

## Current Technical Debt

## Current Product/Engineering/Development Gaps

## v5.1 Migration Risks

## v5.1 Refactoring Candidates

## v5.1 Documentation Migration Plan

## Recommended Obsidian Documentation Structure

## Recommended ADR Backlog

## Recommended Architecture Notes Backlog

## Recommended Domain Documentation Backlog

## Recommended Runbook Backlog

## Recommended Engineering Standards Backlog

## Recommended AI Agent Instructions

## Unknowns and Follow-Up Questions

## Final Recommendations
```

## Documentation Style Requirements

Write in clear technical English.

Do not use marketing language.

Do not overstate certainty.

Use short paragraphs.

Use tables where they improve clarity.

Use bullet lists for inventories and findings.

For each major finding, include evidence by referencing filenames and paths.

When useful, include symbols, class names, method names, enum names, configuration keys, service names, route names, and project names.

Do not paste large source code blocks unless needed.

Prefer concise examples over long code excerpts.

Do not write vague statements such as “the system has authentication.”

Instead, write specific statements such as:

```text
Authentication is configured in <path> using <scheme>. Tokens/cookies appear to be handled by <class>. The current implementation suggests <finding>.
```

## Evidence Rules

Every important claim should be traceable back to repository evidence.

For each important finding, include one or more of:

- File path
- Class name
- Interface name
- Method name
- Enum name
- Configuration key
- Docker service name
- Project file name
- Markdown file path
- Migration file name

Use this format when practical:

```markdown
Evidence:

- `src/.../Example.cs`
- `docs/.../example.md`
- `docker-compose.yml`
```

## Review Existing Markdown Carefully

Review every Markdown file in the repository.

For each Markdown file or logical documentation group, determine:

- What it currently explains
- Whether it appears current
- Whether it conflicts with implementation
- Whether it belongs in the v5.1 documentation model
- Whether it should be retained, rewritten, archived, merged, or deleted
- Which future `docs/` folder it should map into

The existing Markdown may not follow the intended v5.1 documentation structure.

Do not assume the current location is correct.

Document recommended movements.

## Intended v5.1 Documentation Structure

The target v5.1 documentation model is approximately:

```text
docs/
├── 00-start-here/
│   └── index.md
├── 01-product/
│   └── index.md
├── 02-architecture/
│   ├── accounting/
│   ├── ai-rag/
│   ├── approvals/
│   ├── audit/
│   ├── auth/
│   │   └── authentication-overview.md
│   ├── authorization/
│   ├── billing/
│   │   └── billing-licensing-overview.md
│   ├── data-retention/
│   ├── deployment/
│   ├── index.md
│   ├── licensing/
│   ├── notifications/
│   ├── observability/
│   ├── payments/
│   ├── platform/
│   │   └── platform-overview.md
│   ├── reporting/
│   ├── search/
│   ├── tenancy/
│   │   └── org-hierarchy.md
│   └── workflows/
├── 03-decisions/
│   ├── accepted/
│   │   ├── ADR-0001-use-postgresql.md
│   │   ├── ADR-0002-use-blazor-server.md
│   │   ├── ADR-0003-use-bff-auth-pattern.md
│   │   └── ADR-0004-use-markdownlint-cli2.md
│   ├── index.md
│   ├── proposed/
│   └── superseded/
├── 04-domain/
│   ├── bookings/
│   ├── index.md
│   ├── invoices/
│   ├── ledger/
│   ├── orders/
│   ├── orgs/
│   ├── payments/
│   ├── policies/
│   ├── quotes/
│   ├── support/
│   ├── tickets/
│   ├── travellers/
│   └── users/
├── 05-integrations/
│   ├── airplus/
│   ├── amadeus/
│   ├── conferma/
│   ├── email/
│   ├── github/
│   ├── index.md
│   ├── oracle/
│   ├── pci-vault/
│   ├── sabre/
│   ├── sms/
│   ├── stripe/
│   ├── travelport/
│   ├── uatp/
│   └── xero/
├── 06-api/
│   ├── authentication/
│   ├── errors/
│   ├── examples/
│   ├── index.md
│   ├── internal-api/
│   ├── public-api/
│   ├── versioning/
│   └── webhooks/
├── 07-database/
│   ├── backup-restore/
│   ├── index.md
│   ├── indexes/
│   ├── migrations/
│   ├── partitioning/
│   ├── schema/
│   └── seed-data/
├── 08-security/
│   ├── identity/
│   ├── index.md
│   ├── iso27001/
│   ├── mfa/
│   ├── pats/
│   ├── pci-dss/
│   ├── scim/
│   ├── secrets/
│   ├── soc2/
│   ├── sso/
│   └── threat-models/
├── 09-standards/
│   ├── blazor/
│   ├── csharp/
│   ├── dotnet/
│   │   └── dotnet-10-standards.md
│   ├── ef-core/
│   ├── errors/
│   ├── git/
│   ├── index.md
│   ├── logging/
│   ├── naming/
│   ├── postgres/
│   └── testing/
├── 10-runbooks/
│   ├── database/
│   ├── deployment/
│   ├── docker/
│   ├── incidents/
│   ├── index.md
│   ├── local-dev/
│   ├── maintenance/
│   └── support/
├── 11-operations/
│   ├── access-management/
│   ├── backups/
│   ├── environments/
│   ├── index.md
│   ├── monitoring/
│   └── release-management/
├── 12-testing/
│   ├── e2e/
│   ├── index.md
│   ├── integration/
│   ├── performance/
│   ├── security/
│   ├── uat/
│   └── unit/
├── 13-delivery/
│   ├── backlog/
│   ├── index.md
│   ├── phases/
│   ├── release-notes/
│   ├── roadmap/
│   └── sprint-plans/
├── 14-ai-agents/
│   ├── claude-code/
│   │   └── claude-code-operating-model.md
│   ├── guardrails/
│   ├── index.md
│   ├── prompts/
│   ├── reviews/
│   └── tasks/
├── 15-onboarding/
│   ├── contractors/
│   ├── developers/
│   ├── finance/
│   ├── index.md
│   └── support/
├── 99-archive/
└── index.md
```

Use this as the intended target model when recommending where current documentation should move.

## Product, Engineering, and Development Distinction

As part of the review, separate findings into:

- Product Development concerns
- Engineering concerns
- Development implementation concerns

Use this distinction when reviewing gaps, risks, and future work.

Product Development is about what should be built and why.

Engineering is about how the platform should be designed, operated, secured, scaled, observed, and maintained.

Development is about implementing and shipping application code.

When you identify a gap, classify it where possible:

```markdown
| Gap | Category | Evidence | Recommended v5.1 Action |
|---|---|---|---|
| Example | Engineering | `src/...` | Create ADR or architecture note |
```

## Specific Areas to Investigate Deeply

### Platform Shape

Document:

- Number of projects
- Project names
- Project responsibilities
- Dependency direction
- Entry points
- Startup/configuration files
- Runtime services
- Hosting model
- Whether the application appears modular, layered, monolithic, or mixed

### .NET Version and Framework Usage

Document:

- Target frameworks
- ASP.NET Core usage
- Blazor usage
- Web API usage
- Background worker usage
- EF Core usage
- NuGet packages that affect architecture
- Any version mismatches or upgrade risks

### Authentication

Document:

- Login flow
- Cookie usage
- JWT usage
- BFF/proxy usage
- API auth
- Web auth
- Token storage
- Session handling
- Claims creation
- External identity readiness
- SSO/SCIM readiness
- PAT/service token readiness

### Authorization

Document:

- Role model
- Claim model
- Policy model
- Permission model
- Org-scoped access model
- Tenant traversal model
- Sudo/global admin model if present
- Gaps between intended and implemented access control

### Tenancy and Organisation Hierarchy

Document:

- Org entities
- Parent-child relationships
- Vendor/TMC/Client concepts if implemented
- Sudo/platform-level access if implemented
- Tenant isolation assumptions
- Data ownership assumptions
- Query filtering approach
- Any risks of cross-tenant leakage

### Database

Document:

- Database provider
- Connection string pattern
- Schema design
- Migration history
- Naming conventions
- Indexes
- Constraints
- Soft delete usage
- Audit columns
- Tenant columns
- Partitioning usage if any
- Seed data usage
- Backup/restore notes if present

### Billing, Licensing, Payments, Accounting

Document:

- Billing entities
- Payment entities
- Invoice entities
- Ledger entities
- Entitlement entities
- Stripe-specific implementation
- Provider-neutral abstraction if present
- Accounting integration assumptions
- Payment lifecycle
- Refund/credit/debit handling
- Commercial risk boundaries if documented
- v5.1 refactor risks

### Travel Domain

Document:

- Bookings
- Quotes
- Orders
- Travellers
- Policies
- Tickets
- Support
- Approvals
- Suppliers
- GDS/BSP concepts
- Amadeus/Sabre/Travelport readiness
- UATP/Conferma/AirPlus/PCI Vault readiness
- Manual intervention workflows if present

### APIs

Document:

- Controller groups
- Minimal API endpoints if any
- Route conventions
- Error handling
- Versioning
- Public API vs internal API boundaries
- Webhook support
- DTO usage
- Validation usage
- OpenAPI/Swagger usage
- Authentication/authorization per endpoint if clear

### UI

Document:

- UI framework
- Blazor Server usage if present
- Layouts
- Pages
- Components
- Auth state usage
- API call patterns
- Forms
- Validation
- Navigation
- Styling framework
- JavaScript interop if present

### Infrastructure

Document:

- Docker Compose services
- Dockerfiles
- Reverse proxy configuration
- Caddy configuration
- Maintenance mode behaviour if present
- Environment variables
- Secrets handling
- Local vs production differences
- Hosted database assumptions
- Azure assumptions if present
- GitHub Actions or CI/CD assumptions

### Testing

Document:

- Unit tests
- Integration tests
- E2E tests
- Test framework
- Test coverage by area
- Missing tests
- Current test reliability
- What should be added before v5.1 refactors

### AI Agent Readiness

Document:

- Existing CLAUDE.md files
- Existing AI instructions
- Existing prompts
- Existing agent workflow docs
- Whether the repository is ready for safe Claude Code usage
- Guardrails needed before code generation
- Suggested future `CLAUDE.md` structure
- Suggested agent tasks for v5.1

## Markdown Linting Requirements

The output Markdown should be compatible with standard Markdown linting.

Follow these rules:

- Use only one H1 heading.
- Do not use bold text as fake headings.
- Use proper heading levels.
- Use blank lines around headings, lists, and fenced code blocks.
- Use fenced code blocks with language identifiers where practical.
- Avoid trailing whitespace.
- Avoid duplicate headings where possible.
- Keep line length readable but do not destroy tables for line length alone.
- Avoid bare URLs where possible.

## How to Handle Conflicts

If code and documentation disagree, use this format:

```markdown
### Conflict: <short name>

Current documentation says:

- `<path>`

Current implementation suggests:

- `<path>`

Impact:

- ...

Recommended v5.1 action:

- ...
```

## How to Handle Unknowns

Use this format:

```markdown
### Unknown: <short name>

What is unknown:

- ...

Why it matters:

- ...

Evidence checked:

- `path/to/file`
- `path/to/other-file`

Recommended follow-up:

- ...
```

## How to Handle Refactoring Candidates

Use this format:

```markdown
### Refactoring Candidate: <short name>

Current state:

- ...

Evidence:

- `path/to/file`

Risk:

- ...

Recommended v5.1 action:

- ...

Suggested documentation target:

- `docs/...`
```

## How to Handle ADR Candidates

Use this format:

```markdown
| Proposed ADR | Current Evidence | Reason | Suggested Status |
|---|---|---|---|
| ADR-XXXX-example-title | `path/to/file` | Explain why this needs a decision | proposed |
```

ADR candidates should include areas such as:

- Database choice
- Blazor Server
- BFF authentication
- Markdown linting
- Tenancy model
- Org hierarchy
- Role/claim/permission model
- Billing/licensing model
- Payment provider abstraction
- Stripe webhook handling
- PAT/service account handling
- SSO/SCIM model
- Audit model
- Logging/observability model
- Testing standards
- Deployment model
- Backup/restore model
- AI agent operating model

## Specific Questions to Answer

The output file must answer these questions directly:

1. What is the current shape of Cinturon360 v5?
2. What is implemented vs merely documented?
3. What is the safest way to carry v5 into v5.1?
4. What should be preserved?
5. What should be rewritten?
6. What should be archived?
7. What should become ADRs?
8. What should become architecture notes?
9. What should become domain documentation?
10. What should become runbooks?
11. What should become engineering standards?
12. What should become onboarding material?
13. What is risky to let an AI agent modify without more instructions?
14. What information is missing for a future contractor or developer?
15. What should the first v5.1 documentation tasks be?
16. What should the first v5.1 engineering tasks be?
17. What should the first v5.1 development tasks be?

## Important Constraints

Do not make code changes.

Do not reformat code.

Do not create commits.

Do not delete files.

Do not move files.

Do not install packages unless absolutely required for static inspection.

Do not run destructive commands.

Do not run database migrations.

Do not run Docker Compose unless explicitly instructed later.

Do not assume the application starts correctly.

Do not assume tests pass unless you actually run them.

If you do run commands, use read-only commands where possible.

## Suggested Read-Only Commands

Use commands like these as needed:

```bash
pwd
find . -maxdepth 4 -type f | sort
find . -name "*.md" -type f | sort
find . -name "*.csproj" -type f | sort
find . -name "*.sln" -type f | sort
find . -name "CLAUDE.md" -type f | sort
find . -name "docker-compose*.yml" -o -name "docker-compose*.yaml"
find . -name "Dockerfile*" -type f | sort
find . -name "*.cs" -type f | sort
rg "TargetFramework|PackageReference" -n .
rg "AddAuthentication|AddAuthorization|AddJwtBearer|AddCookie|ClaimsPrincipal|AuthenticationStateProvider" -n .
rg "DbContext|DbSet<|OnModelCreating|HasIndex|HasQueryFilter" -n .
rg "Stripe|Conferma|UATP|AirPlus|PCI|Amadeus|Sabre|Travelport|Xero|Oracle" -n .
rg "Tenant|Org|Organisation|Organization|Vendor|TMC|Client|Sudo" -n .
rg "Role|Claim|Permission|Policy|Authorize" -n .
rg "Invoice|Ledger|Billing|License|Entitlement|Payment|Refund|Credit|Debit" -n .
rg "Booking|Quote|Order|Ticket|Traveller|Traveler|Approval" -n .
rg "TODO|FIXME|HACK|TECHDEBT|temporary|refactor" -n .
```

Use `rg` over `grep` where available.

## Suggested Discovery Sequence

Follow this sequence:

1. Map repository files and folders.
2. Identify solution and project structure.
3. Review project files and dependencies.
4. Review runtime entry points.
5. Review configuration and environment model.
6. Review Docker and deployment files.
7. Review database and EF Core model.
8. Review domain entities and enums.
9. Review API endpoints.
10. Review UI project.
11. Review auth and authorization.
12. Review integrations.
13. Review tests.
14. Review Markdown documentation.
15. Compare implementation against documentation.
16. Identify gaps, risks, and stale decisions.
17. Write the final discovery report.

## Quality Bar

The report must be useful enough that the next Claude Code session can use it as context for v5.1 without rediscovering the whole repository.

The report must help a human understand:

- what exists
- what works
- what is unclear
- what is fragmented
- what is risky
- what needs to be carried forward
- what needs to be refactored
- what documentation should exist in Obsidian
- how future AI agents should operate safely

## Final Instruction

Create the file:

```text
claude-migration-to-v5.1.md
```

At the end of your run, provide a short terminal summary only:

```text
Created claude-migration-to-v5.1.md

Key sections completed:
- ...
- ...
- ...

Important follow-up:
- ...
```

Do not include the full file content in the terminal response unless explicitly asked.
