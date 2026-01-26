---
title: Identifier Types & Usage
description: Entity-specific identifier prefixes, lengths, and lifecycle characteristics
categories: [platform, identifiers]
---

## Overview

This page describes how identifiers are used across different entity types, including
their intended lifetime, cardinality, and collision risk profile.

All identifiers use the same underlying generation mechanism but differ in **prefix**
and **random length**.

---

## Identifier Matrix

| Entity                            | Prefix  | Length |
| --------------------------------- | ------- | ------ |
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

---

## Entity Notes

### Organization (Unified) — `org_`
Canonical, long-lived identifier representing a single organization across the platform.
Highly referenced and never reused.

### Organization Domain — `dom_`
Represents a verified domain (e.g. `example.com`) associated with an organization.
Domains may be added or removed over time.

### ClientLicense — `lic_`
Represents contractual or entitlement state. Long-lived and auditable.

### QueuedJob — `job_`
Operational job identifiers for background or deferred processing.
High volume and short-lived.

### SystemLog — `log_`
Identifiers for structured system log records.
Write-heavy and typically retained for limited periods.

### Error Code (Unified) — `err_`
Stable identifier for error definitions.
Multiple records may share the same human-readable error code string.

### Exchange Rate Snapshot — `fxs_`
Point-in-time capture of FX rates.
Typically time-bucketed and expirable.

### Expense Policy — `epol_`
Defines rules for expense handling.
Assigned to organizations or users and referenced by transactions.

### Travel Policy — `tpol_`
Defines travel rules and constraints.
Often referenced by bookings, audits, and enforcement logic.

### Ephemeral Travel Policy — `etp_`
Derived, short-lived policy generated for specific scenarios.
Not considered canonical.

### Flight View Option — `fvo_`
Represents UI-level or presentation-specific flight configuration.

### Flight Offer Search Request DTO — `fosr_`
Transport-layer identifier used for tracing and correlation of search requests.

### Flight Offer Search Result Record — `fos_`
Materialized search result record.
Typically regenerated and scoped to a search lifecycle.

### Travel Quote User — `tqu_`
Represents a user associated with a specific travel quote.
Deleted alongside the quote lifecycle.
