# ADR-0010: Claim Taxonomy Consolidation

**Status:** Proposed  
**Date:** 2026-05-10  
**Deciders:** TBD

## Context

There are currently three coexisting claim and role taxonomies in the codebase that are not aligned with each other:

**Taxonomy 1 — `Cinturon360.Common.Constants.AppConstants`**
Claim type strings: `c360:user_id`, `c360:org_id`, `c360:org_role`, `c360:app_role`, `c360:tenant_id`, `c360:service_account`
Role string constants: `global_admin`, `support`, `finance`, `org_admin`, `approver`, `booker`, `traveller`, `read_only`

**Taxonomy 2 — `Cinturon360.Domain.Enums.Security.{PlatformRole, OrgRole}`**
`PlatformRole` enum: `Sudo`, `PlatformOps`, `PlatformAudit`
`OrgRole` enum: `OrgAdmin`, `Approver`, `Booker`, `Traveller`, `ReadOnly`

**Taxonomy 3 — `Cinturon360.Web.Security.ClaimTypes`**
Web-BFF-specific claims: `UserId`, `OrgId`, `Email`, `DisplayName`, `UserCategory`, `PlatformRole`, `AccessToken`, `RefreshToken`, `TokenExpiry`, `SessionId`, `Theme`, `Language`, `TimeZone`

Misalignments:

- `PlatformRole.Sudo` (enum) vs `global_admin` (string constant) — different names for the same concept.
- `OrgRole.OrgAdmin` (PascalCase enum) vs `org_admin` (snake_case string) — naming convention inconsistency.
- `c360:app_role` (Common) vs `PlatformRole` (Web) — different claim key names for the platform role claim.
- `c360:tenant_id` exists alongside `c360:org_id` — unclear which is canonical for tenancy.

When `AuthorizationBehavior` reads `currentUser.HasPermission(...)` from JWT `perm` claims, and when endpoint policies are checked against claim values, the taxonomy used must be consistent across JWT issuance (`TokenService`), permission checking (`AuthorizationBehavior`), and BFF cookie population (`AuthEndpoints`).

Evidence:

- `src/Cinturon360.Common/Constants/AppConstants.cs:11–46`
- `src/Cinturon360.Domain/Enums/Security/Roles.cs:7–24`
- `src/Cinturon360.Web/Security/ClaimTypes.cs`
- `src/Cinturon360.Infrastructure/Security/TokenService.cs`

## Decision

*Not yet decided.* The decision must:

1. Designate one canonical source of truth for claim type strings (recommended: `Cinturon360.Common.Constants.AppConstants`).
2. Align the domain enum names with the string constants (or vice versa).
3. Consolidate or eliminate the BFF-specific `Web.Security.ClaimTypes` where it duplicates `Common` constants.
4. Clarify whether `c360:tenant_id` and `c360:org_id` are the same (and eliminate one) or different (and document the distinction).

Recommended canonical mapping:

| Concept | Canonical claim key | Canonical enum/constant |
| --- | --- | --- |
| User ID | `c360:user_id` | `AppConstants.ClaimTypes.UserId` |
| Org ID (tenancy) | `c360:org_id` | `AppConstants.ClaimTypes.OrgId` |
| Platform role | `c360:platform_role` | `PlatformRole` enum |
| Org role | `c360:org_role` | `OrgRole` enum |
| Permission code | `perm` | `PermissionCodes` constants |
| Session ID | `c360:session_id` | `AppConstants.ClaimTypes.SessionId` |

The `c360:tenant_id` claim should be removed as a duplicate of `c360:org_id` (or documented as a distinct concept if it represents something other than the user's home org).

## Consequences

**If a canonical taxonomy is adopted:**

- `TokenService.GenerateAccessToken` must emit claims using only the canonical keys.
- `BffAuthStateProvider` and `BffTokenHandler` must read claims using the canonical keys.
- `Cinturon360.Web.Security.ClaimTypes` must either be removed (with references updated to `AppConstants`) or reduced to BFF-only claims not duplicating `AppConstants`.
- Domain enum members must be renamed to match their canonical string representations (or a `[EnumMember(Value = "...")]` attribute added for serialisation).
- Any existing JWT tokens in development sessions will be invalid after the rename; tokens must be re-issued.

**Required follow-up:**

- Audit all usages of `AppConstants.ClaimTypes.*`, `AppConstants.Roles.*`, `PlatformRole`, `OrgRole`, and `Web.Security.ClaimTypes.*` across `src/`.
- Author a single migration commit that renames everything consistently.
- Add an ArchUnitNET test asserting that claim type strings are only defined in `AppConstants`.
