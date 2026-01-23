---
title: UI_PAGE_OPEN
description: Emitted once when a user navigates to a route/page.
categories: [platform, logging]
tags: [ui, events]
weight: 10
---

_Last updated: 24 January 2026_

## Event

`EVT=UI_PAGE_OPEN`

| Field | Value |
|---|---|
| `CAT` | `UI` |
| `ACT` | `VIEW` |
| `OUT` | `OK` |
| `ENT` | `Page` |

## When to emit

Emitted once when a user navigates to a route/page.

## Required fields

- `UID`: authenticated user id (if available)
- `ORG`: organisation id (or active org context)
- `TID`: tenant id
- `RID`: correlation id for this navigation/action chain
- `PATH`: the UI route (recommended: resolved route, not template)
- `message`: optional, short (e.g. page title)

## NOTE format

Use `note` only if there is a meaningful hint, such as:
- `source=menu`
- `source=deeplink`
- `source=redirect`

## Examples

Header-only style:
```
EVT=UI_PAGE_OPEN CAT=UI ACT=VIEW OUT=OK ENT=Page EntId=/companies/xyz RID=… TID=… UID=… ORG=… PATH=/companies/xyz
```

## Guidance

- For initial load, emit after user context is available (so `UID/ORG/TID` are populated).
- Do not emit repeatedly on component re-render; only on navigation.
- If your UI includes tabbed sub-routes, treat each distinct route as a page open.
