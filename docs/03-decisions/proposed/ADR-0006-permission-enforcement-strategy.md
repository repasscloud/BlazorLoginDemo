# ADR-0006: Permission Enforcement Strategy

**Status:** Proposed  
**Date:** 2026-05-10  
**Deciders:** TBD

## Context

Cinturon360 has a permission model (`PermissionCodes.cs`, ~30 codes) and a role/permission store (`RolePermission`). The JWT carries one `perm` claim per permission code. An `AuthorizationBehavior<TRequest,TResponse>` MediatR pipeline behaviour checks `currentUser.HasPermission(...)` for any request implementing `IRequirePermission`.

The gap: **zero** commands or queries implement `IRequirePermission`. Every API endpoint uses bare `RequireAuthorization()` with no policy, role, or permission argument. The entire security skeleton at `src/Cinturon360.Application/Security/{AppClaims,AppPolicies,AppRoles,OrgRoles,OrgRoleRequirements,PermissionEvaluation,TokenIssuance}` is empty.

Three approaches are available:

**Option A — MediatR command tagging only:** Mark every command/query with `IRequirePermission` (specifying the required `PermissionCode`). The existing `AuthorizationBehavior` checks it. Endpoints keep bare `RequireAuthorization()` (just ensures authenticated).

**Option B — ASP.NET Core endpoint policies only:** Register named policies in `AddAuthorization(...)` per permission code. Endpoints use `RequireAuthorization("bookings.read")`. No `IRequirePermission` tagging needed on commands.

**Option C — Both (defence-in-depth):** Endpoint policy checks at the HTTP layer (fast, returns 403 before MediatR pipeline starts); command tagging as a second layer for commands dispatched from non-HTTP contexts (jobs, event handlers).

Evidence:

- `src/Cinturon360.Application/Behaviors/Authorization/AuthorizationBehavior.cs`
- `src/Cinturon360.Application/Behaviors/Authorization/IRequirePermission.cs`
- `src/Cinturon360.Common/Constants/AppConstants.cs:21–30` — `PolicyNames` constants already defined
- Grep for `IRequirePermission` in `src/` returns only the two definition files

## Decision

*Not yet decided.* The recommended option is **C** (both layers) because:

- Endpoint policies provide a fast rejection path at the HTTP boundary.
- Command tagging ensures permissions are enforced when commands are dispatched from background jobs or integration events.
- The `PolicyNames` constants (`RequireAuthenticated`, `RequireSudo`, etc.) are already defined and simply need registering.

## Consequences

**If Option C is chosen:**

- `AddAuthorization(opts => { opts.AddPolicy("bookings.read", p => p.RequireClaim("perm", "bookings.read")); ... })` must be registered for every permission code.
- Every authoritative command/query must implement `IRequirePermission` with its required `PermissionCode`.
- Read-only queries that require authentication but no specific permission may implement `IRequireAuthenticated` (a weaker marker) or nothing (relying on endpoint policy alone).
- ArchUnitNET tests should assert that every command in `Cinturon360.Application.Features` either has `IRequirePermission` or is explicitly exempt.
- `Cinturon360.Application/Security/PermissionEvaluation/` should house the org-scope resolver (given user + target `OrgId`, returns allowed?).

**Risks:**

- Registering ~30 policies by hand is error-prone; a loop over `PermissionCodes` constants is safer.
- Org-scope traversal (`ScopeMode.Self` vs `ScopeMode.SelfAndDescendants`) must be evaluated at claim-generation time (in `TokenService`) not re-evaluated per-request, or the scope must be embedded in the JWT.

**Required follow-up:**

- Populate `Cinturon360.Application/Security/` skeleton.
- Register all policies in a loop.
- Tag every command/query with `IRequirePermission`.
- Write ArchUnitNET test asserting coverage.
- Implement org-scope resolver service.
