---
title: Outcomes
description: Meaning of OUT values and how to choose them consistently.
categories: [platform, logging]
tags: [taxonomy, outcomes]
weight: 30
---

## OUT (Outcome)

`OUT` is the normalised result.

| OUT | Meaning | When to use |
|---|---|---|
| `OK` | expected success | operation completed |
| `WARN` | succeeded but degraded | fallback used, partial result |
| `ERR` | operation failed | validation fail, provider error |
| `FAIL` | critical failure | unrecoverable failure, invariants broken |
| `DENY` | blocked by security | permission denied |
| `TIMEOUT` | exceeded time budget | provider timeout, job timeout |
| `RETRY` | retry scheduled/attempted | job retry, transient error |
| `CANCEL` | cancelled | user cancelled action, job cancelled |

### Outcome selection rules

- **Validation failures**: `OUT=ERR` (not `WARN`).
- **Authorization denial**: `OUT=DENY` (always, regardless of level).
- **Provider errors**: `OUT=ERR` unless they are fatal to the system, then `FAIL`.
- If you emit a retry event, use `OUT=RETRY` and include `TRY` (attempt number) in context.
