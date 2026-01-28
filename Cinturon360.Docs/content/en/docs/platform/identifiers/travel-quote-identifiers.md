---
title: Travel Quote Identifiers
description: Structure, prefixes, lifecycle, and collision considerations for Travel Quote identifiers
categories: [platform, identifiers, quotes]
tags: [quotes, identifiers, prefixes, collisions]
type: docs
---

## Purpose

Travel Quote identifiers uniquely identify **pre‑PNR pricing artefacts** generated during search and pricing flows.
They are **short‑lived**, **high‑volume**, and may be regenerated or deleted after expiry.

This document consolidates:
- Quote ID structure and format
- Booking‑type prefixes
- Length selection
- Lifecycle and retention expectations
- Collision probability and enforcement model

It is intended for **DevTeam**, **Product Support**, **Vendors**, and **TMCs**.

## Canonical format

All Travel Quote identifiers follow:

```
<prefix>_<random>
```

- `prefix` identifies the **booking type**
- `_` is a hard delimiter
- `random` is a NanoID value with no embedded meaning

Random portion length for Travel Quote IDs: **12 characters**.

Example:
```
flt_Q7M9K4A3XWDT
```

## Booking‑type prefixes

| Booking type | Prefix |
|------------|--------|
| Flight | `flt_` |
| Accommodation | `acc_` |
| Taxi | `txi_` |
| Train | `trn_` |
| Hire Car | `car_` |
| Bus | `bus_` |
| eSIM | `sim_` |
| Holiday Activity | `act_` |
| Mixed Booking | `mix_` |

## Lifecycle characteristics

- **Creation:** during search / pricing
- **Mutation:** quotes may be repriced or refreshed
- **Retention:** short‑lived (quotes expire automatically)
- **Deletion:** safe to delete after expiry or replacement

Travel Quote IDs are **not canonical business identifiers** and must not be used for:
- settlement
- ticketing
- reconciliation
- financial reporting

## Operational guidance

### Developers
- Treat Quote IDs as **opaque**
- Prefix may be parsed for routing, diagnostics, and telemetry
- Never infer booking state or pricing guarantees from the ID itself

### Product Support
- Prefix enables fast triage (flight vs taxi vs mixed)
- Quote IDs may no longer exist if expired or regenerated

### Vendors / TMCs
- Quote IDs are reference tokens only
- Expiry or regeneration does not indicate an error condition

## Collision model

Travel Quote IDs use **probabilistic uniqueness** (NanoID) combined with **deterministic enforcement**.

### Alphabet

```
34679ACDEFGHJKMNPQRTUVWXY
```

Alphabet size: **25**

Ambiguous characters removed:
```
0 O 1 I L 2 Z 5 S 8 B
```

### Keyspace size

For random length `L`:

```
N = 25^L
```

For Travel Quotes (`L = 12`):

```
N ≈ 5.96 × 10^16
```

### Collision probability (birthday bound)

For small probabilities:

```
p ≈ n² / (2N)
```

Where:
- `p` = probability of ≥1 collision
- `n` = number of quote IDs generated within the same prefix window
- `N` = keyspace size

A useful rule of thumb:

```
n ≈ √(2N)
```

For Travel Quote IDs:

- √(2N) ≈ **3.5 × 10⁸**

Meaning: hundreds of millions of quote IDs would need to be generated **within the same booking‑type prefix**
before collisions become plausible.

## Deterministic enforcement

Where uniqueness matters, the platform enforces:

```sql
UNIQUE (quote_id)
```

On conflict:
1. regenerate a new ID
2. retry insert
3. continue

Collisions are therefore treated as **non‑events**, not incidents.

## Summary

- Travel Quote IDs are short‑lived, high‑volume reference identifiers
- Booking‑type prefixes make IDs self‑describing
- A 12‑character NanoID provides an enormous keyspace
- Prefix partitioning further reduces collision risk
- Unique index + retry guarantees correctness
