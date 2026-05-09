# ADR-0014: Audit Logging Model

**Status:** Proposed  
**Date:** 2026-05-10  
**Deciders:** TBD

## Context

`UserAuditEvent` exists as a domain entity and `DbSet` in `AppDbContext`, but no code writes to it. The `src/Cinturon360.Data/Interceptors/` directory is empty (only `.gitkeep`). The `src/Cinturon360.Application/SysLog/{Models,Writers,Enrichers,Events}/` directories are also empty.

`BACKLOG.md` notes the following as missing:
- Audit log export
- Impersonation audit
- Audit log archival

The intended design (per `v5-plan.md` and architecture references) includes an EF Core interceptor that automatically emits `UserAuditEvent` rows when domain mutations occur. This is a compliance requirement for enterprise travel management platforms (expense policy enforcement, booking approval chains, user access changes).

Current state:
- `UserAuditEvent` entity has the schema but nothing writes to it.
- Domain mutations (`User.Create`, `Booking.Create`, role assignments, etc.) are not instrumented.
- No audit log page in the Sudo dashboard surfaces these events (the `Sudo/Audit/AuditLog.razor` page exists as a stub).

Options:

**Option A — EF Core SaveChanges interceptor:** A `SaveChangesInterceptor` inspects the `ChangeTracker` on every `SaveChangesAsync` call and writes `UserAuditEvent` rows for tracked changes on auditable entities. The current user ID is injected via `ICurrentUser`.

**Option B — Domain event dispatch:** Domain entities raise events (`UserCreatedEvent`, `BookingCreatedEvent`); `MediatR` `INotificationHandler` instances write audit rows. More granular control; requires domain event infrastructure.

**Option C — Application-layer explicit audit writes:** Each command handler explicitly writes an `IAuditWriter.WriteAsync(...)` call. Most explicit; most verbose.

## Decision

*Not yet decided.* The recommended approach is **Option A** (EF Core interceptor) because:
- It is automatic — new auditable entities are covered without updating every handler.
- It captures the `Before` and `After` state of changed properties.
- It does not require domain event infrastructure (which is also empty at this time).
- It can be implemented as a single `AuditInterceptor : SaveChangesInterceptor` class.

Additionally, the following questions must be answered:

1. **Which entities are auditable?** All entities derived from `SoftDeletableEntity`? All `Entity` subclasses? Or an explicit opt-in via an `IAuditableEntity` marker?
2. **What is captured?** Changed properties (before/after), entity type, entity ID, user ID, org ID, timestamp, operation type (Create/Update/Delete/SoftDelete).
3. **Where is audit data stored?** Same `AppDbContext` (simpler, but could fill the main DB). A separate audit DB or schema. An append-only log stream.
4. **Retention policy?** BACKLOG notes audit log archival is required. Define a retention period (e.g., 7 years for financial records, 1 year for access logs) and an archival mechanism.

## Consequences

**If Option A is chosen:**
- Implement `AuditInterceptor : SaveChangesInterceptor` in `src/Cinturon360.Data/Interceptors/`.
- Register the interceptor in `AppDbContext` configuration.
- `ICurrentUser` must be available in the interceptor (inject via DI factory).
- Define `IAuditableEntity` marker or use the `Entity` base class as the marker.
- `UserAuditEvent` may need new fields: `EntityType`, `EntityId`, `Operation`, `ChangedProperties` (JSON), `UserId`, `OrgId`.
- Background jobs that mutate data must populate `ICurrentUser` with a system user identity to avoid null audit entries.

**Retention:**
- Add a cleanup job to `Cinturon360.Jobs` that archives or deletes `UserAuditEvent` rows older than the retention threshold.
- Consider a separate append-only audit schema or a partitioned table for large volumes.

**Required follow-up:**
- Define `IAuditableEntity` marker.
- Implement and register `AuditInterceptor`.
- Define retention policy.
- Populate `Sudo/Audit/AuditLog.razor` to surface events.
- Add `SysLog` models for structured application-level events (separate from entity audit).
