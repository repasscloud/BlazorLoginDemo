---
title: Collision Handling
description: Collision probability, keyspace sizing, and enforcement strategy for platform identifiers
categories: [platform, identifiers]
tags: [collisions, probability, nanoid]
type: docs
---

## Purpose

Cinturon360 uses **probabilistic identifiers** (NanoID) combined with **deterministic enforcement**
(database uniqueness) to achieve safety without coordination.

This page explains:
- why collisions are theoretically possible
- how likely they are in practice
- how the platform handles them safely

## Alphabet and keyspace

Alphabet used:

```
34679ACDEFGHJKMNPQRTUVWXY
```

Removed characters _(ambiguous in many fonts / screenshots / tickets)_:

```
0 O 1 I L 2 Z 5 S 8 B
```

Alphabet size: **25**

Keyspace size for random length `L`:

```
N = 25^L
```

## Keyspace sizes by length

| Length (L) | Keyspace size N |
|---:|---:|
| 12 | 25¹² ≈ 5.96 × 10¹⁶ |
| 14 | 25¹⁴ ≈ 3.73 × 10¹⁹ |
| 16 | 25¹⁶ ≈ 2.33 × 10²² |
| 18 | 25¹⁸ ≈ 1.46 × 10²⁵ |
| 21 | 25²¹ ≈ 9.31 × 10²⁹ |

## Collision probability (birthday bound)

For small probabilities, collision likelihood can be approximated as:

```
p ≈ n² / (2N)
```

Where:
- `p` = probability of ≥1 collision
- `n` = number of generated IDs in the window
- `N` = keyspace size

## Practical interpretation

A useful rule‑of‑thumb threshold is:

```
n ≈ √(2N)
```

Approximate thresholds:

| Length | √(2N) (order of magnitude) |
|---:|---:|
| 12 | ~3.5 × 10⁸ (hundreds of millions) |
| 14 | ~8.6 × 10⁹ (billions) |
| 16 | ~6.8 × 10¹¹ (hundreds of billions) |
| 18 | ~1.7 × 10¹³ |
| 21 | ~1.4 × 10¹⁵ |

Meaning: collisions only become plausible after generating **enormous volumes** within the *same prefix keyspace*.

## Prefix partitioning effect

Each prefix (`org_`, `job_`, `flt_`, etc.) creates an **independent keyspace**.

This dramatically reduces collision probability in practice, since IDs are not all drawn from one global pool.

## Deterministic enforcement

Where uniqueness matters, Cinturon360 enforces:

```sql
UNIQUE (id)
```

On conflict:
1. generate a new ID
2. retry insert
3. continue

Collisions are therefore **non‑events**, not incidents.

## Audience guidance

### DevTeam
- Never pre‑check for uniqueness
- Rely on unique constraints
- Retry on conflict

### Product Support
- A collision retry is not a fault condition
- No customer action required

### Vendors / TMCs
- Identifiers are implementation details
- Collision handling is transparent

## Summary

- Collisions are theoretically possible but practically negligible
- Length selection is driven by lifecycle and blast radius
- Prefix partitioning further reduces risk
- Unique index + retry guarantees correctness
