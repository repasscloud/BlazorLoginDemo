---
title: Logging
description: Structured logging standard across Web, API, and Workers (multi-tenant, traceable, queryable).
categories: [platform, logging]
tags: [observability, audit, operations]
weight: 5
---

_Last updated: 24 January 2026_

## What this section is

This documentation defines the **single logging contract** used across the platform. It exists so that:
- Developers can emit logs that are consistent and searchable.
- Vendors and TMCs can interpret log entries without guessing what a field means.
- Operators can reconstruct user actions, API calls, workflow execution, and integration failures end-to-end.

## What a log entry represents

A log entry is a **single event**:
- **who** performed an action (`UID`)
- **where** it happened (route/path in `PATH`)
- **which tenant/org** it belongs to (`TID`, `ORG`)
- **what** happened (`EVT`, `CAT`, `ACT`)
- **what the result was** (`OUT`)
- **how to correlate** it with other events (`RID`, optional tracing)

## Quick links

- [Log entry schema and field reference](./schema-and-fields/)
- [Categories, actions, outcomes](./taxonomy/)
- [Standard event families](./event-families/)
- [UI (user action) traceability](./ui-traceability/)
- [API request logging](./api/)
- [Integrations (provider calls)](./integrations/)
- [Jobs and workflows](./jobs-and-workflows/)
- [Security logging](./security/)
- [How to search and correlate logs](./querying-and-correlation/)
- [Implementation guidance (C#)](./implementation/)
