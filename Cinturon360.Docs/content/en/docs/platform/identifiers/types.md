---
title: Types & Usage
description: Entity-specific prefixes, random lengths, lifecycle expectations, and support guidance
categories: [platform, identifiers]
tags: [identifiers, prefixes, lifecycle]
type: docs
---

## Overview

This page defines identifier usage by entity/artefact type, including:

- prefix and random-length selection
- expected lifetime and volume
- whether it is canonical or ephemeral
- support/operational notes

All identifiers use the canonical format:

```
<prefix>_<random>
```

Random lengths shown below are the **random portion** only (the part after the underscore).

## Identifier matrix

| Entity                            | Prefix  | Random length |
| --------------------------------- | ------- | ------------ |
| Organization (Unified)            | `org_`  | **16** |
| Organization Domain               | `dom_`  | 14     |
| ClientLicense                     | `lic_`  | 14     |
| QueuedJob                         | `job_`  | 12     |
| SystemLog                         | `log_`  | 12     |
| Error Code (Unified)              | `err_`  | 14     |
| Exchange Rate Snapshot            | `fxs_`  | 12     |
| Expense Policy                    | `epol_` | 14     |
| Travel Policy                     | `tpol_` | 14     |
| Ephemeral Travel Policy           | `etp_`  | 12     |
| Flight View Option                | `fvo_`  | 12     |
| Flight Offer Search Request DTO   | `fosr_` | 12     |
| Flight Offer Search Result Record | `fos_`  | 12     |
| Travel Quote User                 | `tqu_`  | 12     |

## Detailed notes per type

### Organization (Unified) — `org_` (16)
**Canonical identity** for a single organization. One per org, referenced broadly across the platform.

- **Lifetime:** long-lived (do not reuse)
- **Volume:** low (relative)
- **Reason for length:** high fan-out + high blast radius if ever collided
- **Operational note:** treat as UUID-equivalent in terms of stability

### Organization Domain — `dom_` (14)
Represents a verified domain reference (e.g. `google.com.au`) associated with an organization.

- **Lifetime:** medium/long (domains can be added/removed)
- **Volume:** low-to-medium per org
- **Operational note:** domain string may change; ID remains stable for the record instance

### ClientLicense — `lic_` (14)
Represents license/entitlement state for an org/tenant relationship.

- **Lifetime:** long-lived and auditable
- **Volume:** low-to-medium
- **Operational note:** license state can change without changing the ID

### QueuedJob — `job_` (12)
Represents queued background work. Jobs may expire and be deleted.

- **Lifetime:** short-lived
- **Volume:** high
- **Reason for length:** large enough keyspace; low blast radius; short retention

### SystemLog — `log_` (12)
Represents structured log record identifiers (where logs are stored as entities).

- **Lifetime:** retention-defined
- **Volume:** very high
- **Operational note:** logs are diagnostic artefacts; collisions are handled by uniqueness where required

### Error Code (Unified) — `err_` (14)
Primary key for error-definition records.

- **Lifetime:** long-lived (catalog evolves over time)
- **Volume:** low-to-medium
- **Important:** multiple error records may share the same human-readable `ErrorCode` string; the ID is the stable identity

### Exchange Rate Snapshot — `fxs_` (12)
Point-in-time FX capture used for pricing/audit scenarios.

- **Lifetime:** retention-defined (often expires)
- **Volume:** medium-to-high
- **Operational note:** do not assume one snapshot per day; the model supports multiple snapshots per period

### Expense Policy — `epol_` (14)
Canonical policy identity used for expense rules.

- **Lifetime:** long-lived (policy versions may be handled separately)
- **Volume:** medium
- **Operational note:** assigned to orgs/users; referenced by expense workflows

### Travel Policy — `tpol_` (14)
Canonical policy identity used for travel rules and enforcement.

- **Lifetime:** long-lived
- **Volume:** medium
- **Operational note:** referenced by bookings/quotes/audits; do not embed meaning in the ID

### Ephemeral Travel Policy — `etp_` (12)
Derived/temporary policy artefact (subset of a travel policy) generated for a specific scenario.

- **Lifetime:** short-lived
- **Volume:** medium
- **Operational note:** safe to delete/regenerate; not considered canonical

### Flight View Option — `fvo_` (12)
UI/presentation artefact controlling flight display or selection options.

- **Lifetime:** short-lived to medium (feature-dependent)
- **Volume:** medium-to-high

### Flight Offer Search Request DTO — `fosr_` (12)
Correlation ID for flight offer searches at the request boundary.

- **Lifetime:** short-lived
- **Volume:** high
- **Operational note:** DTO correlation only; not a durable business entity

### Flight Offer Search Result Record — `fos_` (12)
Materialized record for search results (storage/caching).

- **Lifetime:** short-lived to medium
- **Volume:** high
- **Operational note:** regenerated frequently; treat as ephemeral business support artefact

### Travel Quote User — `tqu_` (12)
Represents user association to a travel quote.

- **Lifetime:** quote-lifetime
- **Volume:** high for active quoting
- **Operational note:** removed with quote expiry/cleanup

## Cross-links

- Travel quote booking-type prefixes are documented separately on
  [Travel Quote Prefixes](./travel-quote-prefixes/).
- Probability and collision-handling details are documented on
  [Identifier Generation & Collision Handling](./generation-and-collision/).
