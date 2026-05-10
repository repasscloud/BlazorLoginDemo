# ADR-0005: Enforce Tenant Isolation via EF Core Global Query Filter

**Status:** Proposed  
**Date:** 2026-05-10  
**Deciders:** TBD

## Context

Cinturon360 is a multi-tenant platform. The tenancy unit is `Organisation`; a user's `OrgId` determines which data they can see. The intended access model (`v5-plan.md §5`) states that access never flows upward or sideways in the Vendor → TMC → Client hierarchy.

The current implementation has a critical gap: there is no `HasQueryFilter` on any entity in `src/Cinturon360.Data/Configurations/`. Every repository (`IUserRepository`, `IBookingRepository`, etc.) relies on the caller passing the correct `orgId` parameter. If a caller omits or mis-passes the `orgId`, cross-tenant data will be returned without any persistence-layer safeguard.

Evidence:

- Absence of `HasQueryFilter` in `src/Cinturon360.Data/Configurations/` (confirmed by discovery inspection)
- `src/Cinturon360.Domain/Entities/Organization/Organisation.cs:1–152` — `ParentOrgId`, `OrgType`, hierarchy model present but not enforced in queries
- `v5-plan.md §5` — "access never flows upward or sideways"

Options considered:

**Option A — EF Core global query filter (`HasQueryFilter`):** Add an `ITenantContext` service (scoped, populated from the current user's JWT `c360:org_id` claim) and register a global query filter on all tenant-scoped entities. EF Core automatically appends a `WHERE org_id = @current` predicate to every query. Platform "sudo" users bypass the filter.

**Option B — Repository-layer enforcement:** Keep `orgId` as a repository parameter, add an ArchUnitNET test that all repository methods accept and apply an org filter. Less automatic but explicit.

**Option C — Row-level security (Postgres RLS):** Push enforcement to the database using `SET LOCAL app.current_org_id` and a Postgres policy. Most secure; hardest to implement with EF Core.

## Decision

*Not yet decided.* The recommended option is **A** (EF Core global query filter + `ITenantContext`) because:

- It is automatic — new entities added to the DbContext are covered without remembering to add a parameter.
- It is testable — ArchUnitNET can verify the filter is present on tenant entities.
- It handles the sudo bypass case cleanly via `ITenantContext.IsSudo`.

## Consequences

**If Option A is chosen:**

- All tenant-scoped entities must be registered with `HasQueryFilter(e => tenantContext.OrgId == null || e.OrgId == tenantContext.OrgId)`.
- `ITenantContext` must be a scoped service populated from the JWT `c360:org_id` claim via middleware.
- Sudo (platform admin) users have `IsSudo = true`; the filter evaluates to `true` unconditionally for them.
- `IgnoreQueryFilters()` must be used explicitly in cross-tenant admin queries — callers must opt out deliberately.
- Existing repository methods that accept `orgId` can be simplified; some may be removable.

**Risks:**

- If `ITenantContext` is not populated before a query runs (e.g., in a background job), the filter may silently return empty results or incorrectly return all rows.
- Jobs (`Cinturon360.Jobs`) run without an HTTP context; a job-specific `ITenantContext` implementation that returns `IsSudo = true` (or no filter) is required.

**Required follow-up:**

- Define `ITenantContext` interface and `HttpTenantContext` implementation.
- Register the query filter on a representative set of entities and write an ArchUnitNET test.
- Define the job-context implementation.
- Update repositories to remove redundant `orgId` parameters where safe.
