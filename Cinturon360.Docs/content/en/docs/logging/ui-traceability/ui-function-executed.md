---
title: UI_FUNCTION_EXECUTED
description: Emitted when a user runs a non-CRUD function from the UI (e.g. Test Connection, Reprice Quote).
categories: [platform, logging]
tags: [ui, events]
weight: 10
---

_Last updated: 24 January 2026_

## Event

`EVT=UI_FUNCTION_EXECUTED`

| Field | Value |
|---|---|
| `CAT` | `UI` |
| `ACT` | `EXEC` |
| `OUT` | `OK` |
| `ENT` | `DomainEntity` |

## When to emit

Emitted when a user runs a non-CRUD function from the UI (e.g. Test Connection, Reprice Quote).

## Required fields

- `UID`, `ORG`, `TID`, `RID`, `PATH`
- `ENT` and `EntId` for the target record (where applicable)

## NOTE format

`note` must include:
- `function={FunctionName}`

Optional additions:
- `section={SectionKey}`
- `provider={ProviderName}`

## Examples

```
EVT=UI_FUNCTION_EXECUTED CAT=UI ACT=EXEC OUT=OK ENT=Integration EntId=amadeus RID=… TID=… UID=… ORG=… PATH=/settings/integrations/amadeus NOTE=function=TestConnection
```

## Guidance

- This event is user intent. Provider/API outcomes are logged separately under `INT_*` / `API_*`.
- Use stable function names (PascalCase or UpperSnakeCase) and do not include ids.
