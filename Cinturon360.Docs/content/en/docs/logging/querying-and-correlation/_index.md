---
title: Querying and Correlation
description: How to search logs and reconstruct flows using RID, tenant, org, and event families.
categories: [platform, logging]
tags: [operations, querying]
weight: 100
---

_Last updated: 24 January 2026_

## Primary search keys

When searching logs, use the following order of operations:

1. `RID` — fastest path to reconstruct a complete flow
2. `EVT` — find all occurrences of a specific event
3. `ORG` / `TID` — tenant partition filters
4. `EntId` — entity-level tracing (e.g. all events for a quote)
5. `UID` — actor tracing

## Reconstructing a user action

Common flow:
1. `UI_*` event (intent)
2. `API_*` request end (result)
3. `DATA_*` or domain mutation (actual write)
4. `AUTO_*` / `WF_*` / `INT_*` (async work and provider calls)

The correlation id (`RID`) must tie these together.

## Example: tracing a Save

Search by `RID`, then read chronological order:

- `UI_SAVE_PRESSED`
- `API_REQ_END`
- `DATA_UPDATE`
- `AUTO_JOB_ENQUEUED` (if applicable)
- `INT_CALL_END` (if applicable)
