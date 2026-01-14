---
title: UI_EDIT_MODE_ENABLED
description: Emitted when a user explicitly enables edit mode for a page section.
categories: [platform, logging]
tags: [ui, events]
weight: 10
---

## Event

`EVT=UI_EDIT_MODE_ENABLED`

| Field | Value |
|---|---|
| `CAT` | `UI` |
| `ACT` | `UPDATE` |
| `OUT` | `OK` |
| `ENT` | `DomainEntity` |

## When to emit

Emitted when a user explicitly enables edit mode for a page section.

## Required fields

- `UID`, `ORG`, `TID`, `RID`, `PATH`
- `ENT`: entity type being edited (e.g. `Company`)
- `EntId`: entity id (e.g. `xyz`)

## NOTE format

`note` must include the section name using key-value style:

- `section={SectionKey}`

Optional additions:
- `mode=edit`

## Examples

```
EVT=UI_EDIT_MODE_ENABLED CAT=UI ACT=UPDATE OUT=OK ENT=Company EntId=xyz RID=… TID=… UID=… ORG=… PATH=/companies/xyz NOTE=section=billing
```

```csharp
string? pageRoute = $"/platform/organizations/{orgId}/edit";
await _log.InformationAsync(
    evt: "UI_EDIT_MODE_ENABLED",
    cat: Cinturon360.Shared.Models.Static.SysVar.SysLogCatType.Ui,
    act: Cinturon360.Shared.Models.Static.SysVar.SysLogActionType.Click,
    message: $"Org Edit Section clicked: {sectionName}",
    uid: userId,
    org: orgId,
    path: pageRoute
);
```

## Guidance

- Emit when the user enters edit mode, not when validation runs.
- If you support multiple sections, always log the section key.
- If edit mode is blocked by permissions, emit `CAT=SEC` with `OUT=DENY` (see security section) and optionally a `UI_DENY` event for UI feedback.
