# ADR-0015: Logging and Observability — Structured Fields, Sinks, and Azure Monitor Wiring

**Status:** Proposed  
**Date:** 2026-05-10  
**Deciders:** TBD

## Context

The current logging posture:

- Serilog 4.2.0 is configured in `Cinturon360.Api` with a Console sink and a custom template: `[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}`.
- `Serilog.Sinks.PostgreSQL.Alternative` is in `Directory.Packages.props` but not wired (decision in `QUESTIONS.md Q13`: stdout only in prod).
- `Serilog.Enrichers.{Environment,Thread,CorrelationId}` are referenced; correlation is via `Enrich.FromLogContext()` only — no middleware sets `X-Correlation-Id`.
- `Cinturon360.Infrastructure/Logging/{Sinks,Enrichers,Formatters,Correlation}/` are empty.
- `Cinturon360.Application/SysLog/{Models,Writers,Enrichers,Events}/` are empty.
- No health-check endpoint (`/health` not registered).
- No metrics (no Prometheus, OpenTelemetry, Application Insights).
- No Azure Monitor integration.

The platform will run on Azure Container Apps (per `QUESTIONS.md Q9` and ADR-0017). Azure Container Apps routes stdout to Azure Monitor Logs (Log Analytics) automatically. This means structured JSON logs written to stdout are queryable in Azure Monitor without a Serilog sink.

## Decision

*Not yet decided.* The decision must lock:

1. **Log format:** JSON structured (recommended for Azure Monitor queryability) vs human-readable console (current).
2. **Mandatory structured fields** on every log entry.
3. **Sinks per environment:**
   - Development: human-readable console (current) is acceptable.
   - Production: JSON stdout → Azure Monitor via ACA log routing.
4. **Correlation ID:** How `X-Correlation-Id` is set and propagated.
5. **Health check endpoint:** Whether to add `MapHealthChecks` and what checks to include.
6. **Metrics:** Whether to add OpenTelemetry metrics and which Azure Monitor workspace to target.

Recommended decisions:

**Log format:** Serilog JSON formatter (`Serilog.Formatting.Compact.CompactJsonFormatter`) for production; keep human-readable console for development.

**Mandatory structured fields:**

| Field | Source |
| --- | --- |
| `Timestamp` | Serilog default |
| `Level` | Serilog default |
| `Message` | Serilog default |
| `CorrelationId` | `X-Correlation-Id` header or generated per-request |
| `RequestId` | ASP.NET Core `TraceIdentifier` |
| `UserId` | JWT `c360:user_id` claim (enricher) |
| `OrgId` | JWT `c360:org_id` claim (enricher) |
| `Application` | Static value `cinturon360-api` |
| `Environment` | `ASPNETCORE_ENVIRONMENT` |

**Sinks:**

- Development: Console (human-readable).
- Production: Console with `CompactJsonFormatter` (Azure Monitor picks up stdout).
- Remove `Serilog.Sinks.PostgreSQL.Alternative` from `Directory.Packages.props` if it will never be used.

**Correlation ID:** Add `CorrelationIdMiddleware` that reads `X-Correlation-Id` from the inbound request (or generates a `TraceId`-based value if absent) and pushes it to `LogContext.PushProperty("CorrelationId", ...)`.

**Health checks:** Add `MapHealthChecks("/health")` with a Postgres liveness check (`AddNpgsql(...)`) and a readiness check.

**Metrics:** Defer full OpenTelemetry metrics to a later phase; add `app.UseRequestLogging()` (already referenced) as the baseline.

## Consequences

**If these decisions are accepted:**

- Implement `CorrelationIdMiddleware` in `Cinturon360.Api/Infrastructure/`.
- Add a Serilog user/org enricher that reads the JWT claims.
- Switch `appsettings.Production.json` to use `CompactJsonFormatter`.
- Register `MapHealthChecks("/health/live")` and `MapHealthChecks("/health/ready")` in `Program.cs`.
- Add `Aspire.Npgsql` or `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` for DB health.
- Remove the unused `Serilog.Sinks.PostgreSQL.Alternative` package reference.
- Populate `Cinturon360.Infrastructure/Logging/` with the enricher and middleware.
- Populate `Cinturon360.Application/SysLog/` with application-level event models (distinct from entity audit events in ADR-0014).

**Required follow-up:**

- Define Azure Monitor workspace name and Log Analytics workspace ID for production.
- Wire OpenTelemetry traces in a future ADR (once ACA deployment is defined in ADR-0017).
