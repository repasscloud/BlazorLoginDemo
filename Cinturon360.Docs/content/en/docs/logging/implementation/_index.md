---
title: Implementation
description: How to emit logs correctly in C# and how the platform logger maps fields into stored log entries.
categories: [platform, logging]
tags: [logging]
weight: 110
---

## Logger service shape

The platform logger exposes level-specific methods plus a low-level method that lets you set level/outcome explicitly.

In practice:
- Use the level-specific method for most logs (`InformationAsync`, `WarningAsync`, `ErrorAsync`).
- Use `LogAsync` only when you need to set `Level` and `OUT` explicitly.

## Mapping to stored fields

The logger stores fields into the system log entity.

| Logger parameter | Stored field |
|---|---|
| `evt` | `Evt` (`EVT`) |
| `cat` | `Cat` (`CAT`) |
| `act` | `Act` (`ACT`) |
| `overrideOutcome` / `outcome` | `Out` (`OUT`) |
| `ent` | `Ent` (`ENT`) |
| `entId` | `EntId` |
| `rid` | `Rid` (`RID`) |
| `tid` | `Tid` (`TID`) |
| `uid` | `Uid` (`UID`) |
| `org` | `Org` (`ORG`) |
| `durMs` | `DurMs` |
| `http` | `Http` |
| `stat` | `Stat` |
| `path` | `Path` |
| `note` | `Note` |
| `message` + `ex` | `Message` (combined) |

### Correlation ID rules

- If you are in an HTTP request, `RID` should come from your correlation mechanism (trace id/header).
- If none exists, generate one once and reuse it for the entire request and any spawned jobs.

### Tenant/User rules

- Always set `TID` and `ORG` for tenant-scoped actions.
- Always set `UID` for authenticated actions.
- For background jobs, store the originating `UID` in the job payload so it can be logged later.

## UI traceability implementation sketch (Blazor)

Emit `UI_*` events on deliberate actions:
- On navigation: `UI_PAGE_OPEN`
- On toggling edit mode: `UI_EDIT_MODE_ENABLED`
- On save click: `UI_SAVE_PRESSED`
- On running a command: `UI_FUNCTION_EXECUTED`

Ensure `RID` is the same value used by the API request triggered by that action.
