---
title: Quote ID Generation & Collision Handling
description: How quote identifiers are generated, structured, and protected against collisions
categories: [platform, identifiers]
tags: [platform, identifiers]
---

## Overview

Cinturon360 generates **Quote IDs** to uniquely identify pricing artefacts during the quoting lifecycle (search, pricing, repricing, expiry).

Quote IDs are **short-lived identifiers**:
- They are **not permanent records**
- They are retained for a limited window (currently **14 days**)
- They are safe to delete, regenerate, and retry

Despite being ephemeral, Quote IDs must remain:
- collision-safe
- human-readable in logs
- URL, database, and export friendly
- cheap to generate at scale

This document explains the structure, entropy model, collision maths, and enforcement strategy.

## Quote ID Structure

All Quote IDs follow this format:

```
<prefix>_<random>
```

### Example
```
flt_Q7M9K4A3XWDT
mix_9RDK7QMW4A7P
```

### Components

| Part | Description |
|----|-----------|
| `prefix` | Encodes the **quote type** (semantic meaning) |
| `_` | Hard separator for parsing and log readability |
| `random` | High-entropy NanoID (entropy only, no meaning) |

## Quote Type Prefixes

| Booking Type | Prefix |
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

## NanoID Alphabet

```
34679ACDEFGHJKMNPQRTUVWXY
```

Ambiguous characters removed:

```
0 O 1 I L 2 Z 5 S 8 B
```

Alphabet size: **25 characters**

## Length & Entropy

Typical format:

```
<prefix>_<12-character NanoID>
```

Total combinations:

```
25¹² ≈ 5.96 × 10¹⁶
```

## Collision Maths (Birthday Bound)

Approximation:

```
p ≈ n² / (2N)
```

| IDs per prefix (14 days) | Collision Probability |
|--------------------------|-----------------------|
| 10 million | ~0.08% |
| 100 million | ~8% |
| ~345 million | ~50% |

## Deterministic Safety

A **unique index** on `quoteId` guarantees safety.

```sql
CREATE UNIQUE INDEX ux_quotes_quoteid
ON quotes (quote_id);
```

On conflict:
- regenerate ID
- retry insert

Collisions become **non-events**.

## Design Rules

- Prefix encodes meaning
- NanoID encodes entropy only
- Never embed environment, region, or vendor in the ID

## Audience Notes

**Developers**
- Treat Quote IDs as opaque
- Parse only the prefix if required

**Vendors / TMCs / Clients**
- Quote IDs are transient references
- They are not bookings or financial artefacts

## Summary

Quote IDs are short-lived, prefix-typed, high-entropy identifiers with deterministic collision handling. This design is safe, scalable, and operationally simple.
