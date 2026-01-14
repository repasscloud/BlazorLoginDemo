---
title: Schema and Fields
description: Exact meanings of each log column and how to interpret them.
categories: [platform, logging]
tags: [schema, reference]
weight: 10
---

## Field naming rules

Field names are short by design and intentionally stable.

- `EVT` is the **stable event code** (primary selector).
- `CAT`, `ACT`, `OUT` are enums (taxonomy).
- Identifiers (`RID`, `TID`, `ORG`, `UID`, `EntId`) must be in fields, not buried in free text.
- `NOTE` is for short hints only; it must never be the only place information exists.

## Minimal required fields

Every log entry **must** populate:

| Field | Why it is required |
|---|---|
| `Timestamp` | ordering and auditing |
| `Level` | severity filtering |
| `EVT` | durable event selector |
| `CAT` | top-level grouping |
| `ACT` | verb describing the event |
| `OUT` | normalised result |
| `RID` | correlation across services |
| `TID` | tenant partitioning |
| `ORG` | organisation partitioning |
| `UID` | actor attribution (when available) |

If a user is not authenticated (public endpoints), `UID` may be empty/null, but `RID`, `TID`, and `ORG` must still be set wherever technically possible.

## Canonical fields

The platform treats the following as canonical fields (whether stored in DB columns, structured logs, or both):

| Field | Type | Example | Meaning |
|---|---:|---|---|
| `Timestamp` | UTC | `2026-01-14T12:31:08Z` | when the log was recorded |
| `Level` | enum | `Information` | severity |
| `EVT` | string | `UI_PAGE_OPEN` | stable event code |
| `CAT` | enum | `UI` | category (where it happened) |
| `ACT` | enum | `VIEW` | action verb |
| `OUT` | enum | `OK` | normalised outcome |
| `ENT` | string | `Company` | domain entity type |
| `EntId` | string | `xyz` | entity identifier |
| `RID` | string | `0f1...` | correlation id |
| `TID` | string | `EvoTss:acme` | tenant id |
| `ORG` | string | `28a8c...` | organisation id |
| `UID` | string | `auth0\|u_123` | user id |
| `DUR` / `DurMs` | int | `84` | duration in milliseconds |
| `HTTP` | string | `POST` | HTTP method (API) |
| `STAT` | int | `201` | HTTP status (API) |
| `PATH` | string | `/api/quotes` | request route or UI route |
| `NOTE` | string | `errorCode=validation_failed` | short hint for humans |

### Notes on IDs

- `RID` correlates **a single user intent** across multiple services/logs.
- `TID` identifies the **tenant partition**.
- `ORG` identifies the **organisation partition** (may be same as tenant in some deployments, but still stored distinctly).
- `UID` identifies the actor. Use stable identity provider subject values (not display names).
