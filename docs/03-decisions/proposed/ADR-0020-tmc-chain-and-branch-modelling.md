# ADR-0020: TMC Chain and Branch Modelling

**Status:** Proposed  
**Date:** 2026-05-10  
**Deciders:** TBD

## Context

The `Organisation` aggregate currently carries three fields for Travel Management Company (TMC) chain/branch identity:

- `ChainCode` (string, nullable)
- `ChainName` (string, nullable)
- `BranchCode` (string, nullable)

These fields appear on `src/Cinturon360.Domain/Entities/Organization/Organisation.cs` and are referenced in `notes.txt` as a requirement to support Flight Centre-style group codes, where a TMC group (chain) contains multiple branch offices, each with a distinct code used in GDS booking references (IATA, Sabre, Amadeus, etc.).

The current model has limitations:

- A chain is not a first-class aggregate — it has no identity, no members list, no admin, and no independent lifecycle.
- A branch is not a first-class aggregate — `BranchCode` is a string on `Organisation` with no validation, no uniqueness constraint, and no relationship to its chain.
- Multiple `Organisation` records within the same chain are not linked except by sharing the same `ChainCode` string.
- Chain-level operations (add a branch, set chain-wide defaults, assign a chain admin) have no model.

The commercial hierarchy is Vendor → TMC → Client. Within the TMC tier, a large TMC like Flight Centre operates many branches. Some GDS booking workflows require both the chain code and the branch code. The current string-field approach cannot enforce this.

Evidence:

- `src/Cinturon360.Domain/Entities/Organization/Organisation.cs:1–152`
- `notes.txt` — mentions chain/branch code requirement
- `v5-plan.md §5` — org hierarchy

## Decision

*Not yet decided.* The decision must specify whether to:

**Option A — Promote `OrgChain` to a first-class aggregate:**

- New entity: `OrgChain { Id, ChainCode, ChainName, OwnerOrgId (Vendor or Platform), CreatedAt }`
- New join: `OrgChainMembership { ChainId, OrgId (TMC), BranchCode, IsActive }`
- `Organisation` loses `ChainCode`, `ChainName`, `BranchCode`.
- A TMC org can belong to at most one chain.
- Chain admin actions are scoped to the `OrgChain` aggregate.

**Option B — Keep string fields on `Organisation`, add a uniqueness constraint:**

- Add a unique index on `(ChainCode, BranchCode)` where both are non-null.
- Add a `ChainParentOrgId` foreign key to allow linking branch orgs to a chain-owning org.
- Low migration cost; limited structural expressiveness.

**Option C — Defer until a GDS integration requires it:**

- Leave the current string fields in place.
- Document the gap.
- Pick Option A when the first GDS integration (Duffel, Amadeus, etc.) is built and the chain/branch code is required in booking requests.

## Consequences

**If Option A is chosen:**

- New migration: create `org_chains` and `org_chain_memberships` tables.
- New migration: remove `chain_code`, `chain_name`, `branch_code` columns from `organisations`.
- New domain entities: `OrgChain`, `OrgChainMembership`.
- New commands: `CreateOrgChainCommand`, `AddBranchToChainCommand`, `RemoveBranchFromChainCommand`.
- Admin UI: chain management pages (Vendor dashboard).
- Search and booking workflows that require a `BranchCode` can look it up via `OrgChainMembership`.

**If Option C is chosen (defer):**

- No schema change now.
- Document that `ChainCode` + `BranchCode` on `Organisation` are placeholder fields until the GDS integration is implemented.
- Add a unique index `(org_type = 'Tmc' AND chain_code IS NOT NULL AND branch_code IS NOT NULL)` as a minimal guard.

**Recommended decision:** Option C (defer) for v5.1, unless a GDS integration milestone is scheduled for v5.1. Promote to Option A when the first GDS booking workflow requires chain/branch resolution.

**Required follow-up:**

- Add a unique index on `(chain_code, branch_code)` as a minimal guard regardless of which option is chosen.
- Capture the chain/branch requirement in `docs/04-domain/orgs/tmc-chain-codes.md`.
- Revisit when the first GDS integration (Amadeus or Duffel) is scoped.
