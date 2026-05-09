# ADR-0009: PostgreSQL Version Pinning per Environment

**Status:** Proposed  
**Date:** 2026-05-10  
**Deciders:** TBD

## Context

There are currently three different PostgreSQL versions specified across three sources in the same repository, all claiming to be authoritative:

| Source | Version |
|---|---|
| `v5-plan.md §13` | `postgres:16` |
| `README.md` line 21 | PostgreSQL 17 |
| `deploy/compose/compose.dev.yaml` | `postgres:18-alpine3.22` |

`SESSION-NOTES-2026-04-27.md` documents a deliberate move to Postgres 18 (the volume mount path changed from `/var/lib/postgresql/data` to `/var/lib/postgresql`). This means the compose file is the most recent authoritative source, but it conflicts with both planning and README.

Impact: a new contributor reading the README will provision Postgres 17, encounter the Postgres 18 volume-path difference, and fail on first `dev-start.sh` run.

No Postgres version is specified in:
- `deploy/azure/` (empty)
- CI matrix (no CI exists)
- Any staging/production compose file (none exists)

## Decision

*Not yet decided.* The decision must specify:
1. A single supported PostgreSQL major version for **development** (the dev compose).
2. A single supported PostgreSQL major version for **production** (Azure Database for PostgreSQL Flexible Server or compose-based prod).
3. Whether dev and production use the same major version, or whether dev trails one behind the production major.
4. A policy for when to upgrade (e.g., "upgrade when the new major reaches GA and Npgsql + EF Core support it").

Recommended baseline: **Postgres 17** for all environments.

Rationale: Postgres 18 is not yet GA as of the discovery date (2026-05-10); using an alpha/RC in development risks hitting engine bugs. Postgres 17 is current stable; Azure Database for PostgreSQL Flexible Server supports it. The volume-path change that prompted the upgrade to 18 (moving from `/var/lib/postgresql/data` to `/var/lib/postgresql`) should be evaluated against the stable Postgres 17 mount path before committing to 18.

*If the team prefers Postgres 18 beta:* document this explicitly and pin to a specific patch image tag (not a floating `:18-alpine` tag).

## Consequences

**Whichever version is chosen:**
- `deploy/compose/compose.dev.yaml` image tag, `README.md`, and `v5-plan.md §13` must be updated to the same value simultaneously.
- The CI matrix (once created) must test against the pinned version.
- Azure Database for PostgreSQL Flexible Server tier must be confirmed to support the chosen major.
- `SESSION-NOTES-2026-04-27.md` volume-path documentation must reflect the authoritative mount path.

**If Postgres 17 is chosen:**
- Revert `compose.dev.yaml` image to `postgres:17-alpine3.22` (or a specific patch).
- The volume mount path reverts to `/var/lib/postgresql/data`.
- Existing developer machines running Postgres 18 will need their volumes recreated.

**If Postgres 18 is chosen (beta):**
- Pin to a specific patch tag.
- Document known limitations and a migration plan to GA once it is released.
- Note that Azure Database for PostgreSQL Flexible Server may not support Postgres 18 until after GA.

**Required follow-up:**
- Update all three sources to a single version simultaneously in one commit.
- Add a `POSTGRES_VERSION` variable at the top of `compose.dev.yaml` so future version bumps are a one-line change.
- Include the agreed version in the CI matrix once CI is created.
