---
title: UI_SAVE_PRESSED
description: Emitted when the user presses the Save button on a page.
categories: [platform, logging]
tags: [ui, events]
weight: 10
---

_Last updated: 24 January 2026_

## Event

`EVT=UI_SAVE_PRESSED`

| Field | Value |
|---|---|
| `CAT` | `UI` |
| `ACT` | `UPDATE` |
| `OUT` | `OK` |
| `ENT` | `DomainEntity` |

## When to emit

Emitted when the user presses the Save button on a page.

## Required fields

- `UID`, `ORG`, `TID`, `RID`, `PATH`
- `ENT` and `EntId` for the record being saved (when applicable)

## NOTE format

`note` may include:
- `section={SectionKey}` (if save is scoped to a section)
- `clientValidation={pass|fail}` (only if you have a stable client validation gate)

## Examples

```
EVT=UI_SAVE_PRESSED CAT=UI ACT=UPDATE OUT=OK ENT=Company EntId=xyz RID=… TID=… UID=… ORG=… PATH=/companies/xyz NOTE=section=billing
```

## Guidance

- This event represents user intent only. It is **not** the outcome of the save.
- The API call should emit `API_REQ_END` with its own `OUT`/`STAT` and re-use the same `RID` to correlate UI -> API -> DATA changes.
