# Architecture Decision Records

Generated: 2026-05-10 from `claude-migration-to-v5.1.md` discovery report.

To accept a proposed ADR: review, fill in the **Decision** section, move the file from `proposed/` to `accepted/`, and update its **Status** field.

## Accepted

| ADR | Title |
|---|---|
| [ADR-0001](accepted/ADR-0001-use-postgresql-as-primary-database.md) | Use PostgreSQL as Primary Database |
| [ADR-0002](accepted/ADR-0002-use-blazor-server-no-wasm.md) | Use Blazor Server (no WebAssembly) |
| [ADR-0003](accepted/ADR-0003-use-bff-auth-pattern.md) | Use BFF Auth Pattern |
| [ADR-0004](accepted/ADR-0004-use-markdownlint-cli2.md) | Use markdownlint-cli2 for Markdown Linting |

## Proposed

| ADR | Title | Key question |
|---|---|---|
| [ADR-0005](proposed/ADR-0005-tenant-query-filter.md) | Tenant Query Filter | EF Core global filter vs repository-layer vs RLS? |
| [ADR-0006](proposed/ADR-0006-permission-enforcement-strategy.md) | Permission Enforcement Strategy | Command tagging, endpoint policies, or both? |
| [ADR-0007](proposed/ADR-0007-license-v2-canonical.md) | License v2 as Canonical Billing Model | When to retire legacy billing entities? |
| [ADR-0008](proposed/ADR-0008-job-scheduler-supercronic-vs-ihostedservice.md) | Job Scheduler | Keep IHostedService or restore supercronic? |
| [ADR-0009](proposed/ADR-0009-postgres-version-pin.md) | PostgreSQL Version Pin | Which major version across all environments? |
| [ADR-0010](proposed/ADR-0010-claim-taxonomy-consolidation.md) | Claim Taxonomy Consolidation | Which of the three taxonomies is canonical? |
| [ADR-0011](proposed/ADR-0011-stripe-webhook-handling.md) | Stripe Webhook Handling | Idempotency, signature, and replay strategy |
| [ADR-0012](proposed/ADR-0012-pat-and-service-account-token-format.md) | PAT and Service Account Token Format | Prefix, length, hashing, rotation policy |
| [ADR-0013](proposed/ADR-0013-sso-and-scim-readiness.md) | SSO and SCIM Readiness | First SSO target and SCIM scope |
| [ADR-0014](proposed/ADR-0014-audit-logging-model.md) | Audit Logging Model | EF interceptor vs domain events vs explicit writes |
| [ADR-0015](proposed/ADR-0015-logging-and-observability.md) | Logging and Observability | Structured fields, sinks, Azure Monitor wiring |
| [ADR-0016](proposed/ADR-0016-testing-pyramid.md) | Testing Pyramid | Coverage targets and project responsibilities |
| [ADR-0017](proposed/ADR-0017-deployment-target-aca.md) | Deployment Target — ACA | Bicep modules, CI/CD, environment tiers |
| [ADR-0018](proposed/ADR-0018-backup-and-restore-strategy.md) | Backup and Restore Strategy | RPO/RTO, retention, secret backup |
| [ADR-0019](proposed/ADR-0019-ai-agent-operating-model.md) | AI Agent Operating Model | What agents can do, where, and under what review |
| [ADR-0020](proposed/ADR-0020-tmc-chain-and-branch-modelling.md) | TMC Chain and Branch Modelling | Promote to aggregate or defer? |
| [ADR-0021](proposed/ADR-0021-vendor-search-function-modelling.md) | Vendor Search Function Modelling | Search category entitlement model |
